// Copyright 2015 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Petition Status (Granted, Denied, etc)
    /// </summary>>
    public class PetitionStatus
    {
        /// <summary>
        /// The petition code (unique identifier)
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// The petition description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Indicates if this status code means that the petition is granted or not. 
        /// </summary>
        public bool IsGranted { get; set; }
    }
}
