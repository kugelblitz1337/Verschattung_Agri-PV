
using ALKISService.Controllers;
using ALKISService.Repository;
using ALKISService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controller und MemoryCache
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Swagger/OpenAPI-Konfiguration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Geben Sie 'Bearer' gefolgt von Ihrem Token ein. Beispiel: 'Bearer abc123'"
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

// JWT Auth konfigurieren
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = "ALKISServiceAPI",
           ValidAudience = "Frontend",
           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("arDmt0hEZBBzdm8ywewD9UwafMecqtq6vMrm91jsXptWtuctEPbVAVU16YpfZKBt"))
       };
   });
builder.Services.AddAuthorization();

// Deine Services & Repositories
builder.Services.AddScoped<IFlurstueckService, FlurstueckService>();
builder.Services.AddScoped<CoordinateTransformService>();
builder.Services.AddScoped<IFlurstueckRepository, FlurstueckRepository>();
builder.Services.AddScoped<IMarkingRepository, MarkingRepository>();
builder.Services.AddScoped<ICsvImporter, CsvImporter>();
builder.Services.AddScoped<IDbContext, DbContext>();
builder.Services.AddScoped<IGeometryService, GeometryService>();
builder.Services.AddScoped<IMarkingService, MarkingService>();
builder.Services.AddScoped<IZipDownloader, ZipDownloader>();

var app = builder.Build();

// Swagger und Developer Exception Page NUR im Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // app.UseDeveloperExceptionPage(); // Optional für Stacktraces im Browser
}

// Reihenfolge beachten: Auth dann Controllers
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
