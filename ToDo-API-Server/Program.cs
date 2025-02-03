using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ToDo_API_Server.Data;

// Init web application builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container including database contexts, authorization, identity api endpoints, and controllers
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("AppDb"));
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllers();

// Add Swagger/OpenAPI endpoints and configure security scheme and requirements
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Internal", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        BearerFormat = "JWT",
        Scheme = "Bearer",
        Description = "Specify the authorization token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http
    });
    OpenApiSecurityRequirement openApiSecurityRequirement = new OpenApiSecurityRequirement();
    OpenApiSecurityScheme openApiSecurityScheme = new OpenApiSecurityScheme {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    openApiSecurityRequirement.Add(openApiSecurityScheme, new string[] { });
    options.AddSecurityRequirement(openApiSecurityRequirement);
});

// Init application with builder
var app = builder.Build();

// Map identity api routes 
app.MapIdentityApi<IdentityUser>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use Https redirection, authorization, and map controllers
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Run application
app.Run();
