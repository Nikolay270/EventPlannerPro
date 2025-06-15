using EventPlannerPro.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    o.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
    o.Cookie.SameSite = SameSiteMode.Lax;     
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});


builder.Services.AddAntiforgery(o =>
{
    o.Cookie.SameSite = SameSiteMode.None;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.Configure<CookiePolicyOptions>(c =>
{
    c.MinimumSameSitePolicy = SameSiteMode.None;
    c.Secure = CookieSecurePolicy.Always;
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

string[] roles = { "Admin", "Participant" };
foreach (var r in roles)
    if (!await roleMgr.RoleExistsAsync(r))
        await roleMgr.CreateAsync(new IdentityRole(r));

const string adminEmail = "tenev131@gmail.com";
const string adminPassword = "exvZux5u*#J7yCs";

var admin = await userMgr.FindByEmailAsync(adminEmail);
if (admin == null)
{
    admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
    if ((await userMgr.CreateAsync(admin, adminPassword)).Succeeded)
        await userMgr.AddToRoleAsync(admin, "Admin");
}
else if (!await userMgr.IsInRoleAsync(admin, "Admin"))
{
    await userMgr.AddToRoleAsync(admin, "Admin");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
