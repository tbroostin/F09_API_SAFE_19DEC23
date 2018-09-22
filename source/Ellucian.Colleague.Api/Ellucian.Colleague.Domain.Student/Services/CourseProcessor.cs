// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Services
{
    public class CourseProcessor
    {
        /// <summary>
        /// Constructor for the CourseProcessor
        /// </summary>
        static CourseProcessor()
        {
        }

        /// <summary>
        /// Validate provided course information
        /// </summary>
        /// <param name="course">Course</param>
        public static void ValidateCourseData(Course course, IEnumerable<AcademicLevel> academicLevels, IEnumerable<CourseLevel> courseLevels,
            IEnumerable<CourseStatuses> courseStatuses, IEnumerable<CreditCategory> creditCategories, IEnumerable<Department> departments,
            IEnumerable<GradeScheme> gradeSchemes, IEnumerable<InstructionalMethod> instructionalMethods, IEnumerable<Subject> subjects,
            IEnumerable<Location> locations, IEnumerable<TopicCode> topicCodes)
        {
            if (course == null)
            {
                throw new ArgumentNullException("course", "A course must be provided.");
            }

            // Verify that all provided department codes are valid
            foreach (var dept in course.DepartmentCodes)
            {
                var department = departments.FirstOrDefault(d => d.Code == dept);
                if (department == null)
                {
                    throw new ArgumentException("Invalid department code: " + dept);
                }
            }

            // Verify the subject code
            var subject = subjects.FirstOrDefault(s => s.Code == course.SubjectCode);
            if (subject == null)
            {
                throw new ArgumentException("Invalid subject code: " + course.SubjectCode);
            }

            // Verify that end date, if provided, is not earlier than start date
            if (course.StartDate.HasValue && course.EndDate.HasValue && course.EndDate.Value < course.StartDate.Value)
            {
                throw new ArgumentException("End Date " + course.EndDate.Value.ToShortDateString() + " is earlier than Start Date " + course.StartDate.Value.ToShortDateString());
            }

            // Validate the credit type
            var creditCategory = creditCategories.FirstOrDefault(cc => cc.Code == course.LocalCreditType);
            if (creditCategory == null)
            {
                throw new ArgumentException("Invalid credit type code: " + course.LocalCreditType);
            }

            // Verify any course level codes provided
            if (course.CourseLevelCodes == null || course.CourseLevelCodes.Count() == 0)
            {
                foreach (var courselevel in course.CourseLevelCodes)
                {
                    var courseLevel = courseLevels.FirstOrDefault(cl => cl.Code == courselevel);
                    if (courseLevel == null)
                    {
                        throw new ArgumentException("Invalid course level code: " + courselevel);
                    }
                }
            }

            // Verify that all provided academic level codes are valid
            var acadLevel = academicLevels.FirstOrDefault(al => al.Code == course.AcademicLevelCode);
            if (acadLevel == null)
            {
                throw new ArgumentException("Invalid academic level code: " + course.AcademicLevelCode);
            }

            // Validate the grade scheme, if it exists
            if (!string.IsNullOrEmpty(course.GradeSchemeCode))
            {
                var scheme = gradeSchemes.FirstOrDefault(gs => gs.Code == course.GradeSchemeCode);
                if (scheme == null)
                {
                    throw new ArgumentException("Invalid grade scheme code: " + course.GradeSchemeCode);
                }
            }

            // Verify that all provided instructional method codes are valid
            foreach (var method in course.InstructionalMethodCodes)
            {
                var instructionalMethod = instructionalMethods.FirstOrDefault(im => im.Code == method);
                if (instructionalMethod == null)
                {
                    throw new ArgumentException("Invalid instructional method: " + method);
                }
            }

            foreach (var locationCode in course.LocationCodes)
            {
                var location = locations.FirstOrDefault(l => l.Code == locationCode);
                if (location == null)
                {
                    throw new ArgumentException("Invalid location code: " + locationCode);
                }
            }

            if (!string.IsNullOrEmpty(course.TopicCode))
            {
                var topicCode = topicCodes.FirstOrDefault(tc => tc.Code == course.TopicCode);
                if (topicCode == null)
                {
                    throw new ArgumentException("Invalid topic code: " + course.TopicCode);
                }
            }
        }
    }
}
