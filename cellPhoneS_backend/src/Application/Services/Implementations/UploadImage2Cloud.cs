
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
                            i.BlobUrl != null && i.BlobUrl.Contains("cloudinary"))
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
                string fullFileName = GetFileNameFromUrl(urlToParse); // Hàm này ở dưới cùng

                if (!string.IsNullOrEmpty(fullFileName))
                {
                    // QUAN TRỌNG 1: Cắt bỏ đuôi file để làm key so sánh
                    string nameOnly = Path.GetFileNameWithoutExtension(fullFileName);

                    if (!imageMap.ContainsKey(nameOnly)) imageMap[nameOnly] = new List<long>();
                    imageMap[nameOnly].Add(img.Id);
                }
            }

            Console.WriteLine($"Database: Load được {imageMap.Count} tên file duy nhất (không tính đuôi).");

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
                // QUAN TRỌNG 2: Lưu thêm trường NameWithoutExtension để tí nữa tìm lại ID
                var successfulUploads = new ConcurrentBag<(string LocalFileName, string MimeType, string NameWithoutExtension)>();

                await Parallel.ForEachAsync(batch, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (filePath, token) =>
                {
                    try
                    {
                        string localFileName = Path.GetFileName(filePath);
                        // QUAN TRỌNG 3: Cắt bỏ đuôi file dưới máy tính để đem đi dò tìm
                        string localNameOnly = Path.GetFileNameWithoutExtension(localFileName);

                        // Chỉ upload nếu TÊN KHÔNG ĐUÔI này có trong DB
                        if (imageMap.ContainsKey(localNameOnly))
                        {
                            if (!contentTypeProvider.TryGetContentType(filePath, out string mimeType))
                                mimeType = "application/octet-stream";

                            var uploadRequest = new TransferUtilityUploadRequest
                            {
                                InputStream = File.OpenRead(filePath),
                                Key = localFileName, // Vẫn upload lên R2 bằng tên có đuôi thật
                                BucketName = bucketName,
                                ContentType = mimeType,
                                DisablePayloadSigning = true
                            };

                            await fileTransferUtility.UploadAsync(uploadRequest);

                            // Ghi nhận upload thành công (lưu cả 3 thông tin)
                            successfulUploads.Add((localFileName, mimeType, localNameOnly));
                            Console.WriteLine($"[R2 Uploaded] {localFileName}");
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
                    var fileInfoDict = new Dictionary<long, (string LocalFileName, string MimeType)>();

                    foreach (var item in successfulUploads)
                    {
                        // QUAN TRỌNG 4: Tìm ID dựa vào tên không đuôi
                        if (imageMap.TryGetValue(item.NameWithoutExtension, out var ids))
                        {
                            idsToUpdate.AddRange(ids);
                            foreach (var id in ids)
                            {
                                fileInfoDict[id] = (item.LocalFileName, item.MimeType);
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
                            // QUAN TRỌNG 5: URL mới sử dụng tên CÓ ĐUÔI THẬT của file dưới máy tính
                            string newUrl = $"{r2PublicDomain}/{info.LocalFileName}";

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
        public async Task MigrateCloudinaryToCloudflareAsync()
        {
            Console.WriteLine("--- BẮT ĐẦU QUÁ TRÌNH MIGRATE ---");

            // 1. FIX: Ép kiểu ToLower() để tránh lỗi phân biệt chữ hoa chữ thường
            var imagesToMigrate = await _dbContext.Images
                .Where(i => i.BlobUrl != null && i.BlobUrl.ToLower().Contains("cloudinary"))
                .ToListAsync();

            if (!imagesToMigrate.Any())
            {
                Console.WriteLine("⚠️ Không tìm thấy ảnh nào có chứa từ 'cloudinary' trong BlobUrl.");
                return;
            }

            // 2. Khởi tạo S3 Client
            var credentials = new BasicAWSCredentials(r2AccessKey, r2SecretKey);
            var config = new AmazonS3Config { ServiceURL = r2ServiceUrl };

            using var s3Client = new AmazonS3Client(credentials, config);
            using var httpClient = new HttpClient();

            int successCount = 0;
            int errorCount = 0;

            Console.WriteLine($"Tìm thấy {imagesToMigrate.Count} hình ảnh. Bắt đầu xử lý...");

            foreach (var img in imagesToMigrate)
            {
                try
                {
                    // 3. FIX: Nếu OriginUrl rỗng, lấy chính BlobUrl (chứa link cloudinary) để tải về
                    string downloadUrl = !string.IsNullOrEmpty(img.OriginUrl) ? img.OriginUrl : img.BlobUrl;

                    if (string.IsNullOrEmpty(downloadUrl))
                    {
                        Console.WriteLine($"[Bỏ qua] Ảnh ID {img.Id}: Cả OriginUrl và BlobUrl đều rỗng.");
                        continue;
                    }

                    Console.WriteLine($"[Đang tải] ID {img.Id} từ: {downloadUrl}");

                    // Tải ảnh về
                    using var imageResponse = await httpClient.GetAsync(downloadUrl);
                    imageResponse.EnsureSuccessStatusCode();
                    using var imageStream = await imageResponse.Content.ReadAsStreamAsync();

                    string fileExtension = GetExtensionFromMimeType(img.MimeType);
                    string objectKey = $"migrated_images/{img.Id}_{Guid.NewGuid().ToString("N")[..8]}{fileExtension}";

                    var putRequest = new Amazon.S3.Model.PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = objectKey,
                        InputStream = imageStream,
                        ContentType = img.MimeType,
                        DisablePayloadSigning = true
                    };

                    await s3Client.PutObjectAsync(putRequest);

                    // 4. FIX: Cập nhật thông tin (KHÔNG sửa trường UpdateBy để tránh lỗi Foreign Key)
                    img.BlobUrl = $"{r2PublicDomain}/{objectKey}";
                    img.UpdateDate = DateTime.UtcNow;

                    successCount++;
                    Console.WriteLine($"✅ [Thành công] ID {img.Id} -> {img.BlobUrl}");

                    // Lưu theo lô 50 ảnh
                    if (successCount % 50 == 0)
                    {
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"💾 Đã lưu DB {successCount} ảnh...");
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    errorCount++;
                    Console.WriteLine($"❌ [Lỗi Database] ID {img.Id}: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"❌ [Lỗi Tải/Upload] ID {img.Id}: {ex.Message}");
                }
            }

            // Lưu những ảnh lẻ còn lại ở cuối vòng lặp
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [Lỗi lưu DB cuối cùng]: {ex.InnerException?.Message ?? ex.Message}");
            }

            Console.WriteLine($"--- HOÀN TẤT! Thành công: {successCount}, Lỗi: {errorCount} ---");
        }

        // Hàm phụ trợ lấy đuôi file từ MimeType (bạn có thể tự điều chỉnh theo MimeType thực tế đang lưu)
        private string GetExtensionFromMimeType(string mimeType)
        {
            return mimeType switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => ".jpg" // Mặc định
            };
        }
    }

}