
using System.Collections.Concurrent; // Cần cho ConcurrentBag
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using cellphones_backend.Data;
using cellphones_backend.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace cellPhoneS_backend.Services.Implement
{
    public class UploadImage2Cloud
    {
        // --- CẤU HÌNH ---
        private readonly string r2AccessKey;
        private readonly string r2SecretKey;
        private readonly string r2AccountId;
        private readonly string bucketName = "cellphone-s-image";
        private readonly string r2PublicDomain;

        private readonly string r2ServiceUrl;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public UploadImage2Cloud(ApplicationDbContext context, IConfiguration configuration)
        {
            _dbContext = context;
            _configuration = configuration;
            r2AccessKey = _configuration["ImageHosting:Cloudflare:AccessKey"]!;
            r2SecretKey = _configuration["ImageHosting:Cloudflare:SecretKey"]!;
            r2AccountId = _configuration["ImageHosting:Cloudflare:AccountId"]!;
            r2PublicDomain = _configuration["ImageHosting:Cloudflare:PublicDomain"]!;
            r2ServiceUrl = $"https://{r2AccountId}.r2.cloudflarestorage.com";
        }

        public async Task MigrateFromLocalFolderAsync(string localFolderPath)
        {
            // 1. Setup AWS S3 Client
            var credentials = new BasicAWSCredentials(r2AccessKey, r2SecretKey);
            var config = new AmazonS3Config
            {
                ServiceURL = r2ServiceUrl,
                ForcePathStyle = true
            };

            using var s3Client = new AmazonS3Client(credentials, config);
            using var fileTransferUtility = new TransferUtility(s3Client);
            var contentTypeProvider = new FileExtensionContentTypeProvider();

            // 2. Load dữ liệu DB (Load hết để map cho dễ, nhưng sẽ update theo batch)
            Console.WriteLine("Đang đọc dữ liệu bảng Image...");

            // Tắt Tracking ban đầu để tiết kiệm RAM, ta sẽ Attach lại khi cần update
            var images = await _dbContext.Images
                .AsNoTracking()
                .Where(i =>
                            i.BlobUrl != null && i.BlobUrl.Contains("cloudinary.com"))
                .ToListAsync();

            if (images.Count == 0)
            {
                Console.WriteLine("Không tìm thấy ảnh nào cần migrate.");
                return;
            }

            var imageMap = new Dictionary<string, List<long>>(StringComparer.OrdinalIgnoreCase);

            foreach (var img in images)
            {
                // Logic lấy tên file
                string urlToParse = img.BlobUrl ?? "";
                string fileName = GetFileNameFromUrl(urlToParse); // Hàm này ở dưới cùng

                if (!string.IsNullOrEmpty(fileName))
                {
                    if (!imageMap.ContainsKey(fileName)) imageMap[fileName] = new List<long>();
                    imageMap[fileName].Add(img.Id);
                }
            }

            Console.WriteLine($"Database: Load được {imageMap.Count} tên file duy nhất.");

            // -----------------------------------------------------------------------
            // 2. THÊM: ĐOẠN DEBUG "VẠCH TRẦN SỰ THẬT" (Chạy xong xóa cũng được)
            // -----------------------------------------------------------------------
            Console.WriteLine("\n🔴 --- DEBUG CHECK (So sánh 10 mẫu đầu tiên) ---");
            Console.WriteLine("Tên file máy tính ĐANG CHỜ trong Dictionary (từ DB):");
            foreach (var key in imageMap.Keys.Take(10))
            {
                Console.WriteLine($"   - '{key}'"); // Dấu nháy đơn để soi khoảng trắng thừa
            }

            Console.WriteLine("\nTên file máy tính TÌM THẤY trong Folder Local:");
            var sampleFiles = Directory.GetFiles(localFolderPath, "*.*", SearchOption.AllDirectories).Take(10);
            foreach (var path in sampleFiles)
            {
                Console.WriteLine($"   - '{Path.GetFileName(path)}'");
            }
            Console.WriteLine("🔴 ---------------------------------------------\n");
            // -----------------------------------------------------------------------

            if (!Directory.Exists(localFolderPath))
            {
                Console.WriteLine($"Thư mục không tồn tại: {localFolderPath}");
                return;
            }
            var localFiles = Directory.GetFiles(localFolderPath, "*.*", SearchOption.AllDirectories);
            Console.WriteLine($"Tìm thấy {localFiles.Length} files trong folder local (bao gồm cả folder con).");
            // 3. CHIA BATCH ĐỂ XỬ LÝ (Mỗi lần 50 file để an toàn Transaction)
            int batchSize = 50;
            var fileBatches = localFiles.Chunk(batchSize); // .NET 6 trở lên có hàm Chunk

            int totalSuccess = 0;
            int totalError = 0;

            foreach (var batch in fileBatches)
            {
                // -- BƯỚC 3.1: Upload song song (Không dính dáng gì tới DB context ở đây để tránh lỗi Thread) --
                var successfulUploads = new ConcurrentBag<(string FileName, string MimeType)>();

                await Parallel.ForEachAsync(batch, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (filePath, token) =>
                {
                    try
                    {
                        string fileName = Path.GetFileName(filePath);
                        // Chỉ upload nếu file này có người dùng trong DB
                        if (imageMap.ContainsKey(fileName))
                        {
                            if (!contentTypeProvider.TryGetContentType(filePath, out string mimeType))
                                mimeType = "application/octet-stream";

                            var uploadRequest = new TransferUtilityUploadRequest
                            {
                                InputStream = File.OpenRead(filePath),
                                Key = fileName,
                                BucketName = bucketName,
                                ContentType = mimeType,
                                DisablePayloadSigning = true
                            };

                            await fileTransferUtility.UploadAsync(uploadRequest);

                            // Ghi nhận upload thành công
                            successfulUploads.Add((fileName, mimeType));
                            Console.WriteLine($"[R2 Uploaded] {fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref totalError);
                        Console.WriteLine($"[R2 Error] {filePath}: {ex.Message}");
                    }
                });

                // -- BƯỚC 3.2: Transaction Update DB (Làm tuần tự trên Main Thread) --
                if (successfulUploads.IsEmpty) continue;

                // Bắt đầu Transaction cho Batch này
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Lấy danh sách ID cần update từ Map
                    var idsToUpdate = new List<long>();
                    var fileInfoDict = new Dictionary<long, (string FileName, string MimeType)>();

                    foreach (var item in successfulUploads)
                    {
                        if (imageMap.TryGetValue(item.FileName, out var ids))
                        {
                            idsToUpdate.AddRange(ids);
                            foreach (var id in ids)
                            {
                                fileInfoDict[id] = item; // Lưu lại để tí nữa gán thông tin
                            }
                        }
                    }

                    // Query lại các Entity này từ DB để tracking và update
                    var entitiesToUpdate = await _dbContext.Images
                        .Where(i => idsToUpdate.Contains(i.Id))
                        .ToListAsync();

                    foreach (var imgEntity in entitiesToUpdate)
                    {
                        if (fileInfoDict.TryGetValue(imgEntity.Id, out var info))
                        {
                            // CẬP NHẬT DỮ LIỆU
                            string newUrl = $"{r2PublicDomain}/{info.FileName}";

                            imgEntity.BlobUrl = newUrl;      // Chỉ sửa BlobUrl
                            imgEntity.MimeType = info.MimeType; // Sửa MimeType
                            imgEntity.UpdateDate = DateTime.UtcNow;
                            // OriginUrl giữ nguyên
                        }
                    }

                    // Lưu xuống DB
                    await _dbContext.SaveChangesAsync();

                    // Commit Transaction (Chốt đơn)
                    await transaction.CommitAsync();

                    totalSuccess += entitiesToUpdate.Count;
                    Console.WriteLine($"--- [DB COMMIT] Đã lưu batch {entitiesToUpdate.Count} items ---");
                }
                catch (Exception ex)
                {
                    // Nếu lỗi DB -> Rollback (Upload R2 vẫn còn đó, lần sau chạy lại sẽ ghi đè hoặc bỏ qua)
                    await transaction.RollbackAsync();
                    Console.WriteLine($"[DB TRANSACTION ERROR] Rollback batch này. Lỗi: {ex.Message}");
                    totalError += successfulUploads.Count; // Tính là lỗi
                }

                // Clear ChangeTracker để giải phóng RAM cho batch sau
                _dbContext.ChangeTracker.Clear();
            }

            Console.WriteLine($"HOÀN TẤT TOÀN BỘ! BlobUrl đã update: {totalSuccess}, Lỗi: {totalError}");
        }

        private string GetFileNameFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            try
            {
                var uri = new Uri(url);
                // Lấy tên file
                string fileName = Path.GetFileName(uri.LocalPath);
                // QUAN TRỌNG: Giải mã URL (ví dụ %20 thành dấu cách)
                return Uri.UnescapeDataString(fileName);
            }
            catch
            {
                return "";
            }
        }
    }
}