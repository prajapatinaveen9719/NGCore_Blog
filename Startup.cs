using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NGCore_Blog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NGCore_Blog.Helpers;
using System.Text;

namespace NGCore_Blog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            //Enable Cors my method
            services.AddCors(Options =>
            {
                //Options.AddPolicy("EnableCORS", builder =>
                // {
                //     builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(hostName => true)
                // });

                Options.AddPolicy("EnableCORS",
                   builder => builder
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials()
                   .SetIsOriginAllowed(hostName => true));
            });

            //services to connect with database
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //specifying we are going to use Identity Framework to validate user
            services.AddIdentity<IdentityUser, IdentityRole>(Options =>
            {
                Options.Password.RequireDigit = true;
                Options.Password.RequiredLength = 6;
                Options.Password.RequireNonAlphanumeric = true;
                Options.Password.RequireUppercase = true;
                Options.Password.RequireLowercase = true;
                Options.User.RequireUniqueEmail = true;
                //lockout settings
                Options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                Options.Lockout.MaxFailedAccessAttempts = 5;
                Options.Lockout.AllowedForNewUsers = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            //configuration Strongly typed Settings
            //get all values from appsetting.json
            var appSettingsSection = Configuration.GetSection("AppSettingsClass");
            //map all values of appsettings to Hard Coded Class AppSettingsClass
            services.Configure<AppSettingsClass>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettingsClass>();
            //encoded secret key
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);


            //install  Microsoft.AspNetCore.Authentication.JwtBearer v 3.0.1
            //in order to acess the values in appsetting.json files you need to use hardcoded class
            //Authentication Middleware
            services.AddAuthentication(Options =>
            {
                Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                Options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, Options =>
            {
                Options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = appSettings.Site,
                    ValidAudience = appSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                };
            });


            //"RequiredLoggedIn",policy=>policy.RequiredRole("Admin","Customer","Moderator").RequiredAuthentication()
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequiredLoggedIn", policy => policy.RequireRole("Admin", "Customer", "Moderator").RequireAuthenticatedUser());
                options.AddPolicy("RequiredAdministrationRole", policy => policy.RequireRole("Admin").RequireAuthenticatedUser());
            });
            //Conditions :User Should Be Authenticated . 2) User must Authorized 
            //for this we require two policy 1)for Admin 2)For Customer




        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //tell asp.net use Cors
            app.UseCors("EnableCORS");

          


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
            // To learn more about options for serving an Angular SPA from ASP.NET Core,
            // see https://go.microsoft.com/fwlink/?linkid=864501

            spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
