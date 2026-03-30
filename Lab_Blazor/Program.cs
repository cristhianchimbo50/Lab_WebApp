using Lab_Blazor.Components;
using Lab_Blazor.Services.Auth;
using Lab_Blazor.Services.Convenios;
using Lab_Blazor.Services.Examenes;
using Lab_Blazor.Services.Medicos;
using Lab_Blazor.Services.Ordenes;
using Lab_Blazor.Services.Pacientes;
using Lab_Blazor.Services.Pagos;
using Lab_Blazor.Services.Perfil;
using Lab_Blazor.Services.Reactivos;
using Lab_Blazor.Services.Resultados;
using Lab_Blazor.Services.Usuarios;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration["Api:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException("La variable de entorno o configuración 'Api:BaseUrl' no está definida. Configúrala en Azure App Service o en appsettings.Production.json.");
}
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddAuthorizationCore();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthApiService, AuthApiService>();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();

builder.Services.AddScoped<IPacientesApiService, PacientesApiService>();
builder.Services.AddScoped<IMedicosApiService, MedicosApiService>();
builder.Services.AddScoped<IExamenesApiService, ExamenesApiService>();
builder.Services.AddScoped<IReactivosApiService, ReactivosApiService>();
builder.Services.AddScoped<IExamenComposicionApiService, ExamenComposicionApiService>();
builder.Services.AddScoped<IExamenReactivoAsociacionesApiService, ExamenReactivoAsociacionesApiService>();
builder.Services.AddScoped<IOrdenesApiService, OrdenesApiService>();
builder.Services.AddScoped<IPagosApiService, PagosApiService>();
builder.Services.AddScoped<IResultadosApiService, ResultadosApiService>();
builder.Services.AddScoped<IConveniosApiService, ConveniosApiService>();
builder.Services.AddScoped<IMovimientosApiService, MovimientosApiService>();
builder.Services.AddScoped<IPerfilApiService, PerfilApiService>();
builder.Services.AddScoped<IUsuariosApiService, UsuariosApiService>();
builder.Services.AddScoped<IRecuperacionApiService, RecuperacionApiService>();
builder.Services.AddScoped<SesionVerificacionService>();


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
