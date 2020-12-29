// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    /// <summary>
    /// A pairing of an award period+financial award combination, and the corresponding indication
    /// as to whether the award can transmit excess award amounts.  
    /// </summary>
    public class AwardPeriodAwardTransmitExcessStatus
    {
        private readonly string _awardPeriodAward;
        private readonly bool _transmitExcessIndicator;

        /// <summary>
        /// Creates a new <see cref="AwardPeriodAwardTransmitExcessStatus"/> instance
        /// </summary>
        /// <param name="awardPeriodAward">A string representing the award period identifier and the award identifier,
        /// concatenated together with an asterisk in between; e.g., AWDPD1*AWD1</param>
        /// <param name="transmitExcessIndicator">A boolean indicating that the award contained in the awardPeriodAward
        /// can transmit excess financial aid.  False if the award is a D7 award with its 'Transmit Excess'
        /// indicator set to false, true in all other cases.</param>
        public AwardPeriodAwardTransmitExcessStatus(string awardPeriodAward, bool transmitExcessIndicator)
        {
            if (string.IsNullOrEmpty(awardPeriodAward))
            {
                throw new ArgumentNullException("awardPeriodAward");
            }
            _awardPeriodAward = awardPeriodAward;
            _transmitExcessIndicator = transmitExcessIndicator;
        }

        /// <summary>
        /// A string representing the award period identifier and the award identifier,
        /// concatenated together with an asterisk in between; e.g., AWDPD1*AWD1
        /// </summary>
        public string AwardPeriodAward { get { return _awardPeriodAward; } }

        /// <summary>
        /// A boolean indicating that the award contained in the awardPeriodAward
        /// can transmit excess financial aid.  False if the award is a D7 award with its 'Transmit Excess'
        /// indicator set to false, true in all other cases.
        /// </summary>
        public bool TransmitExcessIndicator { get { return _transmitExcessIndicator; } }
    }
}
