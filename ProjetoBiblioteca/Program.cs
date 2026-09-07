using Microsoft.EntityFrameworkCore;
using ProjetoBiblioteca.Dados;
using ProjetoBiblioteca.Infraestrutura.Health; // Health Checks
using ProjetoBiblioteca.Aplicacao.Middlewares; // Correlation ID
using Serilog; // Logging estruturado
using ProjetoBiblioteca.Infraestrutura.Observabilidade; // OpenTelemetry
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProjetoBiblioteca.Dominio.Interfaces; // Camada de Repositorio
using ProjetoBiblioteca.Infraestrutura.Repositorios;
using ProjetoBiblioteca.Aplicacao.Servicos; // Camada de Servico

var builder = WebApplication.CreateBuilder(args);

// Configuracao do Serilog como provedor global de logs da aplicacao
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/app-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog(); // Substitui o provedor de logging padrao pelo Serilog

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString("OracleConnection")
    )
);

builder.Services.AddControllersWithViews();

// Registro das camadas de Repositorio e Servico (permitem testes unitarios com Mock)
builder.Services.AddScoped<IAutorRepositorio, AutorRepositorio>();
builder.Services.AddScoped<ILivroRepositorio, LivroRepositorio>();
builder.Services.AddScoped<IAutorServico, AutorServico>();
builder.Services.AddScoped<ILivroServico, LivroServico>();

// Registro do Health Check customizado que valida a conexao com o Oracle
builder.Services.AddHealthChecks()
    .AddCheck<BancoDadosHealthCheck>("banco_dados");

// Configuracao e registro do OpenTelemetry (Tracing e Metricas)
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(AplicacaoMetricas.NomeServico))
    .WithTracing(tracing =>
    {
        tracing
            // Auto-instrumentacao das requisicoes ASP.NET Core
            .AddAspNetCoreInstrumentation()
            // Auto-instrumentacao de chamadas HTTP de saida
            .AddHttpClientInstrumentation()
            // Escuta a fonte de Spans customizados criada na aplicacao
            .AddSource(AplicacaoMetricas.NomeServico)
            // Exporta os dados de Tracing no Console (fins didaticos)
            .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            // Auto-instrumentacao para metricas padrao do ASP.NET Core
            .AddAspNetCoreInstrumentation()
            // Escuta o Meter customizado da aplicacao
            .AddMeter(AplicacaoMetricas.NomeServico)
            // Exporta as medicoes no Console
            .AddConsoleExporter();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Le/gera o Correlation ID primeiro, para que ele tambem apareca
// na linha de log automatica do UseSerilogRequestLogging logo abaixo
app.UseMiddleware<CorrelationIdMiddleware>();

// Loga automaticamente cada requisicao HTTP recebida (metodo, rota, status, duracao)
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Endpoint nativo de diagnosticos de saude da aplicacao
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Livros}/{action=Index}/{id?}");

app.Run();
