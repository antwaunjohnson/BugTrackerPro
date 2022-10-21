using BugTrackerPro.Data;
using BugTrackerPro.Models;
using BugTrackerPro.Services;
using BugTrackerPro.Services.Factories;
using BugTrackerPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//var connectionString = DataUtility.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(DataUtility.GetConnectionString(builder.Configuration), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<BTProUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<BTProUserClaimsPrincipalFactory>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// custom services
builder.Services.AddScoped<IBTProRolesService, BTProRolesService>();
builder.Services.AddScoped<IBTProCompanyInfoService, BTProCompanyInfoService>();
builder.Services.AddScoped<IBTProProjectService, BTProProjectService>();
builder.Services.AddScoped<IBTProTicketService, BTProTicketService>();
builder.Services.AddScoped<IBTProTicketHistoryService, BTProTicketHistoryService>();
builder.Services.AddScoped<IBTProNotificationService, BTProNotificationService>();
builder.Services.AddScoped<IBTProInviteService, BTProInviteService>();
builder.Services.AddScoped<IBTProFileService, BTProFileService>();
builder.Services.AddScoped<IBTProLookupService, BTProLookupService>();


builder.Services.AddScoped<IEmailSender, BTProEmailService>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddControllersWithViews();


var app = builder.Build();

await DataUtility.ManageDataAsync(app, builder.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
