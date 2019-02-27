// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of supported tax forms
    /// </summary>
    [Serializable]
    public enum TaxForms
    {
        // Prefixed with "Form" because C# identifiers cannot begin with a number.
        FormW2,
        Form1095C,
        Form1098,
        Form1098T,
        Form1098E,
        FormT4,
        FormT4A,
        FormT2202A,
        Form1099MI,
        FormW2C,
    }
}
