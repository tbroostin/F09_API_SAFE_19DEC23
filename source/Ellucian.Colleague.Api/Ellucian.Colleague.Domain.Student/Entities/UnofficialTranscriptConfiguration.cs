// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information for unofficial transcript report parameters.
    /// </summary>
    [Serializable]
    public class UnofficialTranscriptConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether this unofficial transcript report to use these below parameters.
        /// </summary>
        public bool IsUseTanscriptFormat { get; set; }

        /// <summary>
        /// Report Font Size
        /// </summary>
        public string FontSize { get; set; }

        /// <summary>
        /// Report Page Height
        /// </summary>
        public string PageHeight { get; set; }

        /// <summary>
        /// Report Page Width
        /// </summary>
        public string PageWidth { get; set; }

        /// <summary>
        /// Report Top Margin
        /// </summary>
        public string TopMargin { get; set; }

        /// <summary>
        /// Report Bottom Margin
        /// </summary>
        public string BottomMargin { get; set; }

        /// <summary>
        /// Report Right Margin
        /// </summary>
        public string RightMargin { get; set; }

        /// <summary>
        /// Report Left Margin
        /// </summary>
        public string LeftMargin { get; set; }
    }
}
