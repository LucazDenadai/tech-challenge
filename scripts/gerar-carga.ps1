#!/usr/bin/env pwsh
# Gera carga continua nas APIs para popular os dashboards do Grafana.
# Uso: .\scripts\gerar-carga.ps1 [-DurationSeconds 120] [-DelayMs 200]

param(
    [int]$DurationSeconds = 120,
    [int]$DelayMs = 200
)

$atendimentoBase = "http://localhost:30080"
$estoqueBase     = "http://localhost:30081"

Write-Host "Fazendo login no Atendimento..." -ForegroundColor Cyan
$loginBody = '{"email":"admin@oficina.com","senha":"Admin@123"}'
try {
    $loginResp = Invoke-RestMethod -Method Post -Uri "$atendimentoBase/auth/login" `
        -ContentType "application/json" -Body $loginBody -ErrorAction Stop
    $token = $loginResp.token
    Write-Host "Login ok. Token obtido." -ForegroundColor Green
} catch {
    Write-Host "Falha no login: $_" -ForegroundColor Red
    exit 1
}

$headers = @{ Authorization = "Bearer $token" }

$endpointsAtendimento = @(
    "/clientes",
    "/veiculos",
    "/ordens-servico",
    "/servicos"
)

$endpointsEstoque = @(
    "/estoque/pecas"
)

$deadline  = (Get-Date).AddSeconds($DurationSeconds)
$iteration = 0

Write-Host ""
Write-Host "Gerando carga por $DurationSeconds segundos (Ctrl+C para parar antes)..." -ForegroundColor Yellow
Write-Host ""

while ((Get-Date) -lt $deadline) {
    $iteration++

    foreach ($path in $endpointsAtendimento) {
        try {
            $resp = Invoke-WebRequest -Method Get -Uri "$atendimentoBase$path" `
                -Headers $headers -UseBasicParsing -ErrorAction SilentlyContinue
            Write-Host "[#$iteration] atendimento$path -> $($resp.StatusCode)" -ForegroundColor Green
        } catch {
            $code = $_.Exception.Response.StatusCode.value__
            Write-Host "[#$iteration] atendimento$path -> $code" -ForegroundColor Red
        }
        Start-Sleep -Milliseconds $DelayMs
    }

    foreach ($path in $endpointsEstoque) {
        try {
            $resp = Invoke-WebRequest -Method Get -Uri "$estoqueBase$path" `
                -UseBasicParsing -ErrorAction SilentlyContinue
            Write-Host "[#$iteration] estoque$path -> $($resp.StatusCode)" -ForegroundColor Cyan
        } catch {
            $code = $_.Exception.Response.StatusCode.value__
            Write-Host "[#$iteration] estoque$path -> $code" -ForegroundColor Red
        }
        Start-Sleep -Milliseconds $DelayMs
    }
}

Write-Host ""
Write-Host "Carga finalizada apos $DurationSeconds segundos." -ForegroundColor Yellow
