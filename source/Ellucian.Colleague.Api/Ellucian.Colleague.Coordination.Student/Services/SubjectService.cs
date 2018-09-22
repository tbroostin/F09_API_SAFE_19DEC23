// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class SubjectService : ISubjectService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger logger;

        public SubjectService(IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger)
        {
            _studentReferenceDataRepository = studentReferenceDataRepository;
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN LDM</remarks>
        /// <summary>
        /// Gets all subjects
        /// </summary>
        /// <returns>Collection of Subject DTO objects</returns>
        public IEnumerable<Ellucian.Colleague.Dtos.Subject> GetSubjects()
        {
            var subjectCollection = new List<Ellucian.Colleague.Dtos.Subject>();

            var subjectEntities = _studentReferenceDataRepository.Subjects;
            if (subjectEntities != null && subjectEntities.Count() > 0)
            {
                foreach (var subject in subjectEntities)
                {
                    subjectCollection.Add(ConvertSubjectEntityToDto(subject));
                }
            }
            return subjectCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN LDM</remarks>
        /// <summary>
        /// Converts a Subject domain entity to its corresponding Subject DTO
        /// </summary>
        /// <param name="source">Subject domain entity</param>
        /// <returns>Subject DTO</returns>
        private Ellucian.Colleague.Dtos.Subject ConvertSubjectEntityToDto(Ellucian.Colleague.Domain.Student.Entities.Subject source)
        {
            var subject = new Ellucian.Colleague.Dtos.Subject();

            subject.Guid = source.Guid;
            subject.Abbreviation = source.Code;
            subject.Title = source.Code;
            subject.Description = source.Description;

            return subject;
        }
    }
}
