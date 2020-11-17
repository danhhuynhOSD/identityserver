using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static IdentityServer.InMemoryConfig;

namespace IdentityServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            const string sqlString = @"server=127.0.0.1; port=65350 database =IdentityServer4Config; Integrated Security=true;";

            services.AddIdentityServer(opt =>
            {
                opt.Authentication.CookieLifetime = TimeSpan.FromMinutes(1);
            })
                //.AddInMemoryApiScopes(InMemoryConfig.GetApiScopes())
                //.AddInMemoryApiResources(InMemoryConfig.GetApiResources())
                //.AddInMemoryIdentityResources(InMemoryConfig.GetIdentityResources())
                //.AddInMemoryClients(InMemoryConfig.GetClients())
                //.AddProfileService<CustomProfileService>()
                .AddTestUsers(InMemoryConfig.GetUsers())
                .AddDeveloperSigningCredential()
                .AddConfigurationStore(opt =>
                {
                    opt.ConfigureDbContext = c => c.UseNpgsql(sqlString,
                        sql => sql.MigrationsAssembly(migrationAssembly));
                })
                .AddOperationalStore(opt =>
                {
                    opt.ConfigureDbContext = o => o.UseNpgsql(sqlString,
                        sql => sql.MigrationsAssembly(migrationAssembly));
                });

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
