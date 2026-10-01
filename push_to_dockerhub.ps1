# ==============================================================================
# Script đóng gói và đẩy Backend & DB lên Docker Hub (quangcode153)
# ==============================================================================

Write-Host ">>> [1/4] Đăng nhập Docker Hub với tài khoản 'quangcode153'..." -ForegroundColor Cyan
docker login -u quangcode153

if ($LASTEXITCODE -ne 0) {
    Write-Host "Đăng nhập thất bại hoặc bị hủy. Vui lòng kiểm tra Docker Desktop và thử lại!" -ForegroundColor Red
    exit 1
}

Write-Host "`n>>> [2/4] Đang biên dịch Docker Image cho Backend WebApi..." -ForegroundColor Cyan
docker build -t quangcode153/sportchain-backend:latest -f src/backend/SportChain.WebApi/Dockerfile .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build Backend thất bại!" -ForegroundColor Red
    exit 1
}

Write-Host "`n>>> [3/4] Đang đẩy Backend Image lên Docker Hub..." -ForegroundColor Cyan
docker push quangcode153/sportchain-backend:latest

Write-Host "`n>>> [4/4] Đang chuẩn bị và đẩy SQL Server 2022 Image lên Docker Hub..." -ForegroundColor Cyan
docker pull mcr.microsoft.com/mssql/server:2022-latest
docker tag mcr.microsoft.com/mssql/server:2022-latest quangcode153/sportchain-sqlserver:latest
docker push quangcode153/sportchain-sqlserver:latest

Write-Host "`n=== HOÀN TẤT ĐẨY IMAGE LÊN DOCKER HUB THÀNH CÔNG! ===" -ForegroundColor Green
Write-Host "1. quangcode153/sportchain-backend:latest" -ForegroundColor Yellow
Write-Host "2. quangcode153/sportchain-sqlserver:latest" -ForegroundColor Yellow
