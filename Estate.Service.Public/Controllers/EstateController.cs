using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WayToCol.Common.Contracts;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Estate.Service.Public.Repository;


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

    }
}
