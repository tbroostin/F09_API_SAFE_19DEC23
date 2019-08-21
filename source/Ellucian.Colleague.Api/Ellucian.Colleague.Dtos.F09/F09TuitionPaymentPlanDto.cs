namespace Ellucian.Colleague.Dtos.F09
{
    public enum F09TuitionPaymentPlanType {
        Enroll = 0,
        Change = 1,
    }
    public class F09TuitionPaymentPlanDto
    {
        public string PaymentOption { get; set; }
        public string StudentId { get; set; }
        public string PaymentMethod { get; set; }
    }
}
