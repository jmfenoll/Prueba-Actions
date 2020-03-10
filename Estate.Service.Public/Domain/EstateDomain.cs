//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using WayToCol.Common.Contracts;
//using WayToCol.Common.Contracts.Estates;
//using WayToCol.Estate.Service.Public.Controllers;
//using WayToCol.Estate.Service.Public.Repository;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using WayToCol.Agent.Service.Public.Repository;
using WayToCol.Common.Api.Helpers;
using WayToCol.Common.Contracts;
using WayToCol.Common.Contracts.Agents;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Contracts.Responses;
using WayToCol.Estate.Service.Public.Controllers;
using WayToCol.Estate.Service.Public.Repository;
using WayToCol.EstatStakeholder.Service.Public.Repository;

namespace WayToCol.Estate.Service.Public.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public class EstateDomain
    {
        private ILogger<EstateController> _logger;
        private IEstatePublicRepository _repEstate;
        private IConfiguration _config;
        private EstateStakeholderPublicMongoDbRepository _repStakeholder;
        private AgentPublicMongoDbRepository _repAgent;

        public EstateDomain(ILogger<EstateController> logger, IEstatePublicRepository rep, IConfiguration config, EstateStakeholderPublicMongoDbRepository repstakeHolder, AgentPublicMongoDbRepository repAgent)
        {
            _logger = logger;
            _repEstate = rep;
            _config = config;
            _repStakeholder = repstakeHolder;
            _repAgent = repAgent;
        }


        internal EstateDto Map(propiedadesPropiedadXml estateImport)
        {
            var estate = new EstateImportedToDto(estateImport).ConvertToDto();
            return estate;
        }

        internal EstateFileDto[] MapFiles(propiedadesPropiedadXml estateImport)
        {
            var files = GetPhotos(estateImport);
            return files.ToArray();
        }

        private List<EstateFileDto> GetPhotos(propiedadesPropiedadXml estate)
        {
            var photoProperties = GetPhotoProperties(estate);

            var listPhotos = new List<EstateFileDto>();
            foreach (var photo in photoProperties)
            {
                var file = new EstateFileDto
                {
                    url = photo.url,
                    id = ModelHelper.GetMD5(estate.id + photo.prop),
                    estateId = ModelHelper.GetMD5(estate.id),
                    name = ModelHelper.GetPhotoName(photo.url),
                    mimeType = ModelHelper.GetMimeType(photo.url)
                };
                listPhotos.Add(file);
            }
            return listPhotos;
        }

        private List<(string prop, string url)> GetPhotoProperties(propiedadesPropiedadXml estate)
        {
            var resp = new List<(string prop, string url)>();

            var props = estate.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name.StartsWith("foto"))
                {
                    var value = ((string)prop.GetValue(estate));
                    if (value != null)
                        resp.Add((prop.Name, value));
                }
            }
            return resp;
        }

        internal PaginationModel<EstateDto> Search(string term, int? page, int? pageSize)
        {
            

            var lucene = Utils.Lucene.LuceneFactory.GetInstance(_config);
            if (!pageSize.HasValue)
            {
                pageSize = _config.GetValue<int?>("Settings:ResultsPerPage") ?? 10;
            };

            var respLucene = lucene.SearchPaginated(term, page.Value, pageSize.Value);

            var dataEstate = _repEstate.Find(respLucene.id);


            var resp = new PaginationModel<EstateDto> {
                totalItems = respLucene.total,
                pageSize = pageSize.Value,
                page=page.Value, 
                data= dataEstate
            };

            return resp;
        }

        internal  ServerResponse<PaginationModel<AgentDto>> GetStakeholders(int? page, int? pagesize, string estateId, HttpContext httpContext)
        {
            var resp = new ServerResponse<PaginationModel<AgentDto>>();
            // TODO JMF: No podemos sacar el usuario activo
            var agentId = GetCurrentUser(httpContext);
            agentId = "5e5eba00c461710b68902d0d";

            var estate = _repEstate.Single(x => x.id == estateId);
            var agentsOfAgency = _repAgent.GetIdByEstate(estate.agencyId);
            if (!agentsOfAgency.Contains(agentId))
            {
                resp.AddErrorCode(StatusCodes.Status401Unauthorized,null);
                return resp;
            }

            if (!page.HasValue) page = 1;

            if (!pagesize.HasValue)
            {
                var resPerPage = _config.GetSection("Settings:ResultsPerPage").Value;
                if (resPerPage == null)
                    pagesize = 20;
                else
                    pagesize = int.Parse(resPerPage);
            }
            var pag = new PaginationModel<AgentDto> ();


            var listSkateHolder = _repStakeholder
                .All()
                .Where(sh => sh.estateId == estateId && sh.status == EstateStakeholderDto.enmStatus.accepted.ToString())
                .Select(sh => sh.agentId)
                .ToList();

            var listAgents = _repAgent.Find(listSkateHolder).Skip((page.Value - 1) * pagesize.Value).Take(pagesize.Value).ToList();

            pag.data = listAgents;
            pag.page = page.Value;
            pag.totalItems = listAgents.Count;
            //var a = _repAgent.All().ToList();
            //var b = _repAgent.All().Select(x => x.email).ToList();
            //var c = _repAgent.All().Where(x => listSkateHolder.Contains(x.id.ToString())).Select(x => x.email).ToList();





            //var j = _repAgent.All().Where(x => listSkateHolder.Contains(x.id.ToString())).ToList();

            //var listAgents = _repAgent.All().Where(x => listSkateHolder.Contains(x.id.ToString())).Select(x => x).ToList();
            resp.Content = pag;
            return resp;
        }

        private string GetCurrentUser(HttpContext httpContext)
        {
            string id = null;
            if (httpContext.User.Identity.IsAuthenticated)
                id = httpContext.User.Claims.Where(x => x.Type == "client_id").FirstOrDefault()?.Value;
            return id;
        }
    }
}
