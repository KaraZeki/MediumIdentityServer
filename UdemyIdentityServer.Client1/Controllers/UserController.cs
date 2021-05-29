using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace UdemyIdentityServer.Client1.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration; //appsetting.json a böyle ulaşıyoruz.

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task LogOut()
        {
            await HttpContext.SignOutAsync("Cookies"); //clientten  çıkış yapar , "Cookies" startupta tanımlı
            await HttpContext.SignOutAsync("oidc"); //openid den çıkış yapr , "oidc" o da startupta tanımlı. EĞER BUNU KALDIRIRSAM SADECE Client1 DEN ÇIKIŞ YAPARIM.ayrıca yönledirmeyi de bu yapıyor
        }

        public async Task<IActionResult> GetRefreshToken()
        {
            HttpClient httpClient = new HttpClient();
            var disco = await httpClient.GetDiscoveryDocumentAsync("http://localhost:5001"); //Identity serverdaki 

            if (disco.IsError)
            {
                //loglama yap
            }

            var refreshToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken); //cookie içerisindeki refresh token alyorum.

            RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest(); 
            refreshTokenRequest.ClientId = _configuration["Client1Mvc:ClientId"]; //buradaki token bilgilerine appsetting.json dakki tanımladığımız clien bilgileri
            refreshTokenRequest.ClientSecret = _configuration["Client1Mvc:ClientSecret"];
            refreshTokenRequest.RefreshToken = refreshToken;
            refreshTokenRequest.Address = disco.TokenEndpoint;

            var token = await httpClient.RequestRefreshTokenAsync(refreshTokenRequest);

            if (token.IsError)
            {
                //yönlendirme yap
            }
            var tokens = new List<AuthenticationToken>()
            {
                new AuthenticationToken{ Name=OpenIdConnectParameterNames.IdToken,Value= token.IdentityToken},
                      new AuthenticationToken{ Name=OpenIdConnectParameterNames.AccessToken,Value= token.AccessToken},
                            new AuthenticationToken{ Name=OpenIdConnectParameterNames.RefreshToken,Value= token.RefreshToken},
                                  new AuthenticationToken{ Name=OpenIdConnectParameterNames.ExpiresIn,Value= DateTime.UtcNow.AddSeconds(token.ExpiresIn).ToString("o", CultureInfo.InvariantCulture)} //expire geçerli süreyi belirtir
            };

            var authenticationResult = await HttpContext.AuthenticateAsync(); //aouth verilerini alıyoruz

            var properties = authenticationResult.Properties; //propertileri alıyoruz. 

            properties.StoreTokens(tokens); //propertieslerde artık yeni token lar set edilmiş oluyor.

            await HttpContext.SignInAsync("Cookies", authenticationResult.Principal, properties); //kullanıcının bilgilerinden oluşan bir kimlik ver

            return RedirectToAction("Index");
        }


        //Admin rolüne sahip kullanıcılar tek erişebilir.
        [Authorize(Roles = "admin")]
        public IActionResult AdminAction()
        {
            return View();
        }


        //Admin ve customer rolüne sahip kullanıcılar tek erişebilir.
        [Authorize(Roles = "admin,customer")]
        public IActionResult CustomerAction()
        {
            return View();
        }
    }
}