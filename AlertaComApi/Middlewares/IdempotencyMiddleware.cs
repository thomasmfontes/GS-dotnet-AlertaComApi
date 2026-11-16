using System.Collections.Concurrent;
using System.Text;
using Serilog;

namespace AlertaComApi.Middlewares
{
    public class IdempotentResponse
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = "application/json";
        public byte[] Body { get; set; } = Array.Empty<byte>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class IdempotencyMiddleware
    {
        public const string IdempotencyHeader = "Idempotency-Key";
        private static readonly ConcurrentDictionary<string, IdempotentResponse> Cache = new();
        private readonly RequestDelegate _next;

        public IdempotencyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Aplica verificação de idempotência em POST ou PUT com o header presente
            if (!HttpMethods.IsPost(context.Request.Method) && !HttpMethods.IsPut(context.Request.Method))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(IdempotencyHeader, out var keyValues) || 
                string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
            {
                await _next(context);
                return;
            }

            var idempotencyKey = keyValues.First()!.Trim();

            // 1. Verificação no cache: HIT
            if (Cache.TryGetValue(idempotencyKey, out var cachedResponse))
            {
                Log.Information("[Idempotency] HIT: Requisição com Idempotency-Key '{Key}' já processada. Devolvendo resposta em cache sem reprocessamento.", idempotencyKey);
                context.Response.StatusCode = cachedResponse.StatusCode;
                context.Response.ContentType = cachedResponse.ContentType;
                context.Response.Headers["X-Idempotent-Replay"] = "true";
                context.Response.Headers[IdempotencyHeader] = idempotencyKey;
                await context.Response.Body.WriteAsync(cachedResponse.Body);
                return;
            }

            // 2. Não encontrada: MISS -> Intercepta e armazena
            Log.Information("[Idempotency] MISS: Processando nova requisição com Idempotency-Key '{Key}'.", idempotencyKey);
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Armazena no cache caso o status seja 2xx (Sucesso)
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var bodyBytes = responseBody.ToArray();

                    var entry = new IdempotentResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        ContentType = context.Response.ContentType ?? "application/json",
                        Body = bodyBytes,
                        Timestamp = DateTime.UtcNow
                    };

                    Cache.TryAdd(idempotencyKey, entry);
                    Log.Information("[Idempotency] Resposta armazenada em cache para a chave '{Key}' com status {Status}.", idempotencyKey, context.Response.StatusCode);
                }

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
