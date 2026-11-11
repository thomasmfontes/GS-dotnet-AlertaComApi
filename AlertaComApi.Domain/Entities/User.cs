namespace AlertaComApi.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Role { get; set; } = "Cidadao"; // "Cidadao", "DefesaCivil", "Admin"
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public ICollection<Ocorrencia> Ocorrencias { get; set; } = new List<Ocorrencia>();

        public User() { }

        public User(string username, string passwordHash, string nome, string email, string cpf, string bairro, string role = "Cidadao")
        {
            Username = username;
            PasswordHash = passwordHash;
            Nome = nome;
            Email = email;
            Cpf = cpf;
            Bairro = bairro;
            Role = role;
            DataCriacao = DateTime.UtcNow;
        }
    }
}
