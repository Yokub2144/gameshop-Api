FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# ตั้ง working directory
WORKDIR /app

# คัดลอกไฟล์โปรเจกต์
COPY . .

# ติดตั้ง dependencies และ build โปรเจกต์
RUN dotnet restore
RUN dotnet build -c Release

# Publish แอป
RUN dotnet publish -c Release -o out

# ใช้ runtime image สำหรับรันแอป
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# ตั้ง working directory
WORKDIR /app

# คัดลอกไฟล์ที่ publish มา
COPY --from=build /app/out .

# ตั้งค่า environment variable สำหรับการรันแอป
ENV ASPNETCORE_URLS=http://+:5000

# รันแอป
ENTRYPOINT ["dotnet", "gameshopApi.dll"]