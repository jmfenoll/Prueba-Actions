﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using WayToCol.Agent.Service.Repository;
using WayToCol.Common.Api.Extensions;
using WayToCol.Common.Api.Helpers;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Contracts.Estates.Import;
using WayToCol.Common.Contracts.Responses;
using WayToCol.Estate.Service.Public.Domain;
using WayToCol.Estate.Service.Public.DTO;
using WayToCol.Estate.Service.Public.Extensions;
using WayToCol.Estate.Service.Public.Repository;
using WayToCol.EstateFile.Service.Public.Controllers;
using WayToCol.EstatStakeholder.Service.Public.Repository;

namespace WayToCol.Estate.Service.Public.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Authorize]
    public class EstateController: WayToColControllerBase
    {
        private readonly ILogger<EstateController> _logger;
        private readonly IEstateRepository _repEstate;
        private readonly IEstateFileRepository _repEstateFile;
        private readonly IConfiguration _config;
        private readonly EstateDomain _estateDomain;
        private readonly IEstateStakeholderRepository _repStakeholder;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="rep"></param>
        public EstateController(ILogger<EstateController> logger, IEstateRepository rep, IConfiguration config, EstateDomain estateDomain, IEstateStakeholderRepository stakeholderRep, IEstateFileRepository estateFileRep, IAgentRepository agentRep) 
            :base(config, agentRep)
        {
            _logger = logger;
            _repEstate = rep;
            _config = config;
            _estateDomain = estateDomain;
            _repStakeholder = stakeholderRep;
            _repEstateFile = estateFileRep;
        }

        /// <summary>
        /// Get list of estates paginated
        /// </summary>
        /// <param name="page">Number of page</param>
        /// <param name="pagesize">Items by page</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IActionResult Get([FromQuery]int? page, [FromQuery]int? pagesize)
        {
            try
            {
                // TODO: Habrá que paginar teniendo en cuenta el order de las columnas
                //throw NotImplementedException();
                var response = _repEstate.GetPaginated(ref page, ref pagesize);
                if (response.data == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        /// <summary>
        /// Get list of estates paginated
        /// </summary>
        /// <param name="page">Number of page</param>
        /// <param name="pagesize">Items by page</param>
        /// <returns></returns>
        [HttpGet("/search/{term}")]
        public IActionResult Search(string term, int? page, int? pagesize)
        {
            PaginationModel<EstateDto> resp = _estateDomain.Search(term, page, pagesize);

            return new JsonResult(resp);
            //try
            //{
                
            //    //var response = _rep.GetPaginated(page, pagesize);
            //    //response.Page = page;
            //    //response.Count = pagesize;
            //    //if (response.Data == null)
            //    //    return StatusCode(StatusCodes.Status500InternalServerError);
            //    return StatusCode(StatusCodes.Status200OK, response);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Get");
            //    return StatusCode(StatusCodes.Status500InternalServerError);
            //}

        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="user"></param>
        ///// <param name="pass"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("auth")]
        //public IActionResult Authenticate([FromQuery] string user, [FromQuery] string pass)
        //{
        //    try
        //    {
        //        if (user == "123" && pass == "123")
        //        {
        //            dynamic resp = new ExpandoObject();
        //            resp.token = Guid.NewGuid();
        //            resp.valid = DateTime.Now.AddDays(1);
        //            var strJson = JsonConvert.SerializeObject(resp);
        //            return StatusCode(StatusCodes.Status200OK, strJson);
        //        }
        //        else
        //            return StatusCode(StatusCodes.Status401Unauthorized);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Authenticate");
        //        return StatusCode(StatusCodes.Status500InternalServerError);
        //    }

        //}

        /// <summary>
        /// Get id files from estate
        /// </summary>
        /// <returns></returns>
        /// <example>0c269a6461d9de85b0510966eae9837c</example>
        // https://docs.microsoft.com/es-es/aspnet/core/fundamentals/routing?view=aspnetcore-2.1#route-template-reference
        [HttpGet]
        [Route("{idestate}/files")]
        public IActionResult GetFilesByIdEstate(string idestate)
        {
            try
            {
                var _repFile = (IEstateFileRepository) HttpContext.RequestServices.GetService(typeof(IEstateFileRepository));

                var listFiles= _repFile.GetByIdEstate(idestate);
                if (listFiles == null || listFiles.Count()==0)
                    return StatusCode(StatusCodes.Status204NoContent);
                return StatusCode(StatusCodes.Status200OK, listFiles);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al hacer Get FileEstate");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }


        /// <summary>
        /// Get estate details
        /// </summary>
        /// <param name="idestate">
        /// Id of an estate, for example, fdd0804601659077ea8ab4f8970a4c97
        /// </param>
        /// <returns></returns>
        // https://docs.microsoft.com/es-es/aspnet/core/fundamentals/routing?view=aspnetcore-2.1#route-template-reference
        [HttpGet]
        [Route("{idestate}")]
        public IActionResult GetEstate([FromRoute]string idestate)
        {
            try
            {

                var estate = _repEstate.Single(x => idestate == x.id);
                if (estate == null)
                    return BadRequest();
                return StatusCode(StatusCodes.Status200OK, estate);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al hacer Get FileEstate");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }


        /// <summary>
        /// Used for import an Estate by Chrome Extensioon
        /// </summary>
        /// <param name="estateImport"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SaveImportedEstate(propiedadesPropiedadXml estateImport)
        {
            // Autenticación  (Da problemas)

            // Mapeamos a Estate y a estateFiles
            var estateDto = _estateDomain.Map(estateImport);
            var estateFilesDto = _estateDomain.MapFiles(estateImport);

            var response = UpsertEstateFileAsync(estateFilesDto);
            var numPhotos = response.Count(x => x.Result.StatusCode==HttpStatusCode.OK);
            estateDto.numfotos = numPhotos;
            await UpsertEstateAsync(estateDto);


            //    try
            //    {
            //        // Esto va fuera, La extensión llamará al Public, el Public mapeará y lo pasará al Private ya mapeado.

            //        EstateDto estateDto = Map(estateImport);
            //        var resp = await _rep.UpsertAsync(estateDto);
            //        if (!resp.IsAcknowledged)
            //            throw new Exception("Error al hacer DeleteById" + estateDto.id);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Error(ex, "Error al hacer Upsert de Estado");
            //        return StatusCode(StatusCodes.Status500InternalServerError);
            //    }
            //    return StatusCode(StatusCodes.Status200OK);



            //var client = new HttpProxy2Api();
            //var url = new Uri(_urlSvcFileEstate).Append("");
            //var response = await client.PutAsync(url.AbsoluteUri, file);
            //return response;

            /*try
            {
                var estate = _rep.Single(x => idestate == x.id);
                if (estate == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status200OK, estate);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al hacer Get FileEstate");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }*/
            return Ok();
        }

        private List<Task<HttpResponseMessage>> UpsertEstateFileAsync(EstateFileDto[] estateFilesDto)
        {
            int maxConcurrentTasks = Int32.Parse(_config["privateServices:max_concurrent_tasks"]);

            maxConcurrentTasks = 1;
            var queue=  new List<Task<HttpResponseMessage>>();
            foreach (var file in estateFilesDto)
            {
                queue.Add(UpsertEstateFileAsync(file));
                while (queue.Count(x => x.Status == TaskStatus.Running || x.Status == TaskStatus.WaitingForActivation) >= maxConcurrentTasks) { }
            }
            Task.WaitAll(queue.ToArray());
            return queue;
        }


        private async Task<HttpResponseMessage> UpsertEstateAsync(EstateDto estate)
        {
            
            var url = new Uri(_config["privateServices:estate"]);
            var client = new HttpProxy2Api<EstateDto>(url.ToString());
            var response = await client.PutAsync(estate);
            return response;
        }
        private async Task<HttpResponseMessage> UpsertEstateFileAsync(EstateFileDto estateFile)
        {
            var url = new Uri(_config["privateServices:estateFile"]);
            var client = new HttpProxy2Api<EstateFileDto>(url.ToString());
            var response = await client.PutAsync(estateFile);
            return response;
        }


        /// <summary>
        /// Get list of agents that are subscripted to an estate (paginated)
        /// </summary>
        /// <remarks>
        ///     POST /state/1a924a77b6a0d28fa24666eb0e1adcb5/stakeholder
        ///     {
        ///        "page": 1,
        ///        "pagesize": 1,
        ///     }
        /// </remarks>
        /// <param name="page">Number of page</param>
        /// <param name="pagesize">Items by page</param>
        /// <param name="estateId">Id of Estate</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/estate/{estateId}/stakeholder")]
        public IActionResult GetStakeHolder([FromQuery]int? page, [FromQuery]int? pagesize, string estateId)
        {
            try
            {
                var connectedAgentId = GetCurrentUserIdFromContext();
                // TODO: Habrá que paginar teniendo en cuenta el order de las columnas
                var resp = _estateDomain.GetStakeholders(page, pagesize, estateId, connectedAgentId);
                if (resp.Ok)
                    return Ok(resp.Content);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("publish/{id}")]
        public async Task<IActionResult> Publish([FromRoute] string id, [FromBody] EstatePublishCreateModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest("id parameter is empty");

                var result = await _repEstate.Publish(id, model);
                if (result == null)
                    return BadRequest("public version don't created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unexpected error on publish estate");
                return BadRequest();
            }

            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("publish/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublish([FromRoute] string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest("id parameter is empty");

                var result = await _repEstate.GetPublish(id);
                if (result == null)
                    return NotFound();

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexcepted error on get publish version");
                return BadRequest("unexcepted error");
            }
        }

        /// <summary>
        /// Share an estate to an agent
        /// </summary>
        /// <param name="estate"></param>
        /// <returns></returns>
        [HttpPost("/estate/{estateId}/shareto")]

        public async Task<IActionResult> ShareTo(string estateId, [FromBody] string[] agents)
        {
            try
            {
                var currentAgent = GetCurrentAgentIdFromContext();
                var resp= await _estateDomain.ShareTo(estateId, agents, currentAgent);
                if (resp.Ok)
                    return Ok();
                else
                    return BadRequest(resp.ToString());

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al compartir Estate");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return StatusCode(StatusCodes.Status200OK);
        }



        /// <summary>
        /// Accept an estate shared 
        /// </summary>
        /// <param name="estateId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("{estateId}/shareto/confirm/{token}")]

        public async Task<IActionResult> Confirm(string estateId, string token)
        {
            try
            {
                var resp = await _estateDomain.ConfirmAsync(estateId, token, _repStakeholder);
                if (resp.Ok)
                {
                    return Redirect(_config.GetValue<string>("LandingPageShared"));
                }
                else
                    return StatusCode(resp.GetErrorCodes[0].code, resp.GetErrorCodes[0].description);

            }
            catch (ApplicationException ex)
            {
                Log.Error(ex, "Error al confirmar Estate compartido");
                return StatusCode(StatusCodes.Status400BadRequest);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al confirmar Estate compartido");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpDelete("estate/{estateId}/stakeholder/{stakeholderId}")]
        public async Task<IActionResult> DeleteStakeholder(string estateId, string stakeholderId)
        {
            try
            {
                var currentAgentId = GetCurrentUserIdFromContext();
                var resp = _estateDomain.DeleteStakeholder(HttpContext, estateId, stakeholderId, currentAgentId);
                if (resp.Ok)
                    return StatusCode(StatusCodes.Status200OK);
                else
                    return StatusCode(resp.GetErrorCodes[0].code);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al borrar un colaborador de Inmueble");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        ///// <summary>
        ///// Create a new Estate
        ///// </summary>
        ///// <param name="estate"></param>
        ///// <returns></returns>
        //[HttpPost("")]
        //public async Task<IActionResult> Insert(EstateDto estate)
        //{
        //    try
        //    {
        //        await _repEstate.InsertAsync(estate);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Error al hacer Insert de Estate");
        //        return StatusCode(StatusCodes.Status500InternalServerError);
        //    }
        //    return StatusCode(StatusCodes.Status200OK);
        //}

        ///// <summary>
        ///// Update an estate
        ///// </summary>
        ///// <param name="estate"></param>
        ///// <returns></returns>
        //[HttpPut("")]
        //public async Task<IActionResult> Update(EstateDto estate)
        //{
        //    try
        //    {
        //        var resp = await _repEstate.UpdateAsync(estate);
        //        if (!resp.IsAcknowledged)
        //            throw new Exception("Error al hacer Update");
        //        if (resp.MatchedCount == 0)
        //            return StatusCode(StatusCodes.Status204NoContent);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Error al hacer Upsert de estate");
        //        return StatusCode(StatusCodes.Status500InternalServerError);
        //    }
        //    return StatusCode(StatusCodes.Status200OK);
        //}




    }




}
