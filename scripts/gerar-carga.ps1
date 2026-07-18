#!/usr/bin/env pwsh
# Gera carga continua e paralela nas APIs para acionar o HPA e popular os dashboards do Grafana.
# Uso: .\scripts\gerar-carga.ps1 [-DurationSeconds 120] [-DelayMs 200] [-Parallelism 20]

param(
    [int]$DurationSeconds = 120,
    [int]$DelayMs = 200,
    [int]$Parallelism = 20
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

$targets = @()
foreach ($path in $endpointsAtendimento) { $targets += [pscustomobject]@{ Url = "$atendimentoBase$path"; Nome = "atendimento$path"; ComAuth = $true } }
foreach ($path in $endpointsEstoque)     { $targets += [pscustomobject]@{ Url = "$estoqueBase$path";     Nome = "estoque$path";     ComAuth = $false } }

Add-Type -AssemblyName System.Net.Http
$httpClient = [System.Net.Http.HttpClient]::new()

$deadline  = (Get-Date).AddSeconds($DurationSeconds)
$iteration = 0

Write-Host ""
Write-Host "Gerando carga por $DurationSeconds segundos com $Parallelism requisicoes simultaneas (Ctrl+C para parar antes)..." -ForegroundColor Yellow
Write-Host ""

while ((Get-Date) -lt $deadline) {
    $iteration++

    $requests = 1..$Parallelism | ForEach-Object {
        $target = $targets | Get-Random
        $msg = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Get, $target.Url)
        if ($target.ComAuth) { $msg.Headers.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $token) }
        [pscustomobject]@{ Nome = $target.Nome; Task = $httpClient.SendAsync($msg) }
    }

    [System.Threading.Tasks.Task]::WaitAll(($requests | ForEach-Object { $_.Task }))

    foreach ($r in $requests) {
        if ($r.Task.IsFaulted) {
            Write-Host "[#$iteration] $($r.Nome) -> erro" -ForegroundColor Red
        } else {
            Write-Host "[#$iteration] $($r.Nome) -> $([int]$r.Task.Result.StatusCode)" -ForegroundColor Green
        }
    }

    Start-Sleep -Milliseconds $DelayMs
}

$httpClient.Dispose()

Write-Host ""
Write-Host "Carga finalizada apos $DurationSeconds segundos." -ForegroundColor Yellow
