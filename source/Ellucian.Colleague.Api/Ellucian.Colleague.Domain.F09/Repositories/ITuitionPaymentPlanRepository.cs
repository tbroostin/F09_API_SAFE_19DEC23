using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    public interface ITuitionPaymentPlanRepository
    {
        Task<F09PaymentForm> GetTuitionFormAsync(string studentId);
        Task<F09PaymentInvoice> SubmitTuitionFormAsync(F09TuitionPaymentPlan paymentPlan);
        Task<F09PaymentForm> GetChangeTuitionFormAsync(string studentId);
        Task<string> SubmitChangeTuitionFormAsync(F09TuitionPaymentPlan paymentPlan);
    }
}