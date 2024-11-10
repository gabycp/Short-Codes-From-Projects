namespace EaglePagoLinea.Models
{
    public class TransaccionPagoPP
    {
        public int IdPagoTemporalPP { get; set; }
        public string PaymentUuid { get; set; }
        public string NombreEstudiante { get; set; }
        public DateTime? FechaPago { get; set; }
        public double? Monto { get; set; }
        public string PagadoPor { get; set; }
        public string NumeroDocumento { get; set; }
        public string TransactionId { get; set; }
    }
}
