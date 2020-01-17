using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WayToCol.Common.Contracts.Estates;

// TODO: Esto hay que ponerlo en Nuget, pero está el tema del Log
namespace WayToCol.Estate.Service.Public
{
    class HttpProxy2Api
    {
        internal async Task<HttpResponseMessage> PutAsync<T>(string url, T rawObj)
        {
            var objJson = JsonConvert.SerializeObject(rawObj);
            var content = new StringContent(objJson);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PutAsync(url, content);
                    return response;
                }
                catch (HttpRequestException ex)
                {
                    Log.Error(ex, "No se ha podido conectar al servicio");
                    return new HttpResponseMessage(statusCode: System.Net.HttpStatusCode.NotFound);

                }

                catch (Exception ex)
                {
                    Log.Error(ex, "Error no controlado");
                    return new HttpResponseMessage(statusCode: System.Net.HttpStatusCode.InternalServerError);

                }

            }


        }

        internal async Task<HttpResponseMessage> DeleteAsync(string url, string id)
        {
            var queryString = new QueryString();
            queryString = queryString.Add("id", id);
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync(url + queryString.ToUriComponent());
                return response;
            }
        }
    }
}
