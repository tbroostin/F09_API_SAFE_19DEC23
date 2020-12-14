// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class CodeBuilderObject
    {
        public string SourceCode { get; set; }
        public List<string> DataValues { get; set; }
        public Dictionary<string, List<string>> DataDictionary { get; set; }
        public string[] LimitingKeys { get; set; }
        public string SelectEntity { get; set; }
        public bool ErrorFlag { get; set; }
        public List<string> ErrorMessages { get; set; }
        public CodeBuilderObject()
        {
            DataValues = new List<string>();
            ErrorMessages = new List<string>();
            ErrorFlag = false;
        }
    }
}
