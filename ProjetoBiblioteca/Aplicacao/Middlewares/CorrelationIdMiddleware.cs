using Serilog.Context;

namespace ProjetoBiblioteca.Aplicacao.Middlewares
{
    // Middleware que le (ou gera) o cabecalho X-Correlation-ID de cada requisicao
    // e injeta esse valor no contexto de log do Serilog, permitindo rastrear
    // todas as linhas de log geradas durante o processamento de uma mesma requisicao.
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Le o cabecalho enviado pelo cliente, ou gera um novo Guid caso nao exista
            string correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                                    ?? Guid.NewGuid().ToString();

            // Devolve o Correlation ID na resposta, para o cliente conseguir rastrear tambem
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            // Empilha a propriedade "CorrelationId" no contexto do Serilog durante a requisicao
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
