
using System.Threading.Tasks;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Repository;
using WayToCol.Estate.Service.Public.DTO;
using WayToCol.EstateFile.Service.Public.DTO;

namespace WayToCol.Estate.Service.Public.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEstateRepository : IRepository<EstateDto>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        Common.Contracts.Responses.PaginationModel<EstateDto> GetPaginated(int page, int pagesize);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        Task<EstateVersion> Publish(string id, EstatePublishCreateModel model);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<EstateVersion> GetPublish(string id);
    }


}