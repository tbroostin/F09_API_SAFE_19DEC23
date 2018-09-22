// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Exceptions
{
    public class ExistingStudentPetitionException : System.Exception
    {
        private readonly string _ExistingStudentPetitionId;
        /// <summary>
        /// Id of the student petition that already exists
        /// </summary>
        public string ExistingStudentPetitionId { get { return _ExistingStudentPetitionId; } }

        private readonly string _ExistingStudentPetitionType;
        /// <summary>
        /// Type of the student petition that already exists (string)
        /// </summary>
        public string ExistingStudentPetitionType { get { return _ExistingStudentPetitionType; } }

        private readonly string _ExistingStudentPetitionSectionId;
        /// <summary>
        /// Section Id of the student petition that already exists
        /// </summary>
        public string ExistingStudentPetitionSectionId { get { return _ExistingStudentPetitionSectionId; } }

        public ExistingStudentPetitionException()
        {

        }

        /// <summary>
        /// Constructor for a new ExistingStudentPetitionException
        /// </summary>
        /// <param name="message">Message to use</param>
        /// <param name="existingStudentPetitionId">Id of the existing petition</param>
        public ExistingStudentPetitionException(string message, string existingStudentPetitionId, string existingStudentPetitionSectionId, string existingStudentPetitionType)
            : base(message)
        {
            _ExistingStudentPetitionId = existingStudentPetitionId;
            _ExistingStudentPetitionSectionId = existingStudentPetitionSectionId;
            _ExistingStudentPetitionType = existingStudentPetitionType;
        }
    }
}
