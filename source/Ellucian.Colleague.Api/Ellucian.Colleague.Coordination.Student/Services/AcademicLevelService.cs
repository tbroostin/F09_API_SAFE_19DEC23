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
    public class AcademicLevelService : IAcademicLevelService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger logger;

        public AcademicLevelService(IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger)
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
        /// Gets all academic levels
        /// </summary>
        /// <returns>Collection of AcademicLevel DTO objects</returns>
        public IEnumerable<Ellucian.Colleague.Dtos.AcademicLevel> GetAcademicLevels()
        {
            var academicLevelCollection = new List<Ellucian.Colleague.Dtos.AcademicLevel>();

            var academicLevelEntities = _studentReferenceDataRepository.AcademicLevels;
            if (academicLevelEntities != null && academicLevelEntities.Count() > 0)
            {
                foreach (var academicLevel in academicLevelEntities)
                {
                    academicLevelCollection.Add(ConvertAcademicLevelEntityToDto(academicLevel));
                }
            }
            return academicLevelCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN LDM</remarks>
        /// <summary>
        /// Converts an AcademicLevel domain entity to its corresponding AcademicLevel DTO
        /// </summary>
        /// <param name="source">AcademicLevel domain entity</param>
        /// <returns>AcademicLevel DTO</returns>
        private Ellucian.Colleague.Dtos.AcademicLevel ConvertAcademicLevelEntityToDto(Ellucian.Colleague.Domain.Student.Entities.AcademicLevel source)
        {
            var academicLevel = new Ellucian.Colleague.Dtos.AcademicLevel();

            academicLevel.Guid = source.Guid;
            academicLevel.Abbreviation = source.Code;
            academicLevel.Title = source.Code;
            academicLevel.Description = source.Description;

            return academicLevel;
        }
    }
}
