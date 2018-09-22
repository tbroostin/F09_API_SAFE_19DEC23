// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Minor Code Table with Additional Fields
    /// </summary>
    [Serializable]
    public class Minor : CodeItem
    {
        /// <summary>
        /// Federal Course Classification designated to this Minor
        /// </summary>
        public string FederalCourseClassification;

        public Minor(string code, string desc)
            : base(code, desc)
        {

        }
    }
}
