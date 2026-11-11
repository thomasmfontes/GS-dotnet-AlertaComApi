namespace AlertaComApi.Domain.Entities
{
    public class AuditoriaOcorrencia
    {
        public int Id { get; set; }
        public int OcorrenciaId { get; set; }
        public string Operacao { get; set; } = string.Empty; // "INSERT", "UPDATE", "DELETE"
        public string Usuario { get; set; } = string.Empty;
        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;
        public string? StatusAntigo { get; set; }
        public string? StatusNovo { get; set; }
        public string? SeveridadeAntiga { get; set; }
        public string? SeveridadeNova { get; set; }

        public AuditoriaOcorrencia() { }

        public AuditoriaOcorrencia(int ocorrenciaId, string operacao, string usuario, string? statusAntigo, string? statusNovo, string? severidadeAntiga, string? severidadeNova)
        {
            OcorrenciaId = ocorrenciaId;
            Operacao = operacao;
            Usuario = usuario;
            DataAlteracao = DateTime.UtcNow;
            StatusAntigo = statusAntigo;
            StatusNovo = statusNovo;
            SeveridadeAntiga = severidadeAntiga;
            SeveridadeNova = severidadeNova;
        }
    }
}
