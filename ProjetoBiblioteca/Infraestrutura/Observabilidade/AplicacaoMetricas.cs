using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ProjetoBiblioteca.Infraestrutura.Observabilidade
{
    // Classe centralizadora de Metricas Customizadas e Traces (Spans) da aplicacao
    public static class AplicacaoMetricas
    {
        // Nome do servico utilizado para identificacao no OpenTelemetry
        public const string NomeServico = "Biblioteca.API";

        // Meter nativo do .NET para registro de metricas customizadas
        public static readonly Meter MeterAplicacao = new(NomeServico, "1.0.0");

        // Contador do total de autores cadastrados com sucesso
        public static readonly Counter<long> AutoresCriadosContador =
            MeterAplicacao.CreateCounter<long>(
                name: "autores_criados_total",
                unit: "{autores}",
                description: "Contagem total de autores cadastrados com sucesso na aplicacao");

        // Contador do total de livros cadastrados com sucesso
        public static readonly Counter<long> LivrosCriadosContador =
            MeterAplicacao.CreateCounter<long>(
                name: "livros_criados_total",
                unit: "{livros}",
                description: "Contagem total de livros cadastrados com sucesso na aplicacao");

        // ActivitySource para criacao manual de Spans de Tracing Distribuido
        public static readonly ActivitySource ActivitySourceAplicacao = new(NomeServico);
    }
}
