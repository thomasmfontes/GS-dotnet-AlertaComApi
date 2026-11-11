using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AlertaComApi.Domain.Entities
{
    public class OcorrenciaNoSql
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ocorrenciaIdRelacional")]
        public int OcorrenciaIdRelacional { get; set; }

        [BsonElement("protocolo")]
        public string Protocolo { get; set; } = string.Empty;

        [BsonElement("tipoRisco")]
        public string TipoRisco { get; set; } = string.Empty;

        [BsonElement("severidade")]
        public string Severidade { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;

        [BsonElement("endereco")]
        public string Endereco { get; set; } = string.Empty;

        [BsonElement("bairro")]
        public string Bairro { get; set; } = string.Empty;

        [BsonElement("localizacao")]
        public GeoJsonPointLocalizacao Localizacao { get; set; } = new();

        [BsonElement("tagsIA")]
        public List<string> TagsIA { get; set; } = new();

        [BsonElement("recomendacaoSegurancaIA")]
        public string? RecomendacaoSegurancaIA { get; set; }

        [BsonElement("dataCriacao")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }

    public class GeoJsonPointLocalizacao
    {
        [BsonElement("type")]
        public string Type { get; set; } = "Point";

        [BsonElement("coordinates")]
        public double[] Coordinates { get; set; } = new double[2]; // [longitude, latitude]
    }
}
