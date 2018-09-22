// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains the registration options for single user
    /// </summary>
    [Serializable]
    public class RegistrationOptions
    {
        /// <summary>
        /// The Id of the person to whom these registration options belong.
        /// </summary>
        public string PersonId { get { return _personId; } }
        private string _personId;
        
        /// <summary>
        /// The list of grading types this person is allowed to used in registration.
        /// </summary>
        public ReadOnlyCollection<GradingType> GradingTypes { get; private set; }
        private readonly List<GradingType> _gradingTypes = new List<GradingType>();

        /// <summary>
        /// Creates a RegistrationOptions object
        /// </summary>
        /// <param name="personId">Id of the Person</param>
        /// <param name="gradingTypes">List of grading types</param>
        public RegistrationOptions(string personId, List<GradingType> gradingTypes)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person Id is required.");
            }
            if (gradingTypes == null)
            {
                throw new ArgumentNullException("gradingTypes", "List of grading types must be provided.");
            }
            _personId = personId;
            _gradingTypes = gradingTypes;

            GradingTypes = _gradingTypes.AsReadOnly();
        }
    }
}
