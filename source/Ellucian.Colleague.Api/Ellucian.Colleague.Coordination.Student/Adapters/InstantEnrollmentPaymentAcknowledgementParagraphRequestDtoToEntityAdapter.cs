// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment payment acknowledgement paragraph request DTO to the corresponding entity
    /// </summary>
    public class InstantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapter :
        BaseAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest,
            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest>
    {
        public InstantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Converts an InstantEnrollmentPaymentAcknowledgementParagraphRequest DTO to an InstantEnrollmentPaymentAcknowledgementParagraphRequest entity
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest"/> to convert</param>
        /// <returns>The corrresponding <see cref="Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest "/></returns>
        public override Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest MapToType(Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest(source.PersonId,
                source.CashReceiptId
                );            
        }
    }
}
