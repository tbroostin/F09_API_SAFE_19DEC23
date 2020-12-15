// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class InstantEnrollmentCashReceiptAcknowledgementRequestDtoToEntityAdapter:
        BaseAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgementRequest,
            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgementRequest>
    {
        public InstantEnrollmentCashReceiptAcknowledgementRequestDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Converts an InstantEnrollmentCashReceiptAcknowledgementRequest DTO to an InstantEnrollmentCashReceiptAcknowledgementRequest entity
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Student.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgementRequest"/> to convert</param>
        /// <returns>The corrresponding <see cref="Domain.Student.Entities.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgementRequest "/></returns>
        public override Domain.Student.Entities.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgementRequest MapToType(
            Dtos.Student.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgementRequest source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgementRequest(
                transactionId: source.TransactionId,
                cashReceiptId: source.CashReceiptId,
                personId: source.PersonId
                );
        }
    }
}
