// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Api.Client
{
    [Serializable]
    internal class ColleagueSessionExpiredException : Exception
    {
        public ColleagueSessionExpiredException()
        {
        }

        public ColleagueSessionExpiredException(string message) : base(message)
        {
        }

        public ColleagueSessionExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ColleagueSessionExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}