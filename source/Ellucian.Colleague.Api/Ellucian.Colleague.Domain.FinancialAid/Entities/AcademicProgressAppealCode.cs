// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Defines the code and description of an Academic Progress Appeal Code
    /// </summary>
    [Serializable]
    public class AcademicProgressAppealCode : CodeItem
    {

        public AcademicProgressAppealCode(string code, string desc)
            : base(code, desc)
        {

        }
    }
}
