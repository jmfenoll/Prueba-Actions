using System.Collections.Generic;
using WayToCol.Common.Contracts.Agencies;
using WayToCol.Common.Contracts.Agents;
using WayToCol.Common.Repository;

namespace WayToCol.Agent.Service.Repository
{
    public interface IAgentRepository : IRepository<AgentDto>
    {
        string GetAgentByUserId(string currentUserId);
        List<string> GetIdByEstate(string agencyId);
    }
}
