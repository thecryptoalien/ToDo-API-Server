using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ToDo_API_Server.Data;

// Init web application builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container including database contexts, authorization, identity api endpoints, and controllers
//#if DEBUG
//builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("AppDb"));
//#else
var connectionString = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
//#endif
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
        await context.HttpContext.Response.WriteAsync("Too many requests - RateLimit = 30/min");
    };
});

// Get configuration sections from appsettings.json or env variables
var seedConfig = builder.Configuration.GetSection("SeedData");
var hostConfig = builder.Configuration.GetSection("HostSettings");

// Init application with builder
var app = builder.Build();

// Migrate Database then seed admin user and role if no exist
using(var scope  = app.Services.CreateScope())
{
    // get context and migrate
    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
    context!.Database.Migrate();

    // check and add role
    var roleStore = new RoleStore<IdentityRole>(context!);
    if (!context!.Roles.Any(r => r.Name == "Admin"))
    {
        await roleStore.CreateAsync(new IdentityRole { Name = "Admin" , NormalizedName = "ADMIN" });
    }

    // check and add user if email and password exist in config
    if (!String.IsNullOrEmpty(seedConfig.GetSection("AdminEmail").Value) && !String.IsNullOrEmpty(seedConfig.GetSection("AdminPassword").Value))
    {
        string adminEmail = seedConfig.GetSection("AdminEmail").Value!.ToString();
        if (!context!.Users.Any(u => u.Email == adminEmail))
        {
            var adminUser = new IdentityUser
            {
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                UserName = adminEmail,
                NormalizedUserName = adminEmail.ToUpper()
            };
            var passwordHasher = new PasswordHasher<IdentityUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, seedConfig.GetSection("AdminPassword").Value!);
            var userStore = new UserStore<IdentityUser>(context!);
            await userStore.CreateAsync(adminUser);
        }
        //context!.SaveChanges();

        // Init user manager and add user to role
        UserManager<IdentityUser>? userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>();
        IdentityUser? identityAdminUser = await userManager!.FindByEmailAsync(adminEmail);
        if (!await userManager.IsInRoleAsync(identityAdminUser!, "Admin"))
        {
            await userManager!.AddToRoleAsync(identityAdminUser!, "Admin");
        }

        context!.SaveChanges();
    }
}

// Use Rate Limiter
app.UseRateLimiter();

// Map identity api routes and require rate limits
app.MapIdentityApi<IdentityUser>().RequireRateLimiting("fixedLimits");

// Use Swagger, Swagger UI, ReDoc, and configure
app.UseSwagger(options =>
{
    options.PreSerializeFilters.Add((swagger, httpRequest) =>
    {
        // check for api url param in config else use default
        if (!String.IsNullOrEmpty(hostConfig.GetSection("ApiUrl").Value))
        {
            swagger.Servers.Add(new OpenApiServer
            {
                Url = hostConfig.GetSection("ApiUrl").Value
            });
        }
        else
        {
            swagger.Servers.Add(new OpenApiServer
            {
                Url = $"{httpRequest.Scheme}://{httpRequest.Host}"
            });
        }
    });
});
app.UseSwaggerUI(options =>
{ 
    options.DocumentTitle = "ToDo-API-Server - Swagger UI";
});
app.UseReDoc(options =>
{
    options.DocumentTitle = "ToDo-API-Server - API Docs";
});

// Use Https redirection and authorization
app.UseHttpsRedirection();
app.UseAuthorization();

// Map controllers and require rate limits
app.MapControllers().RequireRateLimiting("fixedLimits");

// Use Default and static files
app.UseDefaultFiles();
app.UseStaticFiles();

// Run application
app.Run();
