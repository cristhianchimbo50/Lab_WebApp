using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.Services;
using Lab_APIRest.Services.Auth;
using Lab_APIRest.Services.Convenios;
using Lab_APIRest.Services.Email;
using Lab_APIRest.Services.Examenes;
using Lab_APIRest.Services.Medicos;
using Lab_APIRest.Services.Ordenes;
using Lab_APIRest.Services.Pacientes;
using Lab_APIRest.Services.Pagos;
using Lab_APIRest.Services.PDF;
using Lab_APIRest.Services.Perfil;
using Lab_APIRest.Services.Reactivos;
using Lab_APIRest.Services.Resultados;
using Lab_APIRest.Services.Usuarios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<LabDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("LabDb")));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API REST", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' seguido del token JWT generado."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

        opt.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
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
builder.Services.AddScoped<IConvenioService, ConvenioService>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IRecuperacionService, RecuperacionService>();


JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };


        options.RequireHttpsMetadata = true;
        options.SaveToken = false;
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