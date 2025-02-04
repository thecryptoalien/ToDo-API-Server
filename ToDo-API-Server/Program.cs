using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ToDo_API_Server.Data;


// Init web application builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container including database contexts, authorization, identity api endpoints, and controllers
#if DEBUG
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("AppDb"));
#else
var connectionString = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
#endif
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllers();

// Add Swagger/OpenAPI endpoints and configure security scheme and other requirements
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
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

// Add rate limiter with fixed limits of 30 requests per min with auto replenishment
builder.Services.AddRateLimiter(options =>
{
    // Set fixed limits 
    options.AddFixedWindowLimiter("fixedLimits", opt =>
    {
        opt.Window = new TimeSpan(0,1,0);
        opt.PermitLimit = 30;
        opt.AutoReplenishment = true;
    });
    // Config rejection code and OnRejected action
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, _) =>
    {
        await context.HttpContext.Response.WriteAsync("Too many requests - RateLimit = 10/min");
    };
});




// Init application with builder
var app = builder.Build();

app.UseRateLimiter();

// Map identity api routes 
app.MapIdentityApi<IdentityUser>().RequireRateLimiting("fixedLimits");

// Use Swagger, Swagger UI, ReDoc, and configure
app.UseSwagger();
app.UseSwaggerUI(options =>
{ 
    options.DocumentTitle = "ToDo-API-Server - Swagger UI"; 
});
app.UseReDoc(options =>
{
    options.DocumentTitle = "ToDo-API-Server - API Docs";
});

// Use Https redirection, authorization, and map controllers
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("fixedLimits");

// Run application
app.Run();
