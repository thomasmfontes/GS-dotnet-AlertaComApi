namespace AlertaComApi.Domain.Enums
{
    public enum TipoRisco
    {
        Alagamento = 1,
        BueiroObstruido = 2,
        QuedaArvore = 3,
        Deslizamento = 4,
        RachaduraVia = 5,
        Outro = 99
    }

    public enum NivelSeveridade
    {
        Baixa = 1,
        Media = 2,
        Alta = 3,
        Critica = 4
    }

    public enum StatusOcorrencia
    {
        Aberta = 1,
        EmAnalise = 2,
        EmAtendimento = 3,
        Resolvida = 4,
        Cancelada = 5
    }

    public static class PerfilUsuario
    {
        public const string Cidadao = "Cidadao";
        public const string DefesaCivil = "DefesaCivil";
        public const string Admin = "Admin";
    }
}
