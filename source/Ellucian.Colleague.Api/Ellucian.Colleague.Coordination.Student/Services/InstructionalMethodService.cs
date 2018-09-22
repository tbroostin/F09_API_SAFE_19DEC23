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
    public class InstructionalMethodService : IInstructionalMethodService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger logger;

        public InstructionalMethodService(IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger)
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
        /// Gets all instructional methods
        /// </summary>
        /// <returns>Collection of InstructionalMethod DTO objects</returns>
        public IEnumerable<Ellucian.Colleague.Dtos.InstructionalMethod> GetInstructionalMethods()
        {
            var instructionalMethodCollection = new List<Ellucian.Colleague.Dtos.InstructionalMethod>();

            var instructionalMethodEntities = _studentReferenceDataRepository.InstructionalMethods;
            if (instructionalMethodEntities != null && instructionalMethodEntities.Count() > 0)
            {
                foreach (var instructionalMethod in instructionalMethodEntities)
                {
                    instructionalMethodCollection.Add(ConvertInstructionalMethodEntityToDto(instructionalMethod));
                }
            }
            return instructionalMethodCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN LDM</remarks>
        /// <summary>
        /// Converts an InstructionalMethod domain entity to its corresponding InstructionalMethod DTO
        /// </summary>
        /// <param name="source">InstructionalMethod domain entity</param>
        /// <returns>InstructionalMethod DTO</returns>
        private Ellucian.Colleague.Dtos.InstructionalMethod ConvertInstructionalMethodEntityToDto(Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod source)
        {
            var instructionalMethod = new Ellucian.Colleague.Dtos.InstructionalMethod();

            instructionalMethod.Guid = source.Guid;
            instructionalMethod.Abbreviation = source.Code;
            instructionalMethod.Title = source.Code;
            instructionalMethod.Description = source.Description;

            return instructionalMethod;
        }
    }
}
