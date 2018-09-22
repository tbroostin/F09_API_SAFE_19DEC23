/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Defines an earnings type for wages or leave associated with an employment position
    /// </summary>
    public class EarningsType
    {
        /// <summary>
        /// The database Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The earnings type description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The category specifies whether earnings are regular, overtime, leave, college work study or miscellaneous
        /// </summary>
        public EarningsCategory Category { get; set; }

        /// <summary>
        /// Is this earnings type active or not?
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The method specifies whether the earnings are for leave accrued, leave taken, or time not paid
        /// </summary>
        public EarningsMethod Method { get; set; }

        /// <summary>
        /// A leave type associated to this earnings type
        /// </summary>
        public LeaveTypeCategory LeaveTypeCategory { get; set; }

        /// <summary>
        /// A multipler that can be applied to base earnings for overtime and compensatory earnings. The default factor is 1 (no earnings multiplier).
        /// </summary>
        public decimal Factor { get; set; }
    }
}
