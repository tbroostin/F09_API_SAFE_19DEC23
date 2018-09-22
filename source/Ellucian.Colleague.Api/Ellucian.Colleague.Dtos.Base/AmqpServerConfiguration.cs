// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Advanced Message Queuing Protocol Server Configuration Information
    /// </summary>
    [DataContract]
    public class AmqpServerConfiguration
    {
        /// <summary>
        /// Advanced Message Queuing Protocol Server Base URL
        /// </summary>
        [DataMember(Name = "secure")]
        public bool ConnectionIsSecure { get; set; }

        /// <summary>
        /// Advanced Message Queuing Protocol Server Base URL
        /// </summary>
        [DataMember(Name = "host")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// Advanced Message Queuing Protocol Server Port Number
        /// </summary>
        [DataMember(Name = "port")]
        public int PortNumber { get; set; }

        /// <summary>
        /// Name of the virtual host that specifies the namespace for entities (exchanges and queues) referred to by the protocol.
        /// </summary>
        [DataMember(Name = "virtualHost")]
        public string VirtualHost { get; set; }

        /// <summary>
        /// Time (in seconds) to wait while establishing a connection with the Advanced Message Queuing Protocol server
        /// </summary>
        [DataMember(Name = "timeout")]
        public int? ConnectionTimeout { get; set; }

        /// <summary>
        /// Flag indicating whether or not to automatically recover Advanced Message Queuing Protocol messages
        /// </summary>
        [DataMember(Name = "autoRecovery")]
        public bool AutomaticallyRecoverMessages { get; set; }

        /// <summary>
        /// Heartbeat value (in seconds) to negotiate with the Advanced Message Queuing Protocol server
        /// </summary>
        [DataMember(Name = "heartbeat")]
        public int? Heartbeat { get; set; }

        ///// <summary>
        ///// Username for authentication to the Advanced Message Queuing Protocol Server
        ///// </summary>
        //[DataMember(Name = "username")]
        //public string Username { get; set; }

        ///// <summary>
        ///// Password for authentication to the Advanced Message Queuing Protocol Server
        ///// </summary>
        //[DataMember(Name = "password")]
        //public string Password { get; set; }
    }
}
