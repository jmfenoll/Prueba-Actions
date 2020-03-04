

using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using WayToCol.Common.Contracts;
using WayToCol.Common.Contracts.Agents;
using WayToCol.Common.Repository;

namespace WayToCol.Agent.Service.Public.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class AgentPublicMongoDbRepository : MongoDbPublicRepository<AgentDto, ObjectId>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public AgentPublicMongoDbRepository(IConfiguration config) : base(config, "Agents")
        {
        }

        internal List<string> GetIdByEstate(string agencyId)
        {
            return this.All().Where(x => x.id_agency == agencyId).Select(x => x.id.ToString()).ToList();
        }
    }
}
