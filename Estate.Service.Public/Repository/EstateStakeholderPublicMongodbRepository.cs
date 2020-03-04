using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Repository;


namespace WayToCol.EstatStakeholder.Service.Public.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class EstateStakeholderPublicMongoDbRepository : MongoDbPublicRepository<EstateStakeholderDto, string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public EstateStakeholderPublicMongoDbRepository(IConfiguration config) : base(config, "EstateStakeholder")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idEstate"></param>
        /// <returns></returns>
        public IEnumerable<String> GetByIdEstate(string idEstate)
        {
            return this.All().Where(x => x.estateId == idEstate).Select(x => x.id).ToList();
        }
    }
}
