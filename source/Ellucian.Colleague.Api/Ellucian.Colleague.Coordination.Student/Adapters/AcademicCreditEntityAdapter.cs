// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.Student;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an academic credit entity to an academic credit DTO
    /// </summary>
    public class AcademicCreditEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit>
    {
        /// <summary>
        /// Academic credit entity adapter (current Entity to Academic Credit version 1) constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public AcademicCreditEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Academic credit entity to dto mapping: Current AcademicCredit entity to AcademicCredit (version 1) dto 
        /// </summary>
        /// <param name="Source"><see cref="Ellucian.Colleague.Domain.Student.Entities.AcademicCredit">AcademicCredit</see> entity object</cref></param>
        /// <returns></returns>
        public override Ellucian.Colleague.Dtos.Student.AcademicCredit MapToType(Ellucian.Colleague.Domain.Student.Entities.AcademicCredit Source)
        {
            //var academicCreditDto = base.MapToType(Source);
            //var academicCreditDto = new Ellucian.Colleague.Dtos.Student.AcademicCredit();
            //// Custom: Need only the ID of the course
            //if (Source.Course != null)
            //{
            //    academicCreditDto.CourseId = Source.Course.Id;
            //}
            //// Custom: Map MidTermGrade offset back to regular datetime object
            //academicCreditDto.MidTermGrades = new List<Ellucian.Colleague.Dtos.Student.MidTermGrade>();
            //var midTermGradeAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>();
            //foreach (var midTermGrade in Source.MidTermGrades)
            //{
            //    academicCreditDto.MidTermGrades.Add(midTermGradeAdapter.MapToType(midTermGrade));
            //}
            //// Custom: convert status to string
            //academicCreditDto.Status = Source.Status.ToString();
            //// Custom: Return only the verified grade ID
            //if (Source.VerifiedGrade != null)
            //{
            //    academicCreditDto.VerifiedGradeId = Source.VerifiedGrade.Id;
            //}
            //// Custom: map Offset back to regular datetime object
            //academicCreditDto.VerifiedGradeTimestamp = Source.VerifiedGradeTimestamp.HasValue ? Source.VerifiedGradeTimestamp.Value.DateTime : (DateTime?)null;

            var academicCreditDto = new Ellucian.Colleague.Dtos.Student.AcademicCredit();

            academicCreditDto.AdjustedCredit = Source.AdjustedCredit ?? 0;
            academicCreditDto.CompletedCredit = Source.CompletedCredit??0;
            academicCreditDto.ContinuingEducationUnits = Source.ContinuingEducationUnits;
            // Custom: Need only the ID of the course
            if (Source.Course != null)
            {
                academicCreditDto.CourseId = Source.Course.Id;
            }
            academicCreditDto.CourseName = Source.CourseName;
            academicCreditDto.Credit = Source.Credit;
            academicCreditDto.GpaCredit = Source.GpaCredit??0;
            academicCreditDto.GradePoints = Source.GradePoints;
            var gradingTypeAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>();
            academicCreditDto.GradingType = gradingTypeAdapter.MapToType(Source.GradingType);
            academicCreditDto.HasVerifiedGrade = Source.HasVerifiedGrade;
            academicCreditDto.Id = Source.Id;
            academicCreditDto.IsCompletedCredit = Source.IsCompletedCredit;
            academicCreditDto.IsNonCourse = Source.IsNonCourse;
            // Custom: Map MidTermGrade offset back to regular datetime object
            academicCreditDto.MidTermGrades = new List<Ellucian.Colleague.Dtos.Student.MidTermGrade>();
            var midTermGradeAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>();
            foreach (var midTermGrade in Source.MidTermGrades)
            {
                academicCreditDto.MidTermGrades.Add(midTermGradeAdapter.MapToType(midTermGrade));
            }
            var replacedStatusAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>();
            academicCreditDto.ReplacedStatus = replacedStatusAdapter.MapToType(Source.ReplacedStatus);
            var replacementStatusAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>();
            academicCreditDto.ReplacementStatus = replacementStatusAdapter.MapToType(Source.ReplacementStatus);
            academicCreditDto.SectionId = Source.SectionId;
            academicCreditDto.SectionNumber = Source.SectionNumber;
            // Custom: convert status to string
            academicCreditDto.Status = Source.Status.ToString();
            academicCreditDto.TermCode = Source.TermCode;
            academicCreditDto.Title = Source.Title;
            // Custom: Return only the verified grade ID
            if (Source.VerifiedGrade != null)
            {
                academicCreditDto.VerifiedGradeId = Source.VerifiedGrade.Id;
            }
            // Custom: map Offset back to regular datetime object
            academicCreditDto.VerifiedGradeTimestamp = Source.VerifiedGradeTimestamp.HasValue ? Source.VerifiedGradeTimestamp.Value.DateTime : (DateTime?)null;
            
            return academicCreditDto;
        }
    }
}
