using Lab_Blazor.Components;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

using Lab_Blazor.Services.Auth;
using Lab_Blazor.Services.Pacientes;
using Lab_Blazor.Services.Medicos;
using Lab_Blazor.Services.Examenes;
using Lab_Blazor.Services.Reactivos;
using Lab_Blazor.Services.Ordenes;
using Lab_Blazor.Services.Pagos;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:5265/");
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ProtectedSessionStorage>();
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



builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
