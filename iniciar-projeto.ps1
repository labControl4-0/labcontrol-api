# Script de Inicializacao Rapida - LabControl API (.NET 9 + PostgreSQL)
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "       Iniciando LabControl API e Banco de Dados          " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Verificar se o Docker Desktop esta aberto
Write-Host "`n[1/4] Verificando Docker Desktop..." -ForegroundColor Yellow
try {
    $dockerCheck = docker ps 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERRO: O Docker Desktop nao parece estar aberto ou com o daemon ativo." -ForegroundColor Red
        Write-Host "Por favor, abra o aplicativo Docker Desktop e aguarde a luz verde antes de rodar este script." -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "ERRO: Falha ao executar comando docker. Verifique se o Docker esta instalado." -ForegroundColor Red
    exit 1
}

# 2. Verificar e subir o container do PostgreSQL
Write-Host "[2/4] Verificando container do PostgreSQL (labcontrol_db)..." -ForegroundColor Yellow
$containerExists = docker ps -a --filter "name=labcontrol_db" --format "{{.Names}}"

if ($containerExists -eq "labcontrol_db") {
    $isRunning = docker ps --filter "name=labcontrol_db" --format "{{.Names}}"
    if ($isRunning -ne "labcontrol_db") {
        Write-Host "Iniciando container existente labcontrol_db..." -ForegroundColor Green
        docker start labcontrol_db | Out-Null
    } else {
        Write-Host "Container labcontrol_db ja esta em execucao." -ForegroundColor Green
    }
} else {
    Write-Host "Criando e iniciando novo container PostgreSQL (labcontrol_db)..." -ForegroundColor Green
    docker run --name labcontrol_db `
        -e POSTGRES_DB=labcontroldb `
        -e POSTGRES_USER=admin `
        -e POSTGRES_PASSWORD=admin `
        -p 5432:5432 `
        -d postgres:15 | Out-Null
}

Write-Host "Aguardando inicializacao do banco de dados (3 segundos)..." -ForegroundColor Gray
Start-Sleep -Seconds 3

# 3. Compilar projeto caso necessario
Write-Host "[3/4] Verificando compilacao (.NET 9)..." -ForegroundColor Yellow
if (-not (Test-Path "bin\Debug\net9.0\LabControlApi.dll")) {
    Write-Host "Compilando projeto..." -ForegroundColor Gray
    dotnet build --no-restore
}

# 4. Abrir Swagger e rodar API via DLL oficial (evitando bloqueio de AppLocker do Windows)
Write-Host "[4/4] Iniciando API e abrindo Swagger no navegador..." -ForegroundColor Yellow
Start-Process "http://localhost:5071/swagger"

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " API iniciada com sucesso!" -ForegroundColor Green
Write-Host " Swagger URL: http://localhost:5071/swagger" -ForegroundColor Cyan
Write-Host " Pressione Ctrl + C no terminal para encerrar a API." -ForegroundColor Gray
Write-Host "==========================================================`n" -ForegroundColor Green

# Executa via DLL com as configuracoes de ambiente de desenvolvimento
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5071"

dotnet bin\Debug\net9.0\LabControlApi.dll

