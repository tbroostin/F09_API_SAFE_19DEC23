﻿//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The LinkTypes enumeration defines the different categories of hyperlinks that can be defined in Colleague Financial Aid.
    /// </summary>
    public enum LinkTypes
    {
        /// <summary>
        /// A Link with this type should point the user to a website to complete an entrance interview
        /// </summary>
        EntranceInterview,

        /// <summary>
        /// A Link with this type should point the user to a website to complete a FAFSA application
        /// </summary>
        FAFSA,

        /// <summary>
        /// A Link with this type should point the user to a website to view FAFSA Forecaster information
        /// </summary>
        Forecaster,

        /// <summary>
        /// A Link with this type should point the user to a website that contains an institution-specific form to complete
        /// </summary>
        Form,

        /// <summary>
        /// A Link with this type should point the user to a website to complete a Master Promissory Note
        /// </summary>
        MPN,

        /// <summary>
        /// A Link with this type should point the user to a website to view Loan information
        /// </summary>
        NSLDS,

        /// <summary>
        /// A Link with this type should point the user to a website to complete a PLUS Loan application
        /// </summary>
        PLUS,

        /// <summary>
        /// A Link with this type should point the user to a website to complete PROFILE application
        /// </summary>
        PROFILE,

        /// <summary>
        /// A Link with this type should point the user to a website that the institution has determined as "helpful"
        /// </summary>
        User,

        /// <summary>
        /// A link with this type would point the user to links about Satisfactory Academic Progress information for the institution
        /// </summary>
        SatisfactoryAcademicProgress
    }
}
