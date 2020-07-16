using IdentityServer.Core.Context;
using IdentityServer.Core.Models;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IdentityServer.Core
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            const string connectionString = @"Filename=identity.db";
            //const string connectionString =   @"Data Source=(LocalDb)\MSSQLLocalDB;database=Test.IdentityServer4.EntityFramework;trusted_connection=yes;";
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(builder => builder.UseSqlite(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)));

            services.AddIdentity<ApplicationUser, IdentityRole>()
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

            IIdentityServerBuilder ids = services.AddIdentityServer(options =>
                   {
                       options.Events.RaiseErrorEvents = true;
                       options.Events.RaiseInformationEvents = true;
                       options.Events.RaiseFailureEvents = true;
                       options.Events.RaiseSuccessEvents = true;
                        // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                        options.EmitStaticAudienceClaim = true;
                   })
                    .AddDeveloperSigningCredential();

            ids.AddOperationalStore(options => options.ConfigureDbContext =
                                            builder => builder.UseSqlite(
                                                connectionString,
                                                sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
            .AddConfigurationStore(options => options.ConfigureDbContext =
                                        builder => builder.UseSqlite(
                                            connectionString,
                                            sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)));

            ids.AddAspNetIdentity<ApplicationUser>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
            })
            .AddCookie("cookie");


            services.AddAuthentication().AddGoogle(opts => {
                opts.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                opts.ClientId = "";
                opts.ClientSecret = "";             
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            Seeder.InitializeDbTestData(app);

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}
