# Sử dụng runtime image chính thức của ASP.NET Core 8.0 để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Khai báo port sẽ lắng nghe trong container (mặc định ASP.NET Core 8.0 lắng nghe port 8080)
EXPOSE 8080

# Sử dụng SDK image chính thức của .NET 8.0 để build mã nguồn
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy các file dự án .csproj để khôi phục (restore) các package NuGet trước
# Điều này giúp tận dụng bộ nhớ đệm (caching) của Docker, tăng tốc build lần sau
COPY ["PipeVolt_Api/PipeVolt_Api.csproj", "PipeVolt_Api/"]
COPY ["PipeVolt_BLL/PipeVolt_BLL.csproj", "PipeVolt_BLL/"]
COPY ["PipeVolt_DAL/PipeVolt_DAL.csproj", "PipeVolt_DAL/"]
RUN dotnet restore "PipeVolt_Api/PipeVolt_Api.csproj"

# Copy toàn bộ mã nguồn còn lại của Backend vào container
COPY . .

# Biên dịch ứng dụng ở chế độ Release
WORKDIR "/src/PipeVolt_Api"
RUN dotnet build "PipeVolt_Api.csproj" -c Release -o /app/build

# Publish ứng dụng để xuất ra các file binary chạy được
FROM build AS publish
RUN dotnet publish "PipeVolt_Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Tạo container chạy chính thức (sản phẩm cuối cùng gọn nhẹ)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PipeVolt_Api.dll"]
