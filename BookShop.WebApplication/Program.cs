using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookShop.WebApplication.Data;
using BookShop.WebApplication.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookShopWebApplicationContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionToSQL") ?? throw new InvalidOperationException("Connection string 'BookShopWebApplicationContextConnection' not found.")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<BookShopWebApplicationContext>();

builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
