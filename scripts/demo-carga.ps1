#Requires -Version 5.1
<#
.SYNOPSIS
    Script de demonstracao para o video do Tech Challenge - Fase 2.

.DESCRIPTION
    Busca dados existentes no banco (cliente, veiculo, servico, peca) e
    demonstra o fluxo de OS sem criar registros auxiliares.

.EXAMPLE
    .\demo-carga.ps1 -Modo Fluxo -UrlAtendimento http://localhost:30080 -UrlEstoque http://localhost:30081
    .\demo-carga.ps1 -Modo Carga -UrlAtendimento http://localhost:30080 -UrlEstoque http://localhost:30081 -QtdOS 60
    .\demo-carga.ps1 -Modo Tudo  -UrlAtendimento http://localhost:30080 -UrlEstoque http://localhost:30081
#>
param(
    [ValidateSet("Fluxo","Carga","Tudo")]
    [string]$Modo = "Tudo",
    [string]$UrlAtendimento = "http://localhost:30080",
    [string]$UrlEstoque     = "http://localhost:30081",
    [string]$Usuario        = "admin@oficina.com",
    [string]$Senha          = "Admin@123",
    [int]$QtdOS             = 60
)

$ErrorActionPreference = "Stop"
$Global:Token   = $null
$Global:Headers = $null

function Write-Step([string]$msg) { Write-Host ""; Write-Host "  >> $msg" -ForegroundColor Cyan }
function Write-Ok([string]$msg)   { Write-Host "     OK: $msg" -ForegroundColor Green }
function Write-Info([string]$msg) { Write-Host "     -- $msg" -ForegroundColor Gray }
function Write-Fail([string]$msg) { Write-Host "     ERRO: $msg" -ForegroundColor Red }

function Invoke-Api([string]$Metodo, [string]$Url, [string]$Body, [switch]$SemAuth) {
    $h = @{ "Content-Type" = "application/json" }
    if (-not $SemAuth -and $Global:Token) { $h["Authorization"] = "Bearer $Global:Token" }
    $params = @{ Method = $Metodo; Uri = $Url; Headers = $h }
    if ($Body) { $params["Body"] = $Body }
    return Invoke-RestMethod @params
}

# ── Login ─────────────────────────────────────────────────────────────────────

function Get-Token {
    Write-Step "Autenticando como $Usuario"
    $body = "{`"email`":`"$Usuario`",`"senha`":`"$Senha`"}"
    $resp = Invoke-Api -Metodo POST -Url "$UrlAtendimento/auth/login" -SemAuth -Body $body
    $Global:Token = $resp.token
    $Global:Headers = @{ "Content-Type" = "application/json"; "Authorization" = "Bearer $Global:Token" }
    Write-Ok "Token JWT obtido"
}

# ── Buscar dados existentes ───────────────────────────────────────────────────

function Get-DadosBase {
    Write-Step "Buscando dados existentes no banco"

    $clientes = Invoke-Api -Metodo GET -Url "$UrlAtendimento/clientes"
    if (-not $clientes -or $clientes.Count -eq 0) {
        throw "Nenhum cliente encontrado. Cadastre um cliente via Swagger antes de rodar o script."
    }
    $cliente = $clientes[0]
    Write-Ok "Cliente: $($cliente.nome) ($($cliente.id))"

    $veiculos = Invoke-Api -Metodo GET -Url "$UrlAtendimento/veiculos"
    if (-not $veiculos -or $veiculos.Count -eq 0) {
        throw "Nenhum veiculo encontrado. Cadastre um veiculo via Swagger antes de rodar o script."
    }
    $veiculo = $veiculos | Where-Object { $_.clienteId -eq $cliente.id } | Select-Object -First 1
    if (-not $veiculo) { $veiculo = $veiculos[0] }
    Write-Ok "Veiculo: $($veiculo.placa) - $($veiculo.marca) $($veiculo.modelo) ($($veiculo.id))"

    $pecas = Invoke-Api -Metodo GET -Url "$UrlEstoque/estoque/pecas"
    $peca = $null
    if ($pecas -and $pecas.Count -gt 0) {
        $peca = $pecas | Where-Object { $_.quantidadeEstoque -gt 0 } | Select-Object -First 1
        if ($peca) { Write-Ok "Peca: $($peca.nome) (estoque: $($peca.quantidadeEstoque)) ($($peca.id))" }
    }
    if (-not $peca) { Write-Info "Nenhuma peca com estoque disponivel - OS sera aberta sem peca" }

    return @{ cliente = $cliente; veiculo = $veiculo; peca = $peca }
}

# ── Fluxo completo de negocio ─────────────────────────────────────────────────

function Invoke-FluxoCompleto($dados) {
    Write-Host ""
    Write-Host "  =============================================" -ForegroundColor Yellow
    Write-Host "  FLUXO COMPLETO - OS DO INICIO AO FIM" -ForegroundColor Yellow
    Write-Host "  =============================================" -ForegroundColor Yellow

    $cId = $dados.cliente.id
    $vId = $dados.veiculo.id

    # Montar itens de peca (opcional)
    $itensPeca = "[]"
    if ($dados.peca) {
        $pecaId = $dados.peca.id
        $itensPeca = "[{`"pecaId`":`"$pecaId`",`"quantidade`":1}]"
    }

    # Abrir OS
    Write-Step "Abrindo Ordem de Servico"
    $body = "{`"clienteId`":`"$cId`",`"veiculoId`":`"$vId`",`"observacoes`":`"Barulho no motor e oleo escuro`",`"pecas`":$itensPeca}"
    $os = Invoke-Api -Metodo POST -Url "$UrlAtendimento/ordens-servico" -Body $body
    $osId = $os.id
    Write-Ok "OS aberta: $osId"

    Start-Sleep -Seconds 1
    $statusResp = Invoke-Api -Metodo GET -Url "$UrlAtendimento/ordens-servico/$osId/status"
    Write-Info "Status atual: $($statusResp.status)"

    # EmDiagnostico (2)
    Write-Step "Avancando para EmDiagnostico"
    Invoke-Api -Metodo PUT -Url "$UrlAtendimento/ordens-servico/$osId/status" -Body "{`"novoStatus`":2}" | Out-Null
    Write-Ok "Status: EmDiagnostico"

    # AguardandoAprovacao (3)
    Write-Step "Avancando para AguardandoAprovacao"
    Invoke-Api -Metodo PUT -Url "$UrlAtendimento/ordens-servico/$osId/status" -Body "{`"novoStatus`":3}" | Out-Null
    Write-Ok "Status: AguardandoAprovacao"

    # Aprovar orcamento
    Write-Step "Aprovando orcamento (endpoint de notificacao externa)"
    Invoke-Api -Metodo PUT -Url "$UrlAtendimento/ordens-servico/$osId/orcamento" -Body "{`"aprovado`":true}" | Out-Null
    Write-Ok "Orcamento aprovado"

    Write-Info "Aprovacao avanca automaticamente para EmExecucao"

    # Finalizada (5) - publica evento
    Write-Step "Finalizando OS - publica OsFinalizadaEvent no RabbitMQ"
    Invoke-Api -Metodo PUT -Url "$UrlAtendimento/ordens-servico/$osId/status" -Body "{`"novoStatus`":5}" | Out-Null
    Write-Ok "Status: Finalizada - evento publicado"

    # Aguardar consumer
    Write-Step "Aguardando consumer do Estoque processar a baixa (3s)"
    Start-Sleep -Seconds 3

    # Verificar estoque se tinha peca
    if ($dados.peca) {
        Write-Step "Verificando estoque apos baixa automatica"
        $pecaAtualizada = Invoke-Api -Metodo GET -Url "$UrlEstoque/estoque/pecas/$($dados.peca.id)"
        $estoqueAntes = $dados.peca.quantidadeEstoque
        $estoqueDepois = $pecaAtualizada.quantidadeEstoque
        Write-Ok "Estoque: $estoqueDepois unidades (era $estoqueAntes)"
        Write-Info "Baixa processada: RabbitMQ / MassTransit / BaixaEstoqueConsumer"
    }

    # Entregue (6)
    Write-Step "Registrando entrega"
    Invoke-Api -Metodo PUT -Url "$UrlAtendimento/ordens-servico/$osId/status" -Body "{`"novoStatus`":6}" | Out-Null
    Write-Ok "OS entregue ao cliente"

    Write-Host ""
    Write-Host "  OS ID : $osId" -ForegroundColor White
    Write-Host "  Fluxo completo executado com sucesso." -ForegroundColor Green
}

# ── Carga sequencial para HPA ─────────────────────────────────────────────────

function Invoke-CargaSequencial($dados) {
    Write-Host ""
    Write-Host "  =============================================" -ForegroundColor Yellow
    Write-Host "  CARGA - $QtdOS OS para subir RPS e acionar HPA" -ForegroundColor Yellow
    Write-Host "  =============================================" -ForegroundColor Yellow
    Write-Host "  Grafana : http://localhost:30300" -ForegroundColor Cyan
    Write-Host "  HPA     : kubectl get hpa -n oficina-mecanica -w" -ForegroundColor Cyan

    $cId = $dados.cliente.id
    $vId = $dados.veiculo.id

    Write-Host ""
    Write-Host "  Iniciando $QtdOS requisicoes de abertura de OS..." -ForegroundColor White

    $sucesso = 0
    $erro    = 0
    $inicio  = Get-Date

    for ($i = 1; $i -le $QtdOS; $i++) {
        $body = "{`"clienteId`":`"$cId`",`"veiculoId`":`"$vId`",`"observacoes`":`"OS carga $i`",`"pecas`":[]}"
        try {
            Invoke-RestMethod -Method POST -Uri "$UrlAtendimento/ordens-servico" -Headers $Global:Headers -Body $body | Out-Null
            $sucesso++
        } catch {
            $erro++
        }
        if ($i % 10 -eq 0) {
            $elapsed = ((Get-Date) - $inicio).TotalSeconds
            $rps = if ($elapsed -gt 0) { [math]::Round($sucesso / $elapsed, 1) } else { 0 }
            Write-Host "  $i/$QtdOS - $rps req/s" -ForegroundColor DarkCyan
        }
    }

    $duracao = ((Get-Date) - $inicio).TotalSeconds
    $rps = if ($duracao -gt 0) { [math]::Round($sucesso / $duracao, 1) } else { 0 }

    Write-Host ""
    Write-Ok "$sucesso OS criadas"
    if ($erro -gt 0) { Write-Fail "$erro erros" }
    Write-Info "Duracao   : $([math]::Round($duracao, 1))s"
    Write-Info "Throughput: $rps req/s medio"
    Write-Host "  Verifique : kubectl get hpa -n oficina-mecanica" -ForegroundColor Cyan
    Write-Host "  Grafana   : http://localhost:30300" -ForegroundColor Cyan
}

# ── Entrypoint ────────────────────────────────────────────────────────────────

Get-Token
$dados = Get-DadosBase

switch ($Modo) {
    "Fluxo" { Invoke-FluxoCompleto $dados }
    "Carga" { Invoke-CargaSequencial $dados }
    "Tudo"  {
        Invoke-FluxoCompleto $dados
        Write-Host ""
        Write-Host "  Aguardando 5s antes de iniciar a carga..." -ForegroundColor Gray
        Start-Sleep -Seconds 5
        Invoke-CargaSequencial $dados
    }
}

Write-Host ""
Write-Host "  Concluido." -ForegroundColor Green
