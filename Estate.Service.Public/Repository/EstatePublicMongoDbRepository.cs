using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using WayToCol.Common.Contracts;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Contracts.Responses;
using WayToCol.Common.Repository;
using WayToCol.Estate.Service.Public.DTO;
using WayToCol.EstateFile.Service.Public.DTO;

namespace WayToCol.Estate.Service.Public.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class EstatePublicMongoDbRepository : MongoDbPublicRepository<EstateDto>, IEstatePublicRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public EstatePublicMongoDbRepository(IConfiguration config) : base(config, "Estates", "id")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        public PaginationModel<EstateDto> GetPaginated(int page, int itemsPerPage) {
                var pag = new PaginationModel<EstateDto>();
                var resp = this.All().Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                pag.data = resp;
                return pag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<EstateVersion> Publish(string id, EstatePublishCreateModel model)
        {
            var builder = Builders<EstateVersion>.Filter;
            var filter = builder.Eq("_id", id);
            var collection = _database.GetCollection<EstateVersion>(nameof(EstateVersion));

            var item = await GetEstateVersion(id, model);
            item.UpdateDate = DateTime.Now;
            item.ShowAddress = model.ShowAddress ?? false;
            if (!string.IsNullOrWhiteSpace(model.Description)) 
                item.Data.descrip1 = model.Description;

            await collection.ReplaceOneAsync(filter, item, new UpdateOptions { IsUpsert = true });

            var result = await (await collection.FindAsync(filter)).FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<EstateVersion> GetPublish(string id)
        {
            var filter = Builders<EstateVersion>.Filter.Eq("_id", id);
            var collection = _database.GetCollection<EstateVersion>(nameof(EstateVersion));
            var result = await (await collection.FindAsync(filter)).FirstOrDefaultAsync();
            return result;
        }

        private static async Task<EstateVersion> GetEstateVersion(string id, EstatePublishCreateModel model)
        {
            var filter = Builders<EstateVersion>.Filter.Eq("_id", id);
            var EstateVersionCollection = _database.GetCollection<EstateVersion>(nameof(EstateVersion));
            var item = await (await EstateVersionCollection.FindAsync(filter)).FirstOrDefaultAsync();
            

            if (item == null)
            {               
                item = new EstateVersion { Id = id, CreateDate = DateTime.Now };
            }

            var estate = await (await _database.GetCollection<EstateDto>("Estates").FindAsync(Builders<EstateDto>.Filter.Eq("id", id))).FirstOrDefaultAsync();
            item.Data = estate;

            return item;
        }


    }
}
