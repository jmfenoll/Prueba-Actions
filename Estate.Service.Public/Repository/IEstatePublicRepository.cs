
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Repository;

namespace WayToCol.Estate.Service.Public.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEstatePublicRepository : IPublicRepository<propiedadesPropiedad>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        Common.Contracts.PaginationModel<propiedadesPropiedad> GetPaginated(int page, int pagesize);
    }


}