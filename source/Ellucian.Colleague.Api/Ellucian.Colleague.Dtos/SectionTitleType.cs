﻿// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined section title types (e.g. short title, long title). 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SectionTitleType : CodeItem2
    {
    }      
}          
