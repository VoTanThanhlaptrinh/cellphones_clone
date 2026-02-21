
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
        public async Task CleanAndUploadPipelineAsync(string localFolderPath)
        {
            Console.WriteLine("=== BƯỚC 1: LẤY TOÀN BỘ DỮ LIỆU TỪ DATABASE ===");

            // 1. Lấy URL từ bảng Images (Chỉ cần có chứa link ảnh là lấy)
            var imageUrls = await _dbContext.Images
                .AsNoTracking()
                .Where(i => i.BlobUrl != null)
                .Select(i => i.BlobUrl)
                .ToListAsync();

            // 2. Lấy URL từ bảng Products (Chỉ cần có chứa link ảnh là lấy)
            var productUrls = await _dbContext.Products
                .AsNoTracking()
                .Where(p => p.ImageUrl != null)
                .Select(p => p.ImageUrl)
                .ToListAsync();

            // 3. Gộp lại và tạo danh sách TÊN FILE HỢP LỆ (Bỏ đuôi .webp, .png...)
            var validNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var url in imageUrls.Concat(productUrls))
            {
                string fullFileName = GetFileNameFromUrl(url!);
                if (!string.IsNullOrEmpty(fullFileName))
                {
                    validNames.Add(Path.GetFileNameWithoutExtension(fullFileName));
                }
            }

            Console.WriteLine($"=> Tổng hợp được {validNames.Count} tên file hợp lệ cần giữ lại.");

            Console.WriteLine("\n=== BƯỚC 2: DỌN DẸP THƯ MỤC LOCAL ===");
            if (!Directory.Exists(localFolderPath))
            {
                Console.WriteLine($"[Lỗi] Không tìm thấy thư mục: {localFolderPath}");
                return;
            }

            var localFiles = Directory.GetFiles(localFolderPath, "*.*", SearchOption.AllDirectories);
            int deleteCount = 0;

            foreach (var filePath in localFiles)
            {
                string localNameOnly = Path.GetFileNameWithoutExtension(filePath);

                // Nếu file dưới máy tính KHÔNG nằm trong danh sách DB -> Tiêu diệt
                if (!validNames.Contains(localNameOnly))
                {
                    File.Delete(filePath);
                    deleteCount++;
                }
            }
            Console.WriteLine($"=> Đã xóa {deleteCount} file rác.");
            Console.WriteLine($"=> Giữ lại {localFiles.Length - deleteCount} file chuẩn bị upload.");

            Console.WriteLine("\n=== BƯỚC 3: UPLOAD LÊN CLOUDFLARE R2 ===");
            // Tới đây folder của bạn đã sạch 100%, gọi lại hàm Upload 
            await UploadMissingFilesToR2Async(localFolderPath);

            Console.WriteLine("\n=== HOÀN TẤT CHIẾN DỊCH! ===");
        }
        public async Task UploadMissingFilesToR2Async(string localFolderPath)
        {
            // 1. Setup AWS S3 Client (Đúng chuẩn cấu trúc của bạn)
            var credentials = new BasicAWSCredentials(r2AccessKey, r2SecretKey);
            var config = new AmazonS3Config
            {
                ServiceURL = r2ServiceUrl,
                ForcePathStyle = true
            };

            using var s3Client = new AmazonS3Client(credentials, config);
            using var fileTransferUtility = new TransferUtility(s3Client);
            var contentTypeProvider = new FileExtensionContentTypeProvider();

            if (!Directory.Exists(localFolderPath))
            {
                Console.WriteLine($"Thư mục không tồn tại: {localFolderPath}");
                return;
            }

            // 2. Lấy danh sách các file ĐÃ TỒN TẠI trên R2 để loại trừ
            Console.WriteLine("Đang lấy danh sách file trên Cloudflare R2...");
            var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string? continuationToken = null;

            do
            {
                var request = new Amazon.S3.Model.ListObjectsV2Request
                {
                    BucketName = bucketName,
                    ContinuationToken = continuationToken
                };

                var response = await s3Client.ListObjectsV2Async(request);

                foreach (var obj in response.S3Objects)
                {
                    existingKeys.Add(obj.Key); // Lưu lại tên file
                }
                continuationToken = response.NextContinuationToken;

            } while (!string.IsNullOrEmpty(continuationToken));

            Console.WriteLine($"R2: Đã load được {existingKeys.Count} file.");

            // 3. Quét folder local và lọc ra những file CHƯA CÓ trên R2
            var localFiles = Directory.GetFiles(localFolderPath, "*.*", SearchOption.AllDirectories);
            var filesToUpload = new List<string>();

            foreach (var filePath in localFiles)
            {
                string fileName = Path.GetFileName(filePath);
                // Nếu tên file dưới máy tính chưa có trong HashSet của R2 -> Thêm vào list cần upload
                if (!existingKeys.Contains(fileName))
                {
                    filesToUpload.Add(filePath);
                }
            }

            Console.WriteLine($"Tìm thấy {localFiles.Length} files trong folder local (bao gồm cả folder con).");
            Console.WriteLine($"Cần upload mới: {filesToUpload.Count} files. Sẽ bỏ qua: {localFiles.Length - filesToUpload.Count} files trùng.");

            if (filesToUpload.Count == 0)
            {
                Console.WriteLine("Tuyệt vời! Tất cả ảnh đã có mặt trên R2. Không cần upload thêm.");
                return;
            }

            // 4. CHIA BATCH ĐỂ XỬ LÝ UPLOAD (Sử dụng luồng như code của bạn)
            int batchSize = 50;
            var fileBatches = filesToUpload.Chunk(batchSize);

            int totalSuccess = 0;
            int totalError = 0;

            foreach (var batch in fileBatches)
            {
                await Parallel.ForEachAsync(batch, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (filePath, token) =>
                {
                    try
                    {
                        string localFileName = Path.GetFileName(filePath);

                        if (!contentTypeProvider.TryGetContentType(filePath, out string mimeType))
                            mimeType = "application/octet-stream";

                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = File.OpenRead(filePath),
                            Key = localFileName,
                            BucketName = bucketName,
                            ContentType = mimeType,
                            DisablePayloadSigning = true
                        };

                        await fileTransferUtility.UploadAsync(uploadRequest);

                        Interlocked.Increment(ref totalSuccess);
                        Console.WriteLine($"[R2 Uploaded] {localFileName}");
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref totalError);
                        Console.WriteLine($"[R2 Error] {filePath}: {ex.Message}");
                    }
                });
            }

            Console.WriteLine($"HOÀN TẤT UPLOAD TOÀN BỘ! Thành công: {totalSuccess}, Lỗi: {totalError}");
        }
    }
}