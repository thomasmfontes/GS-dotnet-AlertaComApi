# Teste Completo de Matriz de Endpoints e Retornos Indevidos - AlertaComApi
$baseUrl = "http://localhost:5208"
$results = @()

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers = @{},
        $Body = $null,
        [int]$ExpectedStatus,
        [string]$Description
    )

    $tag = "OK"
    $color = "Green"
    try {
        $params = @{
            Uri = "$baseUrl$Url"
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
            ErrorAction = "Stop"
            UseBasicParsing = $true
            TimeoutSec = 35
        }
        if ($Body) {
            $params["Body"] = ($Body | ConvertTo-Json -Depth 5)
        }

        $response = Invoke-WebRequest @params
        $actualStatus = [int]$response.StatusCode
        $passed = ($actualStatus -eq $ExpectedStatus)
        if (-not $passed) { $tag = "FALHA"; $color = "Red" }
        
        Write-Host "[$tag] $Name -> Esperado: $ExpectedStatus | Obtido: $actualStatus" -ForegroundColor $color

        $script:results += [PSCustomObject]@{
            Cenario = $Name
            Metodo = $Method
            Rota = $Url
            Esperado = $ExpectedStatus
            Obtido = $actualStatus
            Passou = $passed
            Detalhe = $Description
            ReplayHeader = $response.Headers["x-idempotent-replay"]
        }
    }
    catch {
        $actualStatus = $null
        if ($_.Exception.Response) {
            $actualStatus = [int]$_.Exception.Response.StatusCode
        }
        $passed = ($actualStatus -eq $ExpectedStatus)
        if (-not $passed) { $tag = "FALHA"; $color = "Red" }
        Write-Host "[$tag] $Name -> Esperado: $ExpectedStatus | Obtido: $actualStatus" -ForegroundColor $color
        $script:results += [PSCustomObject]@{
            Cenario = $Name
            Metodo = $Method
            Rota = $Url
            Esperado = $ExpectedStatus
            Obtido = $actualStatus
            Passou = $passed
            Detalhe = $Description
            ReplayHeader = $null
        }
    }
}

Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "INICIANDO TESTES EXAUSTIVOS: TODOS OS ENDPOINTS E CENARIOS DE ERRO" -ForegroundColor Cyan
Write-Host "===============================================================" -ForegroundColor Cyan

# 1. Obter Tokens
$loginCidadao = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body ('{"username":"cidadao","password":"123456"}' ) -UseBasicParsing
$tokenCidadao = $loginCidadao.token
$headersCidadao = @{ "Authorization" = "Bearer $tokenCidadao" }

$loginDefesa = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body ('{"username":"defesacivil","password":"123456"}' ) -UseBasicParsing
$tokenDefesa = $loginDefesa.token
$headersDefesa = @{ "Authorization" = "Bearer $tokenDefesa" }

# ----------------- GRUPO 1: AUTH & USUARIOS -----------------
Test-Endpoint -Name "Auth: Login Sucesso" -Method "Post" -Url "/api/auth/login" -Body @{ username="cidadao"; password="123456" } -ExpectedStatus 200 -Description "Login com credenciais corretas"
Test-Endpoint -Name "Auth: Login Senha Incorreta (401)" -Method "Post" -Url "/api/auth/login" -Body @{ username="cidadao"; password="SENHA_ERRADA" } -ExpectedStatus 401 -Description "Rejeicao com 401 Unauthorized"
Test-Endpoint -Name "Auth: Login Body Invalido (400)" -Method "Post" -Url "/api/auth/login" -Body @{ username=""; password="" } -ExpectedStatus 400 -Description "Validacao FluentValidation disparando 400"
$rand = Get-Random -Minimum 1000 -Maximum 9999
Test-Endpoint -Name "Auth: Register Sucesso (201)" -Method "Post" -Url "/api/auth/register" -Body @{ username="user_$rand"; password="SenhaForte@123"; nome="Novo Teste"; email="teste$rand@email.com"; cpf="000.000.000-00"; bairro="Centro" } -ExpectedStatus 201 -Description "Cadastro de novo usuario retorna 201 Created"
Test-Endpoint -Name "Auth: Register Dados Invalidos (400)" -Method "Post" -Url "/api/auth/register" -Body @{ username="ab"; password="123"; email="invalido" } -ExpectedStatus 400 -Description "Validacao de formato e tamanho minimo"
Test-Endpoint -Name "Auth: Register Duplicado (400)" -Method "Post" -Url "/api/auth/register" -Body @{ username="cidadao"; password="SenhaForte@123"; nome="Duplicado"; email="thomas.fontes@fiap.com.br"; cpf="123.456.789-00"; bairro="Vila Prudente" } -ExpectedStatus 400 -Description "Tentativa de recadastrar username existente"
Test-Endpoint -Name "Auth: Profile Sem Token (401)" -Method "Get" -Url "/api/auth/profile" -ExpectedStatus 401 -Description "Bloqueio de acesso a dados privados sem autenticacao"
Test-Endpoint -Name "Auth: Profile Com Token (200)" -Method "Get" -Url "/api/auth/profile" -Headers $headersCidadao -ExpectedStatus 200 -Description "Retorno do perfil autenticado"

# ----------------- GRUPO 2: OCORRENCIAS -----------------
Test-Endpoint -Name "Ocorrencias: Listagem Publica Paginada (200)" -Method "Get" -Url "/api/ocorrencias?pageIndex=1&pageSize=5" -ExpectedStatus 200 -Description "Consulta aberta com metadados de paginacao"
Test-Endpoint -Name "Ocorrencias: Buscar por ID Existente (200)" -Method "Get" -Url "/api/ocorrencias/1" -ExpectedStatus 200 -Description "Recuperacao de ocorrencia do seed"
Test-Endpoint -Name "Ocorrencias: Buscar por ID Inexistente (404)" -Method "Get" -Url "/api/ocorrencias/999999" -ExpectedStatus 404 -Description "Retorno correto 404 Not Found"
Test-Endpoint -Name "Ocorrencias: Buscar Protocolo Inexistente (404)" -Method "Get" -Url "/api/ocorrencias/protocolo/PROTOCOLO-NAO-EXISTE" -ExpectedStatus 404 -Description "Retorno 404 para protocolo invalido"
Test-Endpoint -Name "Ocorrencias: Minhas Sem Token (401)" -Method "Get" -Url "/api/ocorrencias/minhas" -ExpectedStatus 401 -Description "Tentativa de ver historico sem autenticacao"
Test-Endpoint -Name "Ocorrencias: Minhas Com Token (200)" -Method "Get" -Url "/api/ocorrencias/minhas" -Headers $headersCidadao -ExpectedStatus 200 -Description "Retorno apenas das ocorrencias do Thomas"

$keyIdemp = "TESTE-MATRIZ-$rand"
$headersIdemp = @{ "Authorization" = "Bearer $tokenCidadao"; "Idempotency-Key" = $keyIdemp }
$bodyOcorrencia = @{
    titulo = "Queda de Arvore Interditando Pista"
    descricao = "Arvore de grande porte caiu na fiacao e bloqueou a faixa da direita"
    tipoRisco = 3
    severidade = 2
    latitude = -23.5850
    longitude = -46.5780
    endereco = "Rua Capitao Pacheco e Chaves, 500"
    bairro = "Vila Prudente"
    fotoUrl = "https://alertacom.blob.core.windows.net/evidencias/arvore.jpg"
}

Test-Endpoint -Name "Ocorrencias: Criar Sem Token (401)" -Method "Post" -Url "/api/ocorrencias" -Body $bodyOcorrencia -ExpectedStatus 401 -Description "Nao permite criacao anonima"
Test-Endpoint -Name "Ocorrencias: Criar Body Invalido (400)" -Method "Post" -Url "/api/ocorrencias" -Headers $headersCidadao -Body @{ titulo = ""; descricao = "" } -ExpectedStatus 400 -Description "Validacao de campos obrigatorios vazios"
Test-Endpoint -Name "Ocorrencias: Criar Sucesso 1a Vez (201)" -Method "Post" -Url "/api/ocorrencias" -Headers $headersIdemp -Body $bodyOcorrencia -ExpectedStatus 201 -Description "Primeiro registro com Idempotency-Key"
Test-Endpoint -Name "Ocorrencias: Replay Idempotente (201)" -Method "Post" -Url "/api/ocorrencias" -Headers $headersIdemp -Body $bodyOcorrencia -ExpectedStatus 201 -Description "Reenvio da mesma requisicao devolve cache com x-idempotent-replay"

# RBAC: Atualizacao de Status
Test-Endpoint -Name "Status: Atualizar Sem Token (401)" -Method "Patch" -Url "/api/ocorrencias/1/status" -Body @{ novoStatus = 2; parecerTecnico = "Em atendimento" } -ExpectedStatus 401 -Description "Bloqueio anonimo"
Test-Endpoint -Name "Status: Atualizar Com Token Cidadao (403)" -Method "Patch" -Url "/api/ocorrencias/1/status" -Headers $headersCidadao -Body @{ novoStatus = 2; parecerTecnico = "Tentativa Cidadao" } -ExpectedStatus 403 -Description "Cidadao NAO pode despachar ocorrencia (RBAC)"
Test-Endpoint -Name "Status: Atualizar Com Defesa Civil (200)" -Method "Patch" -Url "/api/ocorrencias/1/status" -Headers $headersDefesa -Body @{ novoStatus = 2; parecerTecnico = "Equipe em deslocamento" } -ExpectedStatus 200 -Description "Defesa Civil autorizada a despachar"
Test-Endpoint -Name "Status: Atualizar ID Inexistente (404)" -Method "Patch" -Url "/api/ocorrencias/999999/status" -Headers $headersDefesa -Body @{ novoStatus = 2; parecerTecnico = "Teste" } -ExpectedStatus 404 -Description "Ocorrencia inexistente retorna 404"

# ----------------- GRUPO 3: ALERTAS DEFESA CIVIL -----------------
Test-Endpoint -Name "Alertas: Listar Ativos Publico (200)" -Method "Get" -Url "/api/alertas" -ExpectedStatus 200 -Description "Listagem publica dos alertas da cidade"
Test-Endpoint -Name "Alertas: Buscar ID Existente (200)" -Method "Get" -Url "/api/alertas/1" -ExpectedStatus 200 -Description "Detalhe de alerta climatico"
Test-Endpoint -Name "Alertas: Buscar ID Inexistente (404)" -Method "Get" -Url "/api/alertas/999999" -ExpectedStatus 404 -Description "Alerta nao encontrado"
Test-Endpoint -Name "Alertas: Criar Sem Token (401)" -Method "Post" -Url "/api/alertas" -Body @{ titulo = "Alerta Fake"; mensagem = "Teste"; tipoRisco = 1; nivel = 1; regiao = "Sul"; bairro = "Santo Amaro" } -ExpectedStatus 401 -Description "Bloqueio anonimo"
Test-Endpoint -Name "Alertas: Criar Com Cidadao (403)" -Method "Post" -Url "/api/alertas" -Headers $headersCidadao -Body @{ titulo = "Alerta Fake"; mensagem = "Teste"; tipoRisco = 1; nivel = 1; regiao = "Sul"; bairro = "Santo Amaro" } -ExpectedStatus 403 -Description "Cidadao nao tem permissao para emitir alerta civico"
Test-Endpoint -Name "Alertas: Criar Com Defesa Civil (201)" -Method "Post" -Url "/api/alertas" -Headers $headersDefesa -Body @{ titulo = "Alerta Chuvas Intensas"; mensagem = "Risco moderado nas proximas 2 horas"; tipoRisco = 1; nivel = 2; regiao = "Zona Leste"; bairro = "Mooca" } -ExpectedStatus 201 -Description "Defesa Civil autorizada a criar alertas"
Test-Endpoint -Name "Alertas: Desativar Com Cidadao (403)" -Method "Patch" -Url "/api/alertas/1/desativar" -Headers $headersCidadao -ExpectedStatus 403 -Description "Cidadao proibido de desativar alerta"
Test-Endpoint -Name "Alertas: Desativar Com Defesa Civil (200)" -Method "Patch" -Url "/api/alertas/1/desativar" -Headers $headersDefesa -ExpectedStatus 200 -Description "Defesa Civil desativa alerta com sucesso"
Test-Endpoint -Name "Alertas: Desativar ID Inexistente (404)" -Method "Patch" -Url "/api/alertas/999999/desativar" -Headers $headersDefesa -ExpectedStatus 404 -Description "Alerta inexistente retorna 404"

# ----------------- GRUPO 4: HEALTH CHECKS -----------------
Test-Endpoint -Name "Health: Sonda Geral (200)" -Method "Get" -Url "/health" -ExpectedStatus 200 -Description "Diagnostico completo da aplicacao e probes"
Test-Endpoint -Name "Health: Liveness Probe (200)" -Method "Get" -Url "/health/live" -ExpectedStatus 200 -Description "Sonda rapida de processo vivo"
Test-Endpoint -Name "Health: Readiness Probe (200)" -Method "Get" -Url "/health/ready" -ExpectedStatus 200 -Description "Sonda de prontidao das dependencias"

$total = $script:results.Count
$passou = ($script:results | Where-Object { $_.Passou -eq $true }).Count
$falhou = ($script:results | Where-Object { $_.Passou -eq $false }).Count

Write-Host ""
Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "RESUMO DA MATRIZ: Total: $total | Passou: $passou | Falhou: $falhou" -ForegroundColor $(if ($falhou -eq 0) { "Green" } else { "Red" })
Write-Host "===============================================================" -ForegroundColor Cyan
