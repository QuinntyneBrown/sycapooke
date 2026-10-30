# Builds the three backend services from one restore. Build context: src/backend.
#   docker build -f ../../deploy/backend.Dockerfile --target api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props global.json Sycapooke.sln ./
COPY Sycapooke.Domain/Sycapooke.Domain.csproj Sycapooke.Domain/
COPY Sycapooke.Application/Sycapooke.Application.csproj Sycapooke.Application/
COPY Sycapooke.Infrastructure/Sycapooke.Infrastructure.csproj Sycapooke.Infrastructure/
COPY Sycapooke.Api/Sycapooke.Api.csproj Sycapooke.Api/
COPY Sycapooke.Worker/Sycapooke.Worker.csproj Sycapooke.Worker/
COPY Sycapooke.Migrator/Sycapooke.Migrator.csproj Sycapooke.Migrator/
RUN dotnet restore Sycapooke.Api/Sycapooke.Api.csproj \
 && dotnet restore Sycapooke.Worker/Sycapooke.Worker.csproj \
 && dotnet restore Sycapooke.Migrator/Sycapooke.Migrator.csproj
COPY . .
RUN dotnet publish Sycapooke.Api/Sycapooke.Api.csproj -c Release --no-restore -o /out/api \
 && dotnet publish Sycapooke.Worker/Sycapooke.Worker.csproj -c Release --no-restore -o /out/worker \
 && dotnet publish Sycapooke.Migrator/Sycapooke.Migrator.csproj -c Release --no-restore -o /out/migrator

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
COPY --from=build /out/api .
RUN mkdir -p /data/photos && chown app:app /data/photos
ENV ASPNETCORE_URLS=http://+:8080 DOTNET_ENVIRONMENT=Production
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Sycapooke.Api.dll"]

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS worker
WORKDIR /app
COPY --from=build /out/worker .
RUN mkdir -p /data/photos && chown app:app /data/photos
ENV DOTNET_ENVIRONMENT=Production
USER app
ENTRYPOINT ["dotnet", "Sycapooke.Worker.dll"]

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS migrator
WORKDIR /app
COPY --from=build /out/migrator .
USER app
ENTRYPOINT ["dotnet", "Sycapooke.Migrator.dll"]
