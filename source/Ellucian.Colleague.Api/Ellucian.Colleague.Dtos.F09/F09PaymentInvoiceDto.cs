namespace Ellucian.Colleague.Dtos.F09
{
    public class F09PaymentInvoiceDto
    {
        public string Term { get; set; }
        public string StudentId { get; set; }
        public string ArCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Distribution { get; set; }
        public string ArType { get; set; }
        public string Mnemonic { get; set; }

        public string PaymentMethod { get; set; }
    }
}
