using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using WayToCol.Common.Contracts;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Estate.Service.Public.Repository;
using WayToCol.EstateFile.Service.Public.Repository;

namespace WayToCol.Estate.Service.Public.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class EstateController : ControllerBase
    {
        private readonly ILogger<EstateController> _logger;
        private readonly IEstatePublicRepository _rep;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="rep"></param>
        public EstateController(ILogger<EstateController> logger, IEstatePublicRepository rep)
        {
            _logger = logger;
            _rep = rep;
        }

        /// <summary>
        /// Get list of estates paginated
        /// </summary>
        /// <param name="page">Number of page</param>
        /// <param name="pagesize">Items by page</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{page:int}/{pagesize:int}")]
        public IActionResult Get(/*[fromquery]*/int page, int pagesize)
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
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

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
        public IActionResult GetEstate(string idestate)
        {
            try
            {
                var estate = _rep.Single(x => idestate == x.idMD5);
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

    }
}
