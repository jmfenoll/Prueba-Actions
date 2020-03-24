using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
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
    public class EstateStakeholderMongoDbRepository : MongoDbRepository<EstateStakeholderDto>, IEstateStakeholderRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public EstateStakeholderMongoDbRepository(IConfiguration config) : base(config, "EstateStakeholder")
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

        public void DeleteAgent(string stakeholderId)
        {
            var builder = Builders<EstateStakeholderDto>.Filter;
            var filter = builder.Eq("agentId", stakeholderId);
            var result = _collection.DeleteOne(filter);
        }
    }
}
