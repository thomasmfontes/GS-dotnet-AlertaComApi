using System.Diagnostics.Metrics;

namespace AlertaComApi.Diagnostics
{
    public static class AlertaComMetrics
    {
        public const string MeterName = "AlertaComApi";
        private static readonly Meter Meter = new(MeterName, "1.0.0");

        public static readonly Counter<long> OcorrenciasCriadasCounter =
            Meter.CreateCounter<long>("alertacom.ocorrencias.criadas.total", "unidades", "Total de ocorrências urbanas registradas");

        public static readonly Counter<long> AlertasEmitidosCounter =
            Meter.CreateCounter<long>("alertacom.alertas.emitidos.total", "unidades", "Total de alertas de risco climático emitidos");

        public static readonly Counter<long> LoginSucessoCounter =
            Meter.CreateCounter<long>("alertacom.auth.login.sucesso.total", "unidades", "Total de logins autenticados");
    }
}
