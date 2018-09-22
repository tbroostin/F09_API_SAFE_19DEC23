// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Indicates the type of the student's program notice text.
    /// </summary>
    [Serializable]
    public enum EvaluationNoticeType
    {
        StudentProgram, Program, Start, End
    }
}
