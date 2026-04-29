using ApiExamenCubosJorgeRodriguez.Data;
using ApiExamenCubosJorgeRodriguez.Helpers;
using ApiExamenCubosJorgeRodriguez.Repositories;
using ApiExamenCubosJorgeRodriguez.Services;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---- KEY VAULT ----
builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(builder.Configuration.GetSection("KeyVault"));
});

SecretClient secretClient = builder.Services.BuildServiceProvider().GetService<SecretClient>();

KeyVaultSecret secreto = await secretClient.GetSecretAsync("secretsqlbd");

KeyVaultSecret secretStorage = await secretClient.GetSecretAsync("secretstorage");
string storageConnectionString = secretStorage.Value;

KeyVaultSecret secretOAuth = await secretClient.GetSecretAsync("ApiOAuthToken--SecretKey");
string oauthSecretKey = secretOAuth.Value;

KeyVaultSecret secretCypher = await secretClient.GetSecretAsync("cypher1");
string cypherKey = secretCypher.Value;

builder.Services.AddTransient<BlobService>(provider => new BlobService(storageConnectionString));

// Add services to the container.

HelperActionOAuthService helper = new HelperActionOAuthService(builder.Configuration, oauthSecretKey);
//ESTA INSTANCIA SOLAMENTE DEBEMOS CREARLA UNA VEZ
builder.Services.AddSingleton<HelperActionOAuthService>(helper);

// INSTANCIAMOS EL HELPERCIFRADO PARA INICIALIZAR LA VARIABLE ESTÁTICA
HelperCifrado helperCifrado = new HelperCifrado(cypherKey);
builder.Services.AddSingleton<HelperCifrado>(helperCifrado);

//HABILITAMOS LA SEGURIDAD DENTRO DE PROGRAM
builder.Services.AddAuthentication(helper.GetAuthenticationSchema())
    .AddJwtBearer(helper.GetJwtBearerOptions());

builder.Services.AddTransient<RepositoryCubos>();
// Provide the connection string for BlobService from configuration.

builder.Services.AddTransient<BlobService>(provider => new BlobService(storageConnectionString));

string connectionString = secreto.Value;
builder.Services.AddDbContext<CubosContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.MapOpenApi();

app.MapScalarApiReference();

app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
