namespace AlertaComApi.Domain.Common
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();

        public static OperationResult Ok(string message = "Operação realizada com sucesso.") =>
            new() { Success = true, Message = message };

        public static OperationResult Fail(string error) =>
            new() { Success = false, Message = error, Errors = new() { error } };

        public static OperationResult Fail(List<string> errors, string message = "Ocorreram erros na operação.") =>
            new() { Success = false, Message = message, Errors = errors };
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; set; }

        public static OperationResult<T> Ok(T data, string message = "Operação realizada com sucesso.") =>
            new() { Success = true, Message = message, Data = data };

        public new static OperationResult<T> Fail(string error) =>
            new() { Success = false, Message = error, Errors = new() { error } };

        public new static OperationResult<T> Fail(List<string> errors, string message = "Ocorreram erros na operação.") =>
            new() { Success = false, Message = message, Errors = errors };
    }
}
