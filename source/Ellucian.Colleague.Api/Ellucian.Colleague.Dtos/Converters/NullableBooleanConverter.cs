// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.Converters
{
    /// <summary>
    /// Nullable boolean converter
    /// </summary>
    public class NullableBooleanConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            //return objectType == typeof(bool?);
            if (Nullable.GetUnderlyingType(objectType) != null)
            {
                return Nullable.GetUnderlyingType(objectType) == typeof(bool?);
            }
            return objectType == typeof(bool?);
        }

        /// <summary>
        /// Reads the input valid boolean value and returns true, false or null
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(Nullable<>))
            {
                Type underlyingType = Nullable.GetUnderlyingType(objectType);
                if (underlyingType == typeof(bool) && reader.ValueType == typeof(bool))
                {
                    if (reader.Value.ToString().ToLower().Trim() != "true" && reader.Value.ToString().ToLower().Trim() != "false")
                        throw new ColleagueWebApiDtoException("Unexpected value when reading boolean.");
                }
                else
                    throw new ColleagueWebApiDtoException("Unexpected value when reading boolean.");
            }

            // allow the value to be null
            if (reader == null || reader.Value == null)
                return null;

            return reader.Value;
        }

        /// <summary>
        /// Specifies that this converter will not participate in writing results.
        /// </summary>
        public override bool CanWrite { get { return false; } }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
