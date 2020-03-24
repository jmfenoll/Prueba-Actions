using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Repository;

namespace WayToCol.EstatStakeholder.Service.Public.Repository
{
    public interface IEstateStakeholderRepository: IRepository<EstateStakeholderDto>
    {
        void DeleteAgent(string stakeholderId);
    }
}