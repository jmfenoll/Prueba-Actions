//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using WayToCol.Common.Contracts;
//using WayToCol.Common.Contracts.Estates;
//using WayToCol.Estate.Service.Public.Controllers;
//using WayToCol.Estate.Service.Public.Repository;

using Mapster;
using System;
using System.Collections.Generic;
using WayToCol.Common.Contracts.Estates;
using WayToCol.EstateFile.Service.Public.Helpers;

namespace WayToCol.Estate.Service.Public.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public class EstateDomain
    {
        internal EstateDto Map(propiedadesPropiedadXml estateImport)
        {
            var estate = estateImport.Adapt<EstateDto>();
            return estate;


        }

        internal EstateFileDto[] MapFiles(propiedadesPropiedadXml estateImport)
        {
            var files = GetPhotos(estateImport);
            return files.ToArray();
        }

        private List<EstateFileDto> GetPhotos(propiedadesPropiedadXml estate)
        {
            var photoProperties = GetPhotoProperties(estate);

            var listPhotos = new List<EstateFileDto>();
            foreach (var photo in photoProperties)
            {
                var file = new EstateFileDto
                {
                    url = photo.url,
                    id = ImportHelper.GetMD5(estate.id + photo.prop),
                    idEstate = ImportHelper.GetMD5(estate.id),
                    name = ImportHelper.GetPhotoName(photo.url),
                    mimeType = ImportHelper.GetMimeType(photo.url)
                };
                listPhotos.Add(file);
            }
            return listPhotos;
        }

        private List<(string prop, string url)> GetPhotoProperties(propiedadesPropiedadXml estate)
        {
            var resp = new List<(string prop, string url)>();

            var props = estate.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name.StartsWith("foto"))
                {
                    var value = ((string)prop.GetValue(estate));
                    if (value != null)
                        resp.Add((prop.Name, value));
                }
            }
            return resp;
        }
    }
}
