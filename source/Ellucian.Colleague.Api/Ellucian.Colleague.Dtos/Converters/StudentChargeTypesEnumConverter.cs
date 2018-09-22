﻿// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.Converters
{
    /// <summary>
    /// JSON converter for AddressType enum - the automation
    /// people want to get a sensible message back on garbage input for enums.
    /// </summary>
    public class StudentChargeTypesEnumConverter : StringEnumConverter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;
            try
            {
                return Enum.Parse(typeof(StudentChargeTypes), enumString, true);
            }
            catch
            {
                if (enumString == null) { enumString = "null"; }
                throw new ArgumentException("Student charge type", "Error converting supplied value '" + enumString + "' to a valid address type.");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}



