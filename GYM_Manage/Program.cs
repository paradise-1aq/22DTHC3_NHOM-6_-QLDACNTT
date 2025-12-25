using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GYM_Manage.Data;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<GYM_DBcontext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

// Middleware kiểm tra đăng nhập cho khu vực Admin
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    
    // Kiểm tra nếu request đi vào khu vực Admin
    if (path.StartsWith("/Admin") || path.StartsWith("/admin"))
    {
        // Kiểm tra session đăng nhập
        var userRole = context.Session.GetString("UserRole");
        
        // Nếu chưa đăng nhập hoặc không phải Admin/Staff thì chuyển hướng về trang đăng nhập
        if (string.IsNullOrEmpty(userRole) || (userRole != "Admin" && userRole != "Staff"))
        {
            context.Response.Redirect("/Auth/Login");
            return;
        }
    }
    
    await next();
});

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ... existing code ...
app.Run();