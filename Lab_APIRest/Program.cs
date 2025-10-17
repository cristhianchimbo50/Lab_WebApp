using Lab_APIRest.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


using Lab_APIRest.Services.Pacientes;
using Lab_APIRest.Services.Medicos;
using Lab_APIRest.Services.Examenes;
using Lab_APIRest.Services.Reactivos;
using Lab_APIRest.Services.Ordenes;
using Lab_APIRest.Services.Pagos;
using Lab_APIRest.Services.Resultados;
using Lab_APIRest.Services.PDF;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<LabDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("LabDb")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<Lab_APIRest.Infrastructure.Services.TokenService>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IMedicoService, MedicoService>();
builder.Services.AddScoped<IExamenService, ExamenService>();
builder.Services.AddScoped<IReactivoService, ReactivoService>();
builder.Services.AddScoped<IExamenReactivoAsociacionService, ExamenReactivoAsociacionService>();
builder.Services.AddScoped<IExamenComposicionService, ExamenComposicionService>();
builder.Services.AddScoped<IOrdenService, OrdenService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IResultadoService, ResultadoService>();
builder.Services.AddScoped<PdfTicketService>();
builder.Services.AddScoped<PdfResultadoService>();





builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
