using Dynamics.DataAccess;
using Dynamics.DataAccess.Repository;
using Dynamics.Services;
using Dynamics.Utility;
using Dynamics.Utility.Mapper;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace Dynamics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Information()
                .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // Add service and scope for Google auth
            builder.Services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                googleOptions.Scope.Add(Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoProfile);
                googleOptions.Scope.Add(Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoEmail);

                // Get user profile
                googleOptions.ClaimActions.MapJsonKey("picture", "picture");
            });
            // Add service for notification
            builder.Services.AddSignalR();
            // Add database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDbContextConnection"));
            });

            // Identity and roles
            builder.Services
                .AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            // Configure authentication cookie
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true; // Auto re-new authentication cookie when almost ended
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie expires after 60 minutes
                });
            // Repos here
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
            builder.Services.AddScoped<IOrganizationVMService, OrganizationVMService>();
            builder.Services.AddScoped<IUserToOragnizationTransactionHistoryVMService, UserToOragnizationTransactionHistoryVMService>();
            builder.Services.AddScoped<IProjectVMService, ProjectVMService>();
            builder.Services.AddScoped<IOrganizationToProjectHistoryVMService, OrganizationToProjectHistoryVMService>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            builder.Services.AddScoped<IRequestRepository, RequestRepository>();
            // Project repos
            
            builder.Services.AddScoped<IProjectResourceRepository, ProjectResourceRepository>();
            builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
            builder.Services
                .AddScoped<IUserToProjectTransactionHistoryRepository,
                    UserToProjectTransactionHistoryRepository>();
            // Organization repos
            builder.Services.AddScoped<IOrganizationMemberRepository, OrganizationMemberRepository>();
            builder.Services.AddScoped<IOrganizationResourceRepository, OrganizationResourceRepository>();
            builder.Services
                .AddScoped<IUserToOrganizationTransactionHistoryRepository,
                    UserToOrganizationTransactionHistoryRepository>();
            // Automapper
            builder.Services.AddAutoMapper(typeof(MyMapper));
            // Add custom services
            builder.Services.AddScoped<ITransactionViewService, TransactionViewService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IPagination, Pagination>();
            // Add email sender
            builder.Services.AddScoped<IEmailSender, EmailSender>();

            builder.Services.AddControllersWithViews()
            .AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
            
            // Configure default routes (This should be after configured the Identity)
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            // Enable razor page
            builder.Services.AddRazorPages();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
            });

            var app = builder.Build();
            // Redirect user to 404 page if not found
            app.Use(async (ctx, next) =>
            {
                await next();

                if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
                {
                    //Re-execute the request so the user gets the error page
                    string originalPath = ctx.Request.Path.Value;
                    ctx.Items["originalPath"] = originalPath;
                    ctx.Request.Path = "/error/PageNotFound";
                    await next();
                }
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            // Specify hub for clients to connect to
            app.MapHub<NotificationHub>("/notification");
                
            app.MapControllers();
            app.UseSession();
            app.UseRouting();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthorization();
            app.UseAuthentication();

            app.MapRazorPages();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Homepage}/{id?}");

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}