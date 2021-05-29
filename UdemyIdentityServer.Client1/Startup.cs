using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UdemyIdentityServer.Client1.Services;

namespace UdemyIdentityServer.Client1
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
            services.AddHttpContextAccessor();
            services.AddScoped<IApiResourceHttpClient, ApiResourceHttpClient>();

            services.AddAuthentication(opts =>
            {
                opts.DefaultScheme = "Cookies";
                opts.DefaultChallengeScheme = "oidc";

            }).AddCookie("Cookies", opts =>
            {
                opts.AccessDeniedPath = "/Home/AccessDenied"; //yetkisi olmayan bir kullan�c�n giri� yapmas� durumunda girece�i sayfa
            }).AddOpenIdConnect("oidc", opts =>
            {
                opts.SignInScheme = "Cookies";
                opts.Authority = "http://localhost:5001";
                opts.ClientId = "Client1-Mvc"; //config.cs belirtilen client bilgileri
                opts.ClientSecret = "secret";
                opts.ResponseType = "code id_token";
                opts.GetClaimsFromUserInfoEndpoint = true; //Burda claimste tan�mlad���m�z �zel bilgiler getirilmesine izin veriliyor.
                opts.SaveTokens = true; //Ba�ar�l� bir aouth i�leminden sonra acces token id_token  ve refresh token gibi tokenler kay�t edilir
                opts.Scope.Add("api1.read"); //bu iste�i yapt���mda Aouthserver.Config.cs de belirlenen izni talep etmi� oluyorum.
                opts.Scope.Add("offline_access");//ofline eri�imde refresh token alabilmek i�in config.cs de de tan�ml�. IdentityServerConstants.StandardScopes.OfflineAccess olaark 
                opts.Scope.Add("CountryAndCity");//Config.cs de tan�m� County bilgileri
                opts.Scope.Add("Roles"); //Config.cs de tan�ml� rol bilgiler
                opts.ClaimActions.MapUniqueJsonKey("country", "country");//Bu metot ile config.cs country resources'i json i�erisine maplemi� olduk
                opts.ClaimActions.MapUniqueJsonKey("city", "city");//Bu metot ile config.cs country resources'i json i�erisine maplemi� olduk ayn�
                opts.ClaimActions.MapUniqueJsonKey("role", "role");//Bu metot ile config.cs country resources'i json i�erisine maplemi� olduk ayn�
              
                opts.RequireHttpsMetadata = false;//zeki metadata hatas� veriyor.


                //bu client giri� yapt���nda bir rol ismini ver.
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    RoleClaimType = "role"
                };
            });

            services.AddControllersWithViews();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}