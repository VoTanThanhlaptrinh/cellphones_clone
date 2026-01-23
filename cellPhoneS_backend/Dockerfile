# --- Stage 1: Build (Dùng SDK 9.0) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file csproj và restore
# Lưu ý: Docker cache dựa trên file này, nên copy riêng nó trước là Best Practice
COPY ["*.csproj", "./"]
RUN dotnet restore

# Copy toàn bộ code và build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# --- Stage 2: Runtime (Dùng Runtime 9.0 - Nhẹ hơn SDK rất nhiều) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy kết quả build từ Stage 1
COPY --from=build /app/publish .

# --- CẤU HÌNH CỔNG (PORT) ---
# Từ .NET 8 trở lên (bao gồm .NET 9), biến môi trường đổi thành ASPNETCORE_HTTP_PORTS
# Render sẽ map port này ra ngoài internet
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# --- CHẠY APP ---
# QUAN TRỌNG: Hãy vào thư mục bin/Debug/net9.0 xem file .dll tên chính xác là gì
# Thường nó sẽ là tên project của bạn. Ví dụ: cellPhoneS_backend.dll
ENTRYPOINT ["dotnet", "cellPhoneS_backend.dll"]