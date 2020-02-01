// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A pairing of an award period+financial award combination, and the corresponding indication
    /// as to whether the award can transmit excess award amounts.  
    /// </summary>
    public class AwardPeriodAwardTransmitExcessStatus
    {
        /// <summary>
        /// A combination of award periods and awards to evaluate.  The award periods and awards
        /// are represented by their identifiers.  The identifiers are concatenated together with a separating
        /// asterisk; e.g., AWDP1*AWARD1.
        /// </summary>
        public string AwardPeriodAward { get; set; }

        /// <summary>
        /// False if the award component of AwardPeriodAward is a D7 award with its
        /// 'Transmit Excess' flag set to No.  True in all other conditions.
        /// </summary>
        public bool TransmitExcessIndicator { get; set; }
    }
}
