using Lucene.Net.Documents;
using Lucene.Net.Index;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WayToCol.Common.Contracts;
using WayToCol.Common.Contracts.Estates;
using WayToCol.Common.Contracts.Responses;
using WayToCol.Common.Repository;
using WayToCol.Estate.Service.Public.DTO;
using WayToCol.Estate.Service.Public.Extensions;
using WayToCol.EstateFile.Service.Public.DTO;
using WayToCol.Utils.Lucene;

namespace WayToCol.Estate.Service.Public.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class EstateMongoDbRepository : MongoDbRepository<EstateDto>, IEstateRepository
    {
        private IConfiguration _config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public EstateMongoDbRepository(IConfiguration config) : base(config, "Estates", "id")
        {
            _config = config;
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

        public override ReplaceOneResult Upsert(EstateDto item)
        {
            var resp = base.Upsert(item);

            UpdateLucene(item);
            return resp;
        }

        public override async Task<ReplaceOneResult> UpsertAsync(EstateDto item)
        {
            var resp = await base.UpsertAsync(item);
            UpdateLucene(item);
            return resp;
        }

        public override void Insert(EstateDto item)
        {
            base.Insert(item);
            UpdateLucene(item);
        }

        public override Task InsertAsync(EstateDto item)
        {
            var resp = base.InsertAsync(item);
            UpdateLucene(item);
            return resp;
        }

        public override ReplaceOneResult Update(EstateDto item)
        {
            var resp = base.Update(item);
            UpdateLucene(item);
            return resp;
        }

        public override Task<ReplaceOneResult> UpdateAsync(EstateDto item)
        {
            var resp = base.UpdateAsync(item);

            UpdateLucene(item);
            return resp;
        }

        [Obsolete("NO USAR porque no borraría en Lucene", true)]
        public override DeleteResult Delete(Expression<Func<EstateDto, bool>> predicate)
        {

            var resp = base.Delete(predicate);
            //DeleteLucene(predicate);
            return resp;
        }

        public override Task<DeleteResult> DeleteAsync(string id)
        {
            var resp = base.DeleteAsync(id);
            DeleteLucene(id);
            return resp;
        }


        private Document GetDocument(EstateDto item)
        {

            string text = "";
            text = text.addTxt(item?.descrip1);
            text = text.addTxt(item?.fichaId.ToString());
            text = text.addTxt(item?.tipo_ofer);
            text = text.addTxt(item?.ciudad);
            text = text.addTxt(item?.zona);
            text = text.addTxt(item?.calle);

            text = text.addBol(item);


            var doc = new Document() {
                new StringField("id", item.id, Field.Store.YES),
                new TextField("searchField", text, Field.Store.NO)
            };
            return doc;
        }

        private void UpdateLucene(EstateDto item)
        {
            var lucene = LuceneFactory.GetInstance(_config);
            var doc = GetDocument(item);
            //lucene.DeleteDocument(new Term("id", item.id));
            lucene.UpdateDocument(new Term("id", item.id), doc);
        }

        private void DeleteLucene(EstateDto item)
        {
            var lucene = LuceneFactory.GetInstance(_config);
            var doc = GetDocument(item);
            lucene.DeleteDocument(new Term("id", item.id));
        }

        private void DeleteLucene(string id)
        {
            var lucene = LuceneFactory.GetInstance(_config);
            lucene.DeleteDocument(new Term("id", id));
        }


    }
}
