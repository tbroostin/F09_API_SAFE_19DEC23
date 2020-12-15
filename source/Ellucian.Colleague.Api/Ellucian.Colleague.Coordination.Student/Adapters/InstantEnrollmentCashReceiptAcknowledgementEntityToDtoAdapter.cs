// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class InstantEnrollmentCashReceiptAcknowledgementEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgement, Dtos.Student.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgement>
    {
        public InstantEnrollmentCashReceiptAcknowledgementEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.EcommerceProcessStatus, Dtos.Student.InstantEnrollment.EcommerceProcessStatus>();
            AddMappingDependency< Domain.Student.Entities.InstantEnrollment.ConvenienceFee, Dtos.Student.InstantEnrollment.ConvenienceFee>();
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.PaymentMethod, Dtos.Student.InstantEnrollment.PaymentMethod>();
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationPaymentGatewayRegisteredSection, Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationPaymentGatewayRegisteredSection>();
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationPaymentGatewayFailedSection, Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationPaymentGatewayFailedSection>();
        }
    }
}
