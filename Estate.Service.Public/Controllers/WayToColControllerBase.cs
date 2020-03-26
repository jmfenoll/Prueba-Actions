using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using WayToCol.Agent.Service.Repository;

namespace WayToCol.EstateFile.Service.Public.Controllers
{
    public class WayToColControllerBase : ControllerBase
    {
        private IConfiguration _config;
        private IAgentRepository _agentRep;

        public WayToColControllerBase(IConfiguration config, IAgentRepository agentRep)
        {
            _config = config;
            _agentRep = agentRep;
        }
        protected string GetCurrentUserIdFromContext()
        {
            var client = new RestClient(_config["oauth:userInfoUrl"]);
            var request = new RestRequest(Method.GET);
            var authorizations = HttpContext.Request.Headers["Authorization"];
            var authorization = authorizations[0].ToString();
            request.AddHeader("Authorization", authorization);
            request.AddParameter("scope", "api_public");
            IRestResponse response = client.Execute(request);
            return "4eafe8a6-4820-4083-84fd-f29e64c4d224";

            Console.WriteLine(response.Content);


            var context = HttpContext;
            string client_id = null;
            List<System.Security.Claims.ClaimsIdentity> claimsIdentity = context.User.Identities.ToList();
            if (claimsIdentity != null)
            {
                client_id = claimsIdentity[0].Claims.Where(x => x.Type == "client_id").FirstOrDefault().Value;
            }
            return client_id;
        }

        protected string GetCurrentAgentIdFromContext()
        {
            var currentUserId = GetCurrentUserIdFromContext();
            return _agentRep.GetAgentByUserId(currentUserId);
        }

    }
}
