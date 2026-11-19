FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos de projeto e restaura dependências
COPY AlertaComApi.Domain/AlertaComApi.Domain.csproj AlertaComApi.Domain/
COPY AlertaComApi.Application/AlertaComApi.Application.csproj AlertaComApi.Application/
COPY AlertaComApi.Infrastructure/AlertaComApi.Infrastructure.csproj AlertaComApi.Infrastructure/
COPY AlertaComApi/AlertaComApi.csproj AlertaComApi/
RUN dotnet restore AlertaComApi/AlertaComApi.csproj

# Copia o restante do código-fonte e publica
COPY . .
WORKDIR /src/AlertaComApi
RUN dotnet publish AlertaComApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime leve
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["dotnet", "AlertaComApi.dll"]
