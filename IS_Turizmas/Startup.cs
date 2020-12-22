using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using IS_Turizmas.Models;
using IS_Turizmas.Identity;
using Microsoft.AspNetCore.Identity;
using IS_Turizmas.SupportClasses;
using Microsoft.AspNetCore.Authentication;
using IS_Turizmas.Controllers;

namespace IS_Turizmas
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();

            services.AddMvc(options => { options.EnableEndpointRouting = false;
                options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
                    (x) => "Reikšmė turi būti skaičius.");
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                    (x) => "Reikšmė negali būti tuščia.");
            });


            services.AddDbContext<ApplicationDbContext>();

            services.AddIdentity<RegistruotiVartotojai, VartotojoPlanai>(config =>
            {
                config.SignIn.RequireConfirmedEmail = false;
                config.Password.RequireUppercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequiredLength = 8;
            }).AddDefaultTokenProviders().AddErrorDescriber<CustomIdentityErrorDescriber>();

            services.AddTransient<IUserStore<RegistruotiVartotojai>, UserStore>();

            services.AddTransient<IRoleStore<VartotojoPlanai>, RoleStore>();

            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

            services.AddHostedService<AccountEmailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                /*if (s.UserManager.FindByNameAsync("dev").Result == null)
                {
                    var result = s.UserManager.CreateAsync(new RegistruotiVartotojai
                    {
                        Slapyvardis = "dev",
                        ElPastas = "dev@app.com",
                        AktyvumoTaskai = 0,
                        Pavardė = "B",
                        Vardas = "A",
                        RegistracijosData = DateTime.Now,
                        PrisijungimoData = DateTime.Now
                    }, "Aut94L#G-a").Result;
                }*/
            }

            /*app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });*/

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=HOME}/{action=MainIndex}/{id?}");
            });
        }
    }
}
