# Teste de Estresse para Rate Limiting (Requisito 04 - HTTP 429)
$url = "http://localhost:5208/api/ocorrencias"
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "TESTANDO RATE LIMITING: Disparando 105 chamadas rápidas..." -ForegroundColor Cyan
Write-Host "Limite configurado na API: 100 requisições por minuto" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

$bloqueado = $false
$bloqueioResponse = $null

1..105 | ForEach-Object {
    $i = $_
    try {
        $res = Invoke-WebRequest -Uri $url -Method Get -UseBasicParsing -ErrorAction Stop
        if ($i % 20 -eq 0 -or $i -eq 1 -or $i -eq 100) {
            Write-Host "Requisição #$i -> Status 200 OK (Dentro da cota permitida)" -ForegroundColor Green
        }
    }
    catch {
        $statusCode = [int]$_.Exception.Response.StatusCode
        if ($statusCode -eq 429) {
            $bloqueado = $true
            if ($null -eq $bloqueioResponse) {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $bloqueioResponse = $reader.ReadToEnd()
            }
            Write-Host "Requisição #$i -> [BLOQUEADA] HTTP 429 Too Many Requests!" -ForegroundColor Red
        }
        else {
            Write-Host "Requisição #$i -> Erro inesperado: $statusCode" -ForegroundColor Yellow
        }
    }
}

Write-Host "==========================================================" -ForegroundColor Cyan
if ($bloqueado) {
    Write-Host "RESULTADO: SUCESSO! O Rate Limiter funcionou e barrou o excesso com HTTP 429." -ForegroundColor Green
    Write-Host ""
    Write-Host "Payload retornado pela API (RFC 6585 ProblemDetails):" -ForegroundColor Cyan
    Write-Host $bloqueioResponse -ForegroundColor Yellow
} else {
    Write-Host "RESULTADO: Não atingiu o limite de 100 reqs na janela." -ForegroundColor Yellow
}
Write-Host "==========================================================" -ForegroundColor Cyan
