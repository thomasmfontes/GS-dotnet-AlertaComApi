using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Interfaces;
using MongoDB.Driver;

namespace AlertaComApi.Infrastructure.NoSql
{
    public class OcorrenciaNoSqlRepository : IOcorrenciaNoSqlRepository
    {
        private readonly IMongoCollection<OcorrenciaNoSql>? _collection;

        public OcorrenciaNoSqlRepository(MongoDbContext context)
        {
            _collection = context.Ocorrencias;
        }

        public async Task<IEnumerable<OcorrenciaNoSql>> GetAllAsync()
        {
            if (_collection == null) return Enumerable.Empty<OcorrenciaNoSql>();
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<OcorrenciaNoSql?> GetByIdAsync(string id)
        {
            if (_collection == null) return null;
            return await _collection.Find(o => o.Id == id).FirstOrDefaultAsync();
        }

        public async Task<OcorrenciaNoSql?> GetByRelacionalIdAsync(int relacionalId)
        {
            if (_collection == null) return null;
            return await _collection.Find(o => o.OcorrenciaIdRelacional == relacionalId).FirstOrDefaultAsync();
        }

        public async Task<OcorrenciaNoSql> AddAsync(OcorrenciaNoSql ocorrenciaNoSql)
        {
            if (_collection != null)
            {
                await _collection.InsertOneAsync(ocorrenciaNoSql);
            }
            return ocorrenciaNoSql;
        }

        public async Task<IEnumerable<OcorrenciaNoSql>> GetNearAsync(double longitude, double latitude, double maxDistanceMeters)
        {
            if (_collection == null) return Enumerable.Empty<OcorrenciaNoSql>();
            // Exemplo de busca geoespacial
            return await _collection.Find(_ => true).Limit(20).ToListAsync();
        }
    }
}
