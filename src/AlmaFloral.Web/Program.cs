using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using AlmaFloral.Web.Services;
using AlmaFloral.Web.Data;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AlmaFloralDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AlmaFloral")
        ?? throw new InvalidOperationException(
            "No se encontró la conexión AlmaFloral.")));

builder.Services.AddScoped<PedidoService>();

builder.Services.AddScoped<GestionService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => {
 o.LoginPath="/Cuenta/Ingresar";o.AccessDeniedPath="/Cuenta/Denegado";
 o.Cookie.Name="AlmaFloral.Acceso";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Lax;
 o.Cookie.SecurePolicy=builder.Environment.IsDevelopment()?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always;
 o.ExpireTimeSpan=TimeSpan.FromMinutes(30);o.SlidingExpiration=true;
 o.Events.OnValidatePrincipal=async c=>{
  var db=c.HttpContext.RequestServices.GetRequiredService<AlmaFloralDbContext>();
  if(!int.TryParse(c.Principal?.FindFirstValue(ClaimTypes.NameIdentifier),out var id) ||
    !await db.Usuarios.AnyAsync(u=>u.UsuarioId==id&&u.Activo&&u.Rol=="Administrador"))c.RejectPrincipal();
 };
});
builder.Services.AddRateLimiter(o=>{
 o.RejectionStatusCode=StatusCodes.Status429TooManyRequests;
 o.AddPolicy("formularios",c=>RateLimitPartition.GetFixedWindowLimiter(
  c.Connection.RemoteIpAddress?.ToString()??"local",_=>new FixedWindowRateLimiterOptions{
   PermitLimit=20,Window=TimeSpan.FromMinutes(1),QueueLimit=0,AutoReplenishment=true}));
});
var app = builder.Build();
if(app.Environment.IsDevelopment()) await AdministradorInicial.CrearAsync(app.Services,app.Configuration);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
