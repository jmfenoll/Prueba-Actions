using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using WayToCol.Common.Contracts;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Contracts.Responses;
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
        public EstatePublicMongoDbRepository(IConfiguration config) : base(config, "Estates", "id")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public PaginationModel<EstateDto> GetPaginated(int page, int itemsPerPage) {
                var pag = new PaginationModel<EstateDto>();
                var resp = this.All().Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                pag.data = resp;
                return pag;
        }



    }
}
