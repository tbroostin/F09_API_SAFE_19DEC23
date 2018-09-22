// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A receivable type and flag indicating whether or not the type is payable
    /// </summary>
    [Serializable]
    public class PayableReceivableType
    {
        private string _code;
        private bool _isPayable;

        /// <summary>
        /// Receivable type code
        /// </summary>
        public string Code { get { return _code; } }

        /// <summary>
        /// Flag indicating whether or not the receivable type may be paid against by account holders
        /// </summary>
        public bool IsPayable { get { return _isPayable; } }

        /// <summary>
        /// Creates a new instance of the <see cref="PayableReceivableType"/> object.
        /// </summary>
        /// <param name="code">Receivable Type code</param>
        /// <param name="isPayable">Flag indicating whether or not the receivable type is payable</param>
        public PayableReceivableType(string code, bool isPayable)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException("code", "A receivable type code must be specified.");

            _code = code;
            _isPayable = isPayable;
        }
    }
}
