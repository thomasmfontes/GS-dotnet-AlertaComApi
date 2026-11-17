Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  AlertaCom API - Executando Testes xUnit " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

dotnet test --logger "console;verbosity=normal"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[SUCESSO] Todos os testes unitários e de integração passaram com êxito!" -ForegroundColor Green
} else {
    Write-Host "`n[ERRO] Alguns testes falharam." -ForegroundColor Red
}
