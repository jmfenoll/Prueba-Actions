using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayToCol.EstateFile.Service.Public.Controllers
{
    public class WayToColControllerBase : ControllerBase
    {
        protected string GetCurrentUserIdFromContext()
        {
            var context = HttpContext;
            string client_id = null;
            List<System.Security.Claims.ClaimsIdentity> claimsIdentity = context.User.Identities.ToList();
            if (claimsIdentity != null)
            {
                client_id = claimsIdentity[0].Claims.Where(x => x.Type == "client_id").FirstOrDefault().Value;
            }
            return client_id;
        }
    }
}
