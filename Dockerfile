# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY BEx.Web/BEx.Web.csproj ./BEx.Web/
COPY BEx.Core/BEx.Core.csproj ./BEx.Core/
COPY Backlog_Expedition/Backlog_Expedition.csproj ./Backlog_Expedition/

# Copy all source code
COPY BEx.Web/ ./BEx.Web/
COPY BEx.Core/ ./BEx.Core/
COPY Backlog_Expedition/ ./Backlog_Expedition/

# Build and publish
RUN dotnet publish ./BEx.Web/BEx.Web.csproj -c Release -o /app/publish --self-contained false


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Create non-root user for security (before copying files)
RUN useradd -m -u 1000 appuser

# Copy published application from builder
COPY --from=builder --chown=appuser:appuser /app/publish .

USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/ || exit 1

# Run application
ENTRYPOINT ["dotnet", "BEx.Web.dll"]
