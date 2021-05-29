using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UdemyIdentityServer.Client1.Services
{

    //Bu clas servisi sayaesinde http token her seferinde yazılmayacak actionlarda. Her ihtiyaç olduğunda burdan çekilecek
    //DİKKAT BU SERVİSLERİ starup.cs de Scoped Etmeyi unutma
    public class ApiResourceHttpClient : IApiResourceHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor; //cotroller dışındaysak _httpContextAccessor ile httpcontext e ulaşıyoruz

        private HttpClient _client;
        public ApiResourceHttpClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
        }

        public async Task<HttpClient> GetHttpClient()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            _client.SetBearerToken(accessToken); //Token ı set ettik.

            return _client;
        }
    }
}