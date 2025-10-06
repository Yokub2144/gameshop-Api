# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .

# restore dependencies
RUN dotnet restore

# publish project
RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# copy publish output
COPY --from=build /app/out .

# set ASP.NET Core URL
ENV ASPNETCORE_URLS=http://+:5000

# run the app
ENTRYPOINT ["dotnet", "Gameshop-Api.dll"]
