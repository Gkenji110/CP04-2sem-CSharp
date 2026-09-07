using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProjetoBiblioteca.Dados;

namespace ProjetoBiblioteca.Infraestrutura.Health
{
    // Health Check customizado que valida a conexão com o banco de dados Oracle
    public class BancoDadosHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _context;

        public BancoDadosHealthCheck(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Tenta abrir/validar a conexão real com o banco Oracle configurado no AppDbContext
                var conexaoOk = await _context.Database.CanConnectAsync(cancellationToken);

                if (conexaoOk)
                {
                    return HealthCheckResult.Healthy(
                        "Conexão com o Banco de Dados Oracle estabelecida com sucesso.");
                }

                return HealthCheckResult.Unhealthy(
                    "Não foi possível estabelecer conexão com o Banco de Dados Oracle.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Falha ao conectar no Banco de Dados Oracle.", ex);
            }
        }
    }
}
