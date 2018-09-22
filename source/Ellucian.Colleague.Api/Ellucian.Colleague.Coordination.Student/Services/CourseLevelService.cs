// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CourseLevelService : ICourseLevelService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger logger;

        public CourseLevelService(IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger)
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
        /// Gets all course levels
        /// </summary>
        /// <returns>Collection of CourseLevel DTO objects</returns>
        public IEnumerable<Ellucian.Colleague.Dtos.CourseLevel> GetCourseLevels()
        {
            var courseLevelCollection = new List<Ellucian.Colleague.Dtos.CourseLevel>();

            var courseLevelEntities = _studentReferenceDataRepository.CourseLevels;
            if (courseLevelEntities != null && courseLevelEntities.Count() > 0)
            {
                foreach (var courseLevel in courseLevelEntities)
                {
                    courseLevelCollection.Add(ConvertCourseLevelEntityToDto(courseLevel));
                }
            }
            return courseLevelCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN LDM</remarks>
        /// <summary>
        /// Converts a CourseLevel domain entity to its corresponding CourseLevel DTO
        /// </summary>
        /// <param name="source">CourseLevel domain entity</param>
        /// <returns>CourseLevel DTO</returns>
        private Ellucian.Colleague.Dtos.CourseLevel ConvertCourseLevelEntityToDto(Ellucian.Colleague.Domain.Student.Entities.CourseLevel source)
        {
            var courseLevel = new Ellucian.Colleague.Dtos.CourseLevel();

            courseLevel.Guid = source.Guid;
            courseLevel.Abbreviation = source.Code;
            courseLevel.Title = source.Code;
            courseLevel.Description = source.Description;

            return courseLevel;
        }
    }
}
