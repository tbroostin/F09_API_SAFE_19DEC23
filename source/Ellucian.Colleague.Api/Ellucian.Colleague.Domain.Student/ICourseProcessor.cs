// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student
{
    public interface ICourseProcessor
    {
        /// <summary>
        /// Validate the data on a given course
        /// </summary>
        /// <param name="course">Course</param>
        void ValidateCourseData(Course course, IEnumerable<AcademicLevel> academicLevels, IEnumerable<CourseLevel> courseLevels,
            IEnumerable<CourseStatuses> courseStatuses, IEnumerable<CreditCategory> creditCategories, IEnumerable<Department> departments,
            IEnumerable<GradeScheme> gradeSchemes, IEnumerable<InstructionalMethod> instructionalMethods, IEnumerable<Subject> subjects,
            IEnumerable<Location> locations, IEnumerable<TopicCode> topicCodes);
    }
}
