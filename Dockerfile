FROM mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /app
COPY . .
RUN dotnet restore
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:5000"]