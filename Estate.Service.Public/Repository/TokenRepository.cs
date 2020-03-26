using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WayToCol.Common.Contracts.Responses;

namespace WayToCol.EstateFile.Service.Repository
{
    public class TokenRepository : ITokenRepository
    {
        private IConfiguration _config;

        public TokenRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<ServerResponse<string>> GetTokenAsync(dynamic data, TimeSpan timeSpan)
        {
            var resp = new ServerResponse<string>();

            var strJson = JsonConvert.SerializeObject(data);

            var _urlToken = _config.GetValue<string>("UrlServices:TokenService");
            if (_urlToken == null)
            {
                resp.AddError("No existe la variable 'UrlToken' en el AppSettings");
                return resp;
            }

            var client = new RestClient(_urlToken);
            var req = new RestRequest(Method.POST);
            req.AddHeader("Content-Type", "text/plain");
            req.AddHeader("accept", "application/json");

            req.AddJsonBody(strJson);

            DateTime expiredDate = DateTime.Now.Add(timeSpan);

            req.AddParameter("expiredAt", expiredDate.ToString("yyyy-MM-dd HH:mm:ss"), ParameterType.QueryStringWithoutEncode);
            // curl -X POST "https://devprivate.waytocol.com/token/" -H "accept: application/json" -H "Content-Type: text/plain" -d "{\"estateId\":\"163d70cc701bf9fa14ba76aba0cb5f46\"}"

            var restResp = await client.ExecuteAsync(req);
            if (restResp.StatusCode != HttpStatusCode.OK)
            {
                resp.AddError("No se ha podido descargar el token");
                return resp;
            }

            var respDyn = JObject.Parse(restResp.Content);
            resp.Content = respDyn.Value<string>("id");
            return resp;
        }
    }
}
