# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder
WORKDIR /build
COPY AlbumSync/ .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine
WORKDIR /app
COPY --from=builder /app .
COPY appsettings.json /app/
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Set sensible defaults; can be overridden in compose or docker run
ENV SYNC_CRON="0 * * * *" \
	SYNC_ARGS=""

ENTRYPOINT ["/entrypoint.sh"]
