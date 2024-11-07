using Dynamics.DataAccess;
using Dynamics.DataAccess.Repository;
using Dynamics.Models.Models;
using Dynamics.Services;
using Dynamics.Utility;
using Dynamics.Utility.Mapper;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Resources;
using Serilog;
using Cloudinary = CloudinaryDotNet.Cloudinary;


namespace Dynamics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            // Add cache to the container, allow admin dashboard get the latest data
            // working with other services as well
            builder.Services.AddMemoryCache();

            // Add cache to the container, allow admin dashboard get the latest data
            // working with other services as well
            builder.Services.AddMemoryCache();

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
            // Add database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            }, ServiceLifetime.Transient);
            
            // Identity and roles
            builder.Services
                .AddIdentity<User, IdentityRole<Guid>>(options =>
                {
                    options.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ àáảãạâầấẩẫậăằắẳẵặèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ()";
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                })
                .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2); // Lockout for 2 minutes
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true; // New user can be locked out as well
            });
            // Configure authentication cookie
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true; // Auto re-new authentication cookie when almost ended
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie expires after 60 minutes
                });

            // Add authorization policy
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(RoleConstants.Admin));
            });


            // Add authorization policy
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(RoleConstants.Admin));
            });


            // Repos here
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
            builder.Services.AddScoped<IOrganizationVMService, OrganizationVMService>();
            builder.Services
                .AddScoped<IUserToOragnizationTransactionHistoryVMService,
                    UserToOragnizationTransactionHistoryVMService>();
            builder.Services.AddScoped<IProjectVMService, ProjectVMService>();
            builder.Services.AddScoped<IOrganizationToProjectHistoryVMService, OrganizationToProjectHistoryVMService>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            builder.Services.AddScoped<IRequestRepository, RequestRepository>();
            builder.Services.AddScoped<IReportRepository, ReportRepository>();
            // Project repos
            builder.Services.AddScoped<IProjectResourceRepository, ProjectResourceRepository>();
            builder.Services.AddScoped<IProjectHistoryRepository, ProjectHistoryRepository>();
            builder.Services
                .AddScoped<IOrganizationToProjectTransactionHistoryRepository,
                    OrganizationToProjectTransactionHistoryRepository>();
            builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
            builder.Services
                .AddScoped<IUserToProjectTransactionHistoryRepository, UserToProjectTransactionHistoryRepository>();
            // Organization repos
            builder.Services.AddScoped<IOrganizationMemberRepository, OrganizationMemberRepository>();
            builder.Services.AddScoped<IOrganizationResourceRepository, OrganizationResourceRepository>();
            builder.Services
                .AddScoped<IUserToOrganizationTransactionHistoryRepository,
                    UserToOrganizationTransactionHistoryRepository>();
            // Wallet repos
            builder.Services.AddScoped<IWalletRepository, WalletRepository>();
            builder.Services.AddScoped<IUserWalletTransactionRepository, UserWalletTransactionRepository>();
            // Automapper
            builder.Services.AddAutoMapper(typeof(MyMapper));
            // Add custom services
            builder.Services.AddScoped<ITransactionViewService, TransactionViewService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IRequestService, RequestService>();
            builder.Services.AddScoped<IOrganizationService, OrganizationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            // VNPAY Service
            builder.Services.AddTransient<IVnPayService, VnPayService>();
            builder.Services.AddScoped<IPagination, Pagination>();
            // Wallet service
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<IUserWalletTransactionService, UserWalletTransactionService>();
            // Add email sender
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            // Cloudinary
            builder.Services.AddSingleton<CloudinaryUploader>();

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

            // Add serilog for debugging
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                // .WriteTo.File("Logs/Logs.txt", rollingInterval: RollingInterval.Minute)
                .MinimumLevel.Information() // You can change this one so that it filters out stuff
                .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger); // This one make it so that ASP will use it

            var app = builder.Build();
            // Redirect user to 404 page if not found
            app.Use(async (ctx, next) =>
            {
                await next();

                if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
                {
                    // Re-execute the request so the user gets the error page
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

            app.MapControllers();
            app.UseSession();
            app.UseRouting();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // These guys must be in order authentication => authorization to work
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}