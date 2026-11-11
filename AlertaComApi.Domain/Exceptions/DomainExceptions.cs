namespace AlertaComApi.Domain.Exceptions
{
    public class DomainValidationException : Exception
    {
        public List<string> Errors { get; }

        public DomainValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public DomainValidationException(List<string> errors) : base("Falha de validação de domínio.")
        {
            Errors = errors;
        }
    }

    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, object id)
            : base($"Entidade '{entityName}' com identificador '{id}' não foi encontrada.")
        {
        }
    }

    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
    }
}
