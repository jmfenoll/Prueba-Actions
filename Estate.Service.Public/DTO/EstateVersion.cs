using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WayToCol.Common.Contracts.Estates;

namespace WayToCol.EstateFile.Service.Public.DTO
{
    /// <summary>
    /// 
    /// </summary>
    public class EstateVersion
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ShowAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public EstateDto Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? UpdateDate { get; set; }

    }
}
