// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Data.Student;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestRequirementRepository : IRequirementRepository
    {

        public List<RequirementType> RequirementTypes
        {
            get
            {
                return new List<RequirementType>()
                {
                    new RequirementType("MAJ", "Major", "1"),
                    new RequirementType("MIN", "Minor", "2"),
                    new RequirementType("SPC", "Specialization", "3"),
                    new RequirementType("CCD", "Ccd", "2"),
                    new RequirementType("OTH", "Other", "5"),
                    new RequirementType("GEN", "General (Core)", "6"),
                    new RequirementType("ELE", "Elective", "7"),
                };
            }
        }

        private List<Requirement> requirementArray 
        {
            get
            { 
                return new List<Requirement>() 
                {
                new Requirement("00001","GEN.ED",  "Lorem ipsum foo","UG", new RequirementType("GEN", "General (Core)", "6")),
                new Requirement("00002","BA.HIST", "Lorem ipsum foo","UG", new RequirementType("MAJ", "Major", "1")), 
                new Requirement("00003","MA.HIST", "Lorem ipsum foo","GR", new RequirementType("MAJ", "Major", "1")),
                new Requirement("00004","MIN.HIST","Lorem ipsum foo","UG", new RequirementType("MIN", "Minor", "2")),
                new Requirement("00005","BS.BIO",  "Lorem ipsum foo","UG", new RequirementType("MAJ", "Major", "1")),
                new Requirement("00006","MS.BIO",  "Lorem ipsum foo","GR", new RequirementType("MAJ", "Major", "1")),
                new Requirement("00007","IN.BIO",  "Lorem ipsum foo","UG", new RequirementType("MIN", "Minor", "2")),
                new Requirement("00008","PREREQ1", "Requires course MATH*100", "UG", new RequirementType("PRE", "Prerequisite", null)),
                new Requirement("00009","PREREQ2", "Requires Course ENGL*101", "UG", new RequirementType("PRE", "Prerequisite", null)),
                new Requirement("00010","COREQ1", "Requires Course MATH*201", "UG", new RequirementType("PRE", "Prerequisite", null)),
                new Requirement("00010","COREQ2", "Recommends Course BIOL*110", "UG", new RequirementType("PRE", "Prerequisite", null)),
                new Requirement("00010","REQ1", "Requires Course MATH*201", "UG", new RequirementType("PRE", "Prerequisite", null))
                };
            }
        }

        private async Task<List<Requirement>> BuildRequirementArrayAsync()
        {
            var reqArray = new List<Requirement>();
            int x = 0;
            foreach (var req in requirementArray)
            {
                x++;
                var Subreq = new Subrequirement(x.ToString(), "Foo Subreq");
                Subreq.Requirement = req;
                Subreq.Groups.Add(new Group(x.ToString(), "Bar Group", Subreq));
                if (req.Code == "PREREQ1")
                {
                    // Create prerequisite that requires math*100
                    req.SubRequirements.Add(Subreq);
                    var group = Subreq.Groups.First();
                    group.Courses = new List<string>() { "143" };  // ID for MATH*100 in TestCourseRepository
                    //group.MinCredits = 3.0m;
                    req.AllowedGrades = new List<Grade>() { (await new TestGradeRepository().GetAsync()).FirstOrDefault() };
                }
                if (req.Code == "PREREQ2")
                {
                    // Create prerequisite that requires engl*102
                    req.SubRequirements.Add(Subreq);
                    var group = Subreq.Groups.First();
                    group.Courses = new List<string>() { "7701" }; // ID for DENT*104 in TestCourseRepository (term and nonterm sections available)
                    //group.MinCredits = 3.0m;
                }

                if (req.Code == "COREQ1")
                {
                    // Create requisite that requires MATH-201 (course 47) at the same time
                    req.SubRequirements.Add(Subreq);
                    var group = Subreq.Groups.First();
                    group.Courses = new List<string>() { "47" }; // ID for MATH-201 in TestCourseRepository 
                }

                if (req.Code == "COREQ2")
                {
                    // Create requisite that requires MATH-201 (course 47) at the same time
                    req.SubRequirements.Add(Subreq);
                    var group = Subreq.Groups.First();
                    group.Courses = new List<string>() { "110" }; // ID for MATH-201 in TestCourseRepository 
                }

                if (req.Code == "REQ1")
                {
                    // Create requisite that requires MATH-201 (course 47) at the same time
                    req.SubRequirements.Add(Subreq);
                    var group = Subreq.Groups.First();
                    group.Courses = new List<string>() { "47" }; // ID for MATH-201 in TestCourseRepository 

                    // Rules added for CourseServiceTests/ReturnsCorrectCoursesForProgramRequirementWithRule
                    // Add rules to every level of req/subreq/group. We don't care about what this will do to the search results (that's the job
                    // of a different test. This setup is needed to verify that the course search builds a rule request for each and every course
                    // and each and every rule at every level of the requirments.

                    // Add rules to the requirement
                    var rd = new RuleDescriptor() { Id = "REQRULE1", PrimaryView = "COURSES" };
                    rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NO", "NE", "\"100\""));
                    var rule = (Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)new CourseRuleAdapter().Create(rd);
                    req.AcademicCreditRules.Add(new RequirementRule((Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)rule));
                    rd = new RuleDescriptor() { Id = "REQRULE2", PrimaryView = "COURSES" };
                    rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NO", "NE", "\"200\""));
                    rule = (Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)new CourseRuleAdapter().Create(rd);
                    req.AcademicCreditRules.Add(new RequirementRule((Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)rule));

                    // Add rules to the subrequirement
                    rd = new RuleDescriptor() { Id = "SUBREQRULE1", PrimaryView = "COURSES" };
                    rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NO", "NE", "\"300\""));
                    rule = (Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)new CourseRuleAdapter().Create(rd);
                    Subreq.AcademicCreditRules.Add(new RequirementRule((Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)rule));
                    rd = new RuleDescriptor() { Id = "SUBREQRULE2", PrimaryView = "COURSES" };
                    rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NO", "NE", "\"400\""));
                    rule = (Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)new CourseRuleAdapter().Create(rd);
                    Subreq.AcademicCreditRules.Add(new RequirementRule((Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)rule));

                    // Add rules to the group
                    rd = new RuleDescriptor() { Id = "GROUPRULE1", PrimaryView = "COURSES" };
                    rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NO", "NE", "\"500\""));
                    rule = (Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)new CourseRuleAdapter().Create(rd);
                    group.AcademicCreditRules.Add(new RequirementRule((Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)rule));
                    rd = new RuleDescriptor() { Id = "GROUPRULE2", PrimaryView = "COURSES" };
                    rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NO", "NE", "\"600\""));
                    rule = (Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)new CourseRuleAdapter().Create(rd);
                    group.AcademicCreditRules.Add(new RequirementRule((Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)rule));
                }
                reqArray.Add(req);
            }

            return await Task.FromResult(reqArray);
        }

        public async Task<Requirement> GetAsync(string requirementCode)
        {
            try
            {
                var req = (await BuildRequirementArrayAsync()).Where(r => r.Code == requirementCode).First();
                return req;
            }
            catch
            {
                throw new KeyNotFoundException("Requirement " + requirementCode.ToString() + "not found");
            }
        }

        public async Task<IEnumerable<Requirement>> GetAsync(IEnumerable<string> requirementCodes, ProgramRequirements programrequirements)
        {
            IEnumerable<Requirement> requirements = await GetAsync(requirementCodes);
            foreach (var req in requirements)
            {
                req.ProgramRequirements = programrequirements;
            }
            return requirements;
        }

        // Always return the full set
        public async Task<IEnumerable<Requirement>> GetAsync(IEnumerable<string> requirementCodes)
        {
            return await BuildRequirementArrayAsync();
        }

        public async Task<DegreeAuditParameters> GetDegreeAuditParametersAsync()
        {
            var newParameters = new DegreeAuditParameters(ExtraCourses.Apply, false,false, false);
            return await Task.FromResult(newParameters);
        }
    }
}
