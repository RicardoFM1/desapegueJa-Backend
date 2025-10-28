using BackendDesapegaJa.Interfaces;
using BackendDesapegaJa.Repositories;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.FileProviders;
using System;
using System.Text;
using System.IO;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot" // Defina a pasta de arquivos estáticos, se quiser personalizar
});

// ------------------------
// Services
// ------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// MySQL via DI
builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string connString = config.GetConnectionString("DefaultConnection");
    return new MySqlConnection(connString);
});

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var chave = builder.Configuration["TokenKEY:SECRET_KEY"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave))
    };
});

builder.Services.AddAuthorization();

// ------------------------
// Repositórios e Serviços
// ------------------------
builder.Services.AddScoped<IUsuarioRepository, UsuariosRepository>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddScoped<IProdutoRepository, ProdutosRepository>();
builder.Services.AddScoped<ProdutoService>();

builder.Services.AddScoped<ICategoriasRepository, CategoriasRepository>();
builder.Services.AddScoped<CategoriasService>();

builder.Services.AddScoped<IEnderecoRepository, EnderecosRepository>();
builder.Services.AddScoped<EnderecosService>();

builder.Services.AddScoped<IFormasDePagamentoRepository, FormasDePagamentoRepository>();
builder.Services.AddScoped<FormasDePagamentoService>();

builder.Services.AddScoped<IStatusDePagamentoRepository, StatusDePagamentoRepository>();
builder.Services.AddScoped<StatusDePagamentoService>();

builder.Services.AddScoped<IStatusOrdemRepository, StatusOrdemRepository>();
builder.Services.AddScoped<StatusOrdemService>();

builder.Services.AddScoped<IOrdemDeCompraRepository, OrdemDeCompraRepository>();
builder.Services.AddScoped<OrdemDeCompraService>();

builder.Services.AddScoped<IPagamentosRepository, PagamentosRepository>();
builder.Services.AddScoped<PagamentoService>();

builder.Services.AddScoped<ICarrinhoRepository, CarrinhoRepository>();
builder.Services.AddScoped<CarrinhoService>();

// ------------------------
// Build app
// ------------------------
var app = builder.Build();

// ------------------------
// Logging
// ------------------------
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
logger.LogInformation("Environment.ContentRootPath = {ContentRoot}", app.Environment.ContentRootPath);
logger.LogInformation("Environment.WebRootPath = {WebRoot}", app.Environment.WebRootPath);
logger.LogInformation("EnvironmentName = {Env}", app.Environment.EnvironmentName);
logger.LogInformation("Process CurrentDirectory = {CurrentDir}", Environment.CurrentDirectory);
logger.LogInformation("AppContext.BaseDirectory = {BaseDir}", AppContext.BaseDirectory);

// ------------------------
// Middleware
// ------------------------
app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Arquivos estáticos
if (!string.IsNullOrEmpty(app.Environment.WebRootPath) && Directory.Exists(app.Environment.WebRootPath))
{
    logger.LogInformation("Serving static files from: {WebRoot}", app.Environment.WebRootPath);
    app.UseStaticFiles();
}
else
{
    logger.LogWarning("WebRoot not found or empty. Static files disabled. Expected path: {WebRoot}", app.Environment.WebRootPath);
}

// Rotas
app.MapControllers();

// ------------------------
// Run
// ------------------------
app.Run();
