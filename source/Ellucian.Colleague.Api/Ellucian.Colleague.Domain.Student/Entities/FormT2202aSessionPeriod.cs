// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class FormT2202aSessionPeriod
    {
        #region Student attributes
        /// <summary>
        /// Student's from Year.
        /// </summary>
        public string StudentFromYear { get { return this.studentFromYear; } }
        private readonly string studentFromYear;

        /// <summary>
        /// Student's from month.
        /// </summary>
        public string StudentFromMonth { get { return this.studentFromMonth; } }
        private readonly string studentFromMonth;

        /// <summary>
        /// Student's to Year.
        /// </summary>
        public string StudentToYear { get { return this.studentToYear; } }
        private readonly string studentToYear;

        /// <summary>
        /// Student's to month.
        /// </summary>
        public string StudentToMonth { get { return this.studentToMonth; } }
        private readonly string studentToMonth;

        /// <summary>
        /// Student's box A amount in string format.
        /// </summary>
        public string BoxAAmountString
        {
            get
            {
                if(this.boxAAmount.HasValue)
                {
                    return this.boxAAmount.Value.ToString("N2");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Student's box A amount.
        /// </summary>
        public decimal? BoxAAmount { get { return this.boxAAmount; } }
        private readonly decimal? boxAAmount;

        /// <summary>
        /// Student's box B part-time hours.
        /// </summary>
        public int? BoxBHours { get { return this.boxBHours; } }
        private readonly int? boxBHours;

        /// <summary>
        /// Student's box C full-time hours.
        /// </summary>
        public int? BoxCHours { get { return this.boxCHours; } }
        private readonly int? boxCHours;
        #endregion

        public FormT2202aSessionPeriod(string fromYear, string fromMonth, string toYear, string toMonth, decimal? tuitionAmount, int? parttimeHours, int? fulltimeHours)
        {
            if (string.IsNullOrEmpty(fromYear))
            {
                throw new ArgumentNullException("fromYear", "From year is required.");
            }

            if (string.IsNullOrEmpty(fromMonth))
            {
                throw new ArgumentNullException("fromMonth", "From month is required.");
            }

            if (string.IsNullOrEmpty(toYear))
            {
                throw new ArgumentNullException("toYear", "To year is required.");
            }

            if (string.IsNullOrEmpty(toMonth))
            {
                throw new ArgumentNullException("toMonth", "To month is required.");
            }

            if (tuitionAmount == null && parttimeHours == null && fulltimeHours == null)
            {
                throw new ArgumentNullException("tuitionAmount", "Tuition amount, part-time hours, and full-time hours cannot all be null.");
            }
            this.studentFromYear = fromYear;
            this.studentFromMonth = fromMonth;
            this.studentToYear = toYear;
            this.studentToMonth = toMonth;
            this.boxAAmount = tuitionAmount;
            this.boxBHours = parttimeHours;
            this.boxCHours = fulltimeHours;
        }
    }
}
