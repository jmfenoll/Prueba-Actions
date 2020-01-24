using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using WayToCol.Common.Api.Extensions;
using WayToCol.Common.Api.Helpers;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Estate.Service.Public.Domain;
using WayToCol.Estate.Service.Public.Extensions;
using WayToCol.Estate.Service.Public.Repository;
using WayToCol.EstateFile.Service.Public.Repository;

namespace WayToCol.Estate.Service.Public.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Authorize]
    public class EstateController : ControllerBase
    {
        private readonly ILogger<EstateController> _logger;
        private readonly IEstatePublicRepository _rep;
        private readonly IConfiguration _config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="rep"></param>
        public EstateController(ILogger<EstateController> logger, IEstatePublicRepository rep, IConfiguration config)
        {
            _logger = logger;
            _rep = rep;
            _config = config;
        }

        /// <summary>
        /// Get list of estates paginated
        /// </summary>
        /// <param name="page">Number of page</param>
        /// <param name="pagesize">Items by page</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/")]
        public IActionResult Get([FromQuery]int page, [FromQuery]int pagesize)
        {
            try

            {

                // TODO: Que no devuelvan los nulos
                // TODO: Los registros por página dónde se parametrizan?
                // TODO: Habrá que paginar teniendo en cuenta el order de las columnas
                //throw NotImplementedException();
                var response = _rep.GetPaginated(page, pagesize);
                response.Page = page;
                response.Count = pagesize;
                if (response.Data == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

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
                var _repFile = (IEstateFilePublicRepository) HttpContext.RequestServices.GetService(typeof(IEstateFilePublicRepository));

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
                var estate = _rep.Single(x => idestate == x.id);
                if (estate == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status200OK, estate);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al hacer Get FileEstate");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }


        /// <summary>
        /// Used for import an Estate by Xml
        /// </summary>
        /// <param name="estateImport"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SaveImportedEstate(propiedadesPropiedadXml estateImport)
        {
            // Autenticación  (Da problemas)
            var dom = new EstateDomain();

            // Mapeamos a Estate y a estateFiles
            var estateDto = dom.Map(estateImport);
            var estateFilesDto = dom.MapFiles(estateImport);

            var response = UpsertEstateFileAsync(estateFilesDto);
            var numPhotos = response.Count(x => x.Result.StatusCode==HttpStatusCode.OK);
            estateDto.numfotos = numPhotos.ToString();
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
            return StatusCode(StatusCodes.Status200OK);
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
            var client = new HttpProxy2Api();
            var url = new Uri(_config["privateServices:estate"]);
            var response = await client.PutAsync(url.AbsoluteUri, estate);
            return response;
        }
        private async Task<HttpResponseMessage> UpsertEstateFileAsync(EstateFileDto estateFile)
        {
            var client = new HttpProxy2Api();
            var url = new Uri(_config["privateServices:estateFile"]);
            var response = await client.PutAsync(url.AbsoluteUri, estateFile);
            return response;
        }


    }
}
