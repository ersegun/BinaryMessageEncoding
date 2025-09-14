# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY BinaryMessageEncodingAPI/BinaryMessageEncodingAPI.csproj BinaryMessageEncodingAPI/
COPY BinaryMessageEncodingAPI.nUnitTest/BinaryMessageEncodingAPI.nUnitTest.csproj BinaryMessageEncodingAPI.nUnitTest/
RUN dotnet restore BinaryMessageEncodingAPI/BinaryMessageEncodingAPI.csproj

# copy everything else and build
COPY . .
WORKDIR /src/BinaryMessageEncodingAPI
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BinaryMessageEncodingAPI.dll"]
