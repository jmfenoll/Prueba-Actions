using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using WayToCol.Common.Contracts;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Repository;

namespace WayToCol.Estate.Service.Public.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class EstatePublicMongoDbRepository : MongoDbPublicRepository<EstateDto>, IEstatePublicRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public EstatePublicMongoDbRepository(IConfiguration config) : base(config, "Estates", "idMD5")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public PaginationModel<EstateDto> GetPaginated(int page, int pagesize) {
                var pag = new PaginationModel<EstateDto>();
                var resp = this.All().Skip((page - 1) * pagesize).Take(pagesize).ToList();

                if (resp == null)
                    pag.TotalPages = 0;
                else
                    pag.TotalPages = (int)Math.Ceiling(Convert.ToDecimal(this.All().Count()) / pagesize);
                pag.Data = resp;
                return pag;


        }

    }
}
