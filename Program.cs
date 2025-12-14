using BackendDesapegaJa.Helpers;
using BackendDesapegaJa.Interfaces;
using BackendDesapegaJa.Repositories;
using BackendDesapegaJa.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System;
using System.IO;
using System.Text;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot" 
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
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost") ||
                origin.EndsWith(".onrender.com"));
    });
});

// Postgres via DI
builder.Services.AddScoped<NpgsqlConnection>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connString = config.GetConnectionString("DefaultConnection");

    // Tentativa de forçar a conexão IPv4
    var connBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connString);
    

    return new NpgsqlConnection(connString); 

    
});

builder.Services.AddAuthentication(options =>
{
   
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

   
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
 
    options.Cookie.Name = "ExternalAuthCookie";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5); 
    options.Events.OnRedirectToLogin = context =>
    {
   
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
})
.AddJwtBearer(options =>
{
 
    var chave = builder.Configuration["TokenKEY:SECRET_KEY"]
        ?? throw new InvalidOperationException("Chave JWT ausente em appsettings.json (TokenKEY:SECRET_KEY).");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave))
    };
})
.AddGoogle(googleOptions =>
{
   
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]
        ?? throw new InvalidOperationException("ClientId não configurado.");

    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
        ?? throw new InvalidOperationException("ClientSecret não configurado.");

    googleOptions.CallbackPath = "/signin-google";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100 MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100 MB
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

builder.Services.AddScoped<IOrdemProdutoRepository, OrdemProdutoRepository>();
builder.Services.AddScoped<OrdemProdutoService>();

builder.Services.AddScoped<IPagamentosRepository, PagamentosRepository>();
builder.Services.AddScoped<PagamentoService>();

builder.Services.AddScoped<ICarrinhoRepository, CarrinhoRepository>();
builder.Services.AddScoped<CarrinhoService>();

builder.Services.AddScoped<FreteService>();
builder.Services.AddHttpClient<MercadoPagoIntegration>();
builder.Services.AddHostedService<ExpiracaoPagamentosService>();
// ------------------------
// Build app
// ------------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


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
// ------------------------
// Middleware (Corrigido)
// ------------------------
app.UseHttpsRedirection();
if (!string.IsNullOrEmpty(app.Environment.WebRootPath) && Directory.Exists(app.Environment.WebRootPath))
{
    logger.LogInformation("Serving static files from: {WebRoot}", app.Environment.WebRootPath);
    app.UseStaticFiles();
}
else
{
    logger.LogWarning("WebRoot not found or empty. Static files disabled. Expected path: {WebRoot}", app.Environment.WebRootPath);
}

// 1. UseRouting: Deve vir antes de Authentication e Authorization
app.UseRouting();

app.UseCors("AllowLocalhost");
// 2. UseAuthentication: Adiciona suporte a autenticação (QUEM é o usuário).
app.UseAuthentication();

// 3. UseAuthorization: Adiciona suporte a autorização (O QUE o usuário pode fazer).
app.UseAuthorization();




// Rotas
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// ------------------------
// Run
// ------------------------
app.Run();
