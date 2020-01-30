
using System;
using System.Collections.Generic;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Repository;

namespace WayToCol.Estate.Service.Public.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEstateFilePublicRepository : IPublicRepository<EstateFileDto>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idEstate"></param>
        /// <returns></returns>
        IEnumerable<String> GetByIdEstate(string idEstate);
    }


}