# ========== BUILD STAGE ==========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy toàn bộ project vào container (nếu project nhỏ là OK)
COPY . . 

# Restore đầy đủ
RUN dotnet restore "SWD392-backend.csproj"

# Build project
RUN dotnet publish "SWD392-backend.csproj" -c Release -o /app/publish

# ========== RUNTIME STAGE ==========
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
COPY .env .env

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "SWD392-backend.dll"]
