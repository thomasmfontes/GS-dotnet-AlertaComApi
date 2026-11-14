using System.Security.Cryptography;
using System.Text;
using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlertaComApi.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AlertaComContext context)
        {
            if (await context.Users.AnyAsync())
            {
                return; // Já populado
            }

            // Senha padrão para os usuários de teste: "123456"
            var defaultPasswordHash = HashPassword("123456");

            // 1. Usuários iniciais
            var cidadao = new User(
                username: "cidadao",
                passwordHash: defaultPasswordHash,
                nome: "Thomas Fontes",
                email: "thomas.fontes@fiap.com.br",
                cpf: "123.456.789-00",
                bairro: "Vila Prudente",
                role: PerfilUsuario.Cidadao
            )
            {
                Telefone = "(11) 98765-4321"
            };

            var defesaCivil = new User(
                username: "defesacivil",
                passwordHash: defaultPasswordHash,
                nome: "Defesa Civil Regional SP",
                email: "defesa.civil@sp.gov.br",
                cpf: "000.111.222-33",
                bairro: "Sé",
                role: PerfilUsuario.DefesaCivil
            )
            {
                Telefone = "(11) 199"
            };

            var admin = new User(
                username: "admin",
                passwordHash: defaultPasswordHash,
                nome: "Administrador AlertaCom",
                email: "admin@alertacom.com.br",
                cpf: "999.888.777-66",
                bairro: "Consolação",
                role: PerfilUsuario.Admin
            );

            await context.Users.AddRangeAsync(cidadao, defesaCivil, admin);
            await context.SaveChangesAsync();

            // 2. Alertas Climáticos Oficiais
            var alertas = new List<AlertaClimatico>
            {
                new AlertaClimatico(
                    titulo: "Alerta Vermelho: Risco Crítico de Alagamento na Av. Anhaia Mello",
                    mensagem: "Precipitação contínua superior a 60mm/h. Evite transitar pelo trecho da Vila Prudente. Desvio recomendado pela Av. Salim Farah Maluf.",
                    tipoRisco: TipoRisco.Alagamento,
                    nivel: NivelSeveridade.Critica,
                    regiao: "Zona Leste",
                    bairro: "Vila Prudente",
                    duracaoHoras: 12
                ),
                new AlertaClimatico(
                    titulo: "Aviso de Rajadas de Vento e Risco de Queda de Árvores",
                    mensagem: "Ventos de até 75 km/h previstos para as próximas 6 horas. Mantenha distância de fiações elétricas e não se abrigue sob árvores isoladas.",
                    tipoRisco: TipoRisco.QuedaArvore,
                    nivel: NivelSeveridade.Alta,
                    regiao: "Zona Sul",
                    bairro: "Santo Amaro",
                    duracaoHoras: 8
                ),
                new AlertaClimatico(
                    titulo: "Atenção: Bueiros com Escoamento Lento após Temporal",
                    mensagem: "Equipes de zeladoria da subprefeitura em ronda para desobstrução de bocas de lobo.",
                    tipoRisco: TipoRisco.BueiroObstruido,
                    nivel: NivelSeveridade.Media,
                    regiao: "Centro",
                    bairro: "Bela Vista",
                    duracaoHoras: 24
                )
            };

            await context.Alertas.AddRangeAsync(alertas);
            await context.SaveChangesAsync();

            // 3. Ocorrências Cidadãs
            var ocorrencias = new List<Ocorrencia>
            {
                new Ocorrencia(
                    titulo: "Bueiro completamente entupido com lixo na esquina",
                    descricao: "Grande acúmulo de garrafas pet e entulho cobrindo toda a grade do bueiro. Qualquer chuva de 10 minutos já faz a água subir na calçada.",
                    tipoRisco: TipoRisco.BueiroObstruido,
                    severidade: NivelSeveridade.Alta,
                    latitude: -23.5851,
                    longitude: -46.5823,
                    endereco: "Av. Professor Luiz Ignácio Anhaia Mello, 1350",
                    bairro: "Vila Prudente",
                    userId: cidadao.Id,
                    fotoUrl: "https://images.unsplash.com/photo-1542601906990-b4d3fb778b09?w=600",
                    idempotencyKey: "SEED-IDEMPOTENCY-001"
                ),
                new Ocorrencia(
                    titulo: "Ponto de alagamento intransitável embaixo do viaduto",
                    descricao: "A água já atinge a altura do capô dos veículos. Dois carros pararam na via impedindo a passagem do transporte coletivo.",
                    tipoRisco: TipoRisco.Alagamento,
                    severidade: NivelSeveridade.Critica,
                    latitude: -23.5890,
                    longitude: -46.5780,
                    endereco: "Viaduto Grande São Paulo",
                    bairro: "Vila Prudente",
                    userId: cidadao.Id,
                    fotoUrl: "https://images.unsplash.com/photo-1515694346937-94d85e41e6f0?w=600",
                    idempotencyKey: "SEED-IDEMPOTENCY-002"
                )
                {
                    Status = StatusOcorrencia.EmAtendimento,
                    ParecerTecnico = "Equipe do Corpo de Bombeiros e CET despachada para interdição da via."
                },
                new Ocorrencia(
                    titulo: "Galho de árvore grande prestes a cair sobre fiação",
                    descricao: "Após a ventania de ontem, galho pesado de eucalipto está apoiado diretamente nos fios de alta tensão.",
                    tipoRisco: TipoRisco.QuedaArvore,
                    severidade: NivelSeveridade.Media,
                    latitude: -23.5570,
                    longitude: -46.6620,
                    endereco: "Rua Augusta, 1800",
                    bairro: "Consolação",
                    userId: cidadao.Id,
                    fotoUrl: "https://images.unsplash.com/photo-1502082553048-f009c37129b9?w=600",
                    idempotencyKey: "SEED-IDEMPOTENCY-003"
                )
                {
                    Status = StatusOcorrencia.Resolvida,
                    ParecerTecnico = "Enel e Subprefeitura realizaram a poda preventiva e liberação da calçada."
                }
            };

            await context.Ocorrencias.AddRangeAsync(ocorrencias);
            await context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
