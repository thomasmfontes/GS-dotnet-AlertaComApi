using AlertaComApi.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AlertaComApi.Infrastructure.NoSql
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "alertacom_db";
        public string OcorrenciasCollectionName { get; set; } = "ocorrencias_geoloc";
    }

    public class MongoDbContext
    {
        private readonly IMongoDatabase? _database;
        private readonly MongoDbSettings _settings;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            _settings = settings.Value;
            try
            {
                var clientSettings = MongoClientSettings.FromConnectionString(_settings.ConnectionString);
                clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(1);
                clientSettings.ConnectTimeout = TimeSpan.FromSeconds(1);
                var client = new MongoClient(clientSettings);
                _database = client.GetDatabase(_settings.DatabaseName);
            }
            catch
            {
                _database = null;
            }
        }

        public IMongoCollection<OcorrenciaNoSql>? Ocorrencias =>
            _database?.GetCollection<OcorrenciaNoSql>(_settings.OcorrenciasCollectionName);
    }
}
