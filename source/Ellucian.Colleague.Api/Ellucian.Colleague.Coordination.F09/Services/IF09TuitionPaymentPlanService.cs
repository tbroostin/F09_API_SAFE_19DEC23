using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IF09TuitionPaymentPlanService
    {
        Task<F09PaymentFormDto> GetTuitionPaymentFormAsync(string studentId);
        Task<Dtos.F09.F09PaymentInvoiceDto> SubmitTuitionPaymentFormAsync(Dtos.F09.F09TuitionPaymentPlanDto dto);
    }
}