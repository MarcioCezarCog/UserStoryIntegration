using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Adicionar serviços ao container
builder.Services.AddControllers();

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Chat API com Integração LLM",
        Version = "v1",
        Description = "API RESTful para interface de chat personalizado que consome modelos de linguagem (LLM)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Equipe de Desenvolvimento",
            Email = "dev@exemplo.com"
        }
    });
    
    // Agrupar os endpoints por controller
    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    
    // Ordenar as operações por path relativo
    options.OrderActionsBy(apiDesc => apiDesc.RelativePath);
    
    // Configurar esquema de segurança para JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // Adicionar suporte para comentários XML na documentação
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    // Verificar se o arquivo existe para evitar erros
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Registrar as camadas da aplicação
// Descomente estas linhas após implementar os métodos de extensão
//builder.Services.AddApplicationServices();
//builder.Services.AddInfrastructureServices(builder.Configuration);

// Adicionar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy => policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? new[] { "http://localhost:3000" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Health Checks configuração avançada
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "api" });
    // Descomente estas linhas após implementar o contexto do banco de dados e a conexão com APIs externas
    //.AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { "database" })
    //.AddUrlGroup(
    //    new Uri(builder.Configuration["LLMService:OpenAI:BaseUrl"] ?? "https://api.openai.com"),
    //    name: "openai-api",
    //    tags: new[] { "llm", "external" })
    //.AddUrlGroup(
    //    new Uri(builder.Configuration["LLMService:Anthropic:BaseUrl"] ?? "https://api.anthropic.com"),
    //    name: "anthropic-api",
    //    tags: new[] { "llm", "external" })
    //.AddUrlGroup(
    //    new Uri(builder.Configuration["LLMService:GoogleAI:BaseUrl"] ?? "https://generativelanguage.googleapis.com"),
    //    name: "google-ai-api",
    //    tags: new[] { "llm", "external" });

var app = builder.Build();

// Configurar o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat API com Integração LLM v1");
        c.RoutePrefix = string.Empty; // Define a raiz como a página do Swagger
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

// Middleware personalizado para logging e tratamento de exceções
// Descomente estas linhas após implementar os middlewares
//app.UseApiExceptionHandling();
//app.UseRequestLogging();

app.MapControllers();

// Configurar endpoints de Health Check
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live") || check.Tags.Contains("api"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/database", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/external", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("external"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

try
{
    Log.Information("Iniciando a aplicação");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}
