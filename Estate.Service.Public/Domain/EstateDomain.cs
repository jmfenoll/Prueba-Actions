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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using WayToCol.Agent.Service.Repository;
using WayToCol.Common.Api.Helpers;
using WayToCol.Common.Contracts;
using WayToCol.Common.Contracts.Agents;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Contracts.Estates.Import;
using WayToCol.Common.Contracts.Responses;
using WayToCol.Common.MongoDbRepository.Helpers;
using WayToCol.Estate.Service.Public.Controllers;
using WayToCol.Estate.Service.Public.Repository;
using WayToCol.EstateFile.Service.Repository;
using WayToCol.EstatStakeholder.Service.Public.Repository;

namespace WayToCol.Estate.Service.Public.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public class EstateDomain
    {
        private const int pageSizeDefault= 10;
        private ILogger<EstateController> _logger;
        private IEstateRepository _estateRep;
        private IConfiguration _config;
        private IEstateStakeholderRepository _stakeholderRep;
        private IAgentRepository _agentRep;
        private ITokenRepository _tokenRep;

        public EstateDomain(
            ILogger<EstateController> logger, 
            IEstateRepository estateRep, 
            IConfiguration config, 
            IEstateStakeholderRepository stakeHolderRep, 
            IAgentRepository agentRep,
            ITokenRepository tokenRep)
        {
            _logger = logger;
            _estateRep = estateRep;
            _config = config;
            _stakeholderRep = stakeHolderRep;
            _agentRep = agentRep;
            _tokenRep = tokenRep;
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
                    id = DataHelper.GetMd5(estate.id + photo.prop),
                    estateId = DataHelper.GetMd5(estate.id),
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
            page = page ?? 1;
            if (!pageSize.HasValue) pageSize = _config.GetValue<int?>("Settings:ResultsPerPage") ?? pageSizeDefault;

            var lucene = Utils.Lucene.LuceneFactory.GetInstance(_config);
            
            var respLucene = lucene.SearchPaginated(term, page.Value, pageSize.Value);

            var dataEstate = _estateRep.Find(respLucene.id);

            var resp = new PaginationModel<EstateDto> {
                totalItems = respLucene.total,
                pageSize = pageSize.Value,
                page=page.Value, 
                data= dataEstate
            };

            return resp;
        }

        internal  ServerResponse<PaginationModel<AgentDto>> GetStakeholders(int? page, int? pageSize, string estateId,string agentId)
        {
            page = page ?? 1;
            if (!pageSize.HasValue) pageSize = _config.GetValue<int?>("Settings:ResultsPerPage") ?? pageSizeDefault;

            var resp = new ServerResponse<PaginationModel<AgentDto>>();
             
            var estate = _estateRep.Single(x => x.id == estateId);
            var agentsOfAgency = _agentRep.GetIdByEstate(estate.agencyId);
            if (!agentsOfAgency.Contains(agentId))
            {
                resp.AddErrorCode(StatusCodes.Status401Unauthorized,null);
                return resp;
            }

            var pag = new PaginationModel<AgentDto> ();


            var listSkateHolder = _stakeholderRep
                .All()
                .Where(sh => sh.estateId == estateId && sh.status == EstateStakeholderDto.enmStatus.accepted.ToString())
                .Select(sh => sh.agentId)
                .ToList();

            var listAgents = _agentRep.Find(listSkateHolder).Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();

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

        internal async Task<ServerResponse<string>> ConfirmAsync(string estateId, string token, IEstateStakeholderRepository stakeRep)
        {
            var resp = new ServerResponse<string>();

            var respToken = await GetInfoTokenAsync(token);

            if (respToken.estateId != estateId)
            {
                resp.AddErrorCode(StatusCodes.Status400BadRequest, "No coincide el Estate de la URL con el Estate del Token");
                return resp;
            }

            string agentId = respToken.agentId;

            EstateStakeholderDto stakeHolder = stakeRep.Single(x => x.estateId == estateId && x.agentId == agentId);
            if (stakeHolder == null)
            {
                resp.AddErrorCode(StatusCodes.Status400BadRequest, "No se puede encontrar el stakeholder en base de datos");
                return resp;
            }

            stakeHolder.status = EstateStakeholderDto.enmStatus.accepted.ToString();
            stakeHolder.acceptedDate = DateTime.Now;
            stakeHolder.updateDate = DateTime.Now;
            await stakeRep.UpdateAsync(stakeHolder);
            return resp;

        }

        private async Task<(string estateId, string agentId)> GetInfoTokenAsync(string token)
        {
            (string estateId, string agentId) resp = (null, null);

            var _urlToken = _config.GetValue<string>("UrlToken");
            if (_urlToken == null)
                throw new ApplicationException("No existe la variable 'UrlToken' en el AppSettings");

            var client = new RestClient(_urlToken);
            var req = new RestRequest($"/{token}/consume", Method.PUT);
            var respToken = await client.ExecuteAsync(req);
            if (respToken.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("No se ha podido descargar el token");
            }

            dynamic respDyn = JObject.Parse(respToken.Content);
            string id = respDyn.id;
            string dataJson = respDyn.data;
            dynamic data = JObject.Parse(dataJson);
            string agentid = data.agentId;
            resp.agentId = data.agentId;
            resp.estateId = data.estateId;

            return resp;
        }

        // Todo JMF: El Stakeholder no debería de ser otro servicio?
        // TODO JMF: Aparte de las appSettings por entornos, ¿Convendría tener un único fichero con TODAS las URL?
        internal ServerResponse DeleteStakeholder(HttpContext httpContext, string estateId, string stakeholderId, string currentAgentId)
        {
            var resp = new ServerResponse();
            var estate = _estateRep.Single(x => x.id == estateId);
            // TODO JMF: No deberíamos de hacer esto
            var agentsOfAgency = _agentRep.GetIdByEstate(estate.agencyId);
            if (!agentsOfAgency.Contains(currentAgentId))
            {
                resp.AddErrorCode(StatusCodes.Status401Unauthorized, null);
                return resp;
            }
            _stakeholderRep.DeleteAgent(stakeholderId);
            return resp;
        }

        internal async Task<ServerResponse> ShareTo(string estateId, string[] agentsId, string currentAgentId)
        {
            var resp = new ServerResponse();
            // Verify that AgentConnected belongs to Agency
            var estate = await _estateRep.GetByIdAsync(estateId);
            var agentInEstate = _agentRep.Count(x => x.id == currentAgentId && x.agencyId == estate.agencyId);
            if (agentInEstate == 0)
                resp.AddError("The connected agent can't share an estate that doesn't belong to other agency");

            if (resp.Ok)
            {
                // Get all the stakeholders in the array
                var skateHoldersDb = _stakeholderRep.All().Where(x => x.estateId == estateId && agentsId.Contains(x.agentId)).ToList();

                foreach (var agentId in agentsId)
                {
                    var respEachAgent = new ServerResponse();

                    dynamic data = new ExpandoObject();
                    data.estateId = estateId;
                    data.estateId = agentId;

                    var respToken = await _tokenRep.GetTokenAsync(data, new TimeSpan(1,0,0,0));

                    var stakeHolder = new EstateStakeholderDto();

                    if (!respToken.Ok)
                    {
                        resp.AddResponse(respToken);
                        continue;

                    }
                    var token = respToken.Content;
                    
                    var existSkateHolder = skateHoldersDb.Where(x => x.agentId == agentId && x.estateId == estateId).FirstOrDefault();
                    if (existSkateHolder == null)
                    {
                        // Si no existe lo inserta
                        stakeHolder = new EstateStakeholderDto()
                        {
                            id = DataHelper.GetMd5(estateId + agentId),
                            agentId = agentId,
                            estateId = estateId,
                            createDate = DateTime.Now,
                            updateDate = DateTime.Now,
                            status = EstateStakeholderDto.enmStatus.pending.ToString()
                        };
                        await _stakeholderRep.InsertAsync(stakeHolder);
                    }
                    else
                    {
                        // Si existe lo actualiza

                        existSkateHolder.updateDate = DateTime.Now;
                        existSkateHolder.status = EstateStakeholderDto.enmStatus.pending.ToString();
                        existSkateHolder.canceledDate = null;
                        existSkateHolder.acceptedDate = null;
                        await _stakeholderRep.UpdateAsync(existSkateHolder);
                        stakeHolder = existSkateHolder;
                    }

                    var mail = new Mail.SenderMail(_config);
                    var agent = _agentRep.GetById(agentId);
                    
                    var urlEstate = _config.GetValue<string>("UrlServices:EstateService");
                    var body = $@"Hola, {agent.name}.</br>
Te están compartiendo un inmueble.</br>
Pincha en <a href={urlEstate}{stakeHolder.estateId}/shareto/confirm/{token}>este enlace </a> para aceptarlo.";
                    //a href=https://devapi.waytocol.com/state/-987/shareto/confirm/29dce7b3-51fe-4a86-b166-c37157ddd71a

                    await mail.SendMailToConfirm(agent.email, "[Waytocol] Nuevo inmueble compartido", body, "");
                    Serilog.Log.Information(body);

                    resp.AddResponse(respEachAgent);
                }
            }
            return resp;
        }





    }
}
