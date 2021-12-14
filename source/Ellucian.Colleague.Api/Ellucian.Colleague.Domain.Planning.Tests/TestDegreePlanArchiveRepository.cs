// Copyright 2013-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Domain.Planning.Tests
{
    public class TestDegreePlanArchiveRepository : IDegreePlanArchiveRepository
    {
        public async Task<Planning.Entities.DegreePlanArchive> AddAsync(Planning.Entities.DegreePlanArchive degreePlanArchive)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Planning.Entities.DegreePlanArchive>> GetDegreePlanArchivesAsync(int degreePlanId)
        {
            IEnumerable<DegreePlan> plans = (await new TestStudentDegreePlanRepository().GetAsync()).Where(p=>p.Id == degreePlanId);
            return await BuildDegreePlanArchiveAsync(plans);
        }

        public async Task<Planning.Entities.DegreePlanArchive> GetDegreePlanArchiveAsync(int degreePlanArchiveId)
        {
            IEnumerable<DegreePlan> plans = await new TestStudentDegreePlanRepository().GetAsync();
            return (await BuildDegreePlanArchiveAsync(plans)).FirstOrDefault(p => p.Id == degreePlanArchiveId);
        }

        private  async  Task<IEnumerable<Planning.Entities.DegreePlanArchive>> BuildDegreePlanArchiveAsync(IEnumerable<DegreePlan> plans)
        {
            int counter = 1;

            List<Planning.Entities.DegreePlanArchive> archives = new List<Planning.Entities.DegreePlanArchive>();
            foreach (var plan in plans)
            {
                // first archive has no archived courses
                var archiveCourses = new List<ArchivedCourse>();

                Planning.Entities.DegreePlanArchive archive1 = new Planning.Entities.DegreePlanArchive(counter, plan.Id, plan.PersonId, 1);
                counter = counter + 1;
                // The first archive will have no courses and no notes on it. 
                archive1.CreatedBy = "0000222";
                archive1.CreatedDate = new DateTime(2013, 2, 1, 9, 0, 0);
                archive1.ReviewedBy = "0000222";
                archive1.ReviewedDate = new DateTime(2013, 2, 1, 8, 0, 0);
                archive1.StudentPrograms =  (await new TestStudentProgramRepository().GetAsync(plan.PersonId)).ToList();
                archive1.Notes = new List<DegreePlanNote>();
                archive1.ArchivedCourses = archiveCourses;
                archives.Add(archive1);

                // second archive has archived courses
                archiveCourses = ( await BuildArchiveCoursesAsync(plan)).ToList();

                Planning.Entities.DegreePlanArchive archive2 = new Planning.Entities.DegreePlanArchive(counter, plan.Id, plan.PersonId, 10);
                counter = counter + 1;
                archive2.CreatedBy = "0000111";
                archive2.CreatedDate = new DateTime(2013, 5, 1, 9, 0, 0);
                archive2.ReviewedBy = "0000111";
                archive2.ReviewedDate = new DateTime(2013, 5, 1, 8, 0, 0);
                archive2.StudentPrograms =(await  new TestStudentProgramRepository().GetAsync(plan.PersonId)).ToList();

                // If this is degree plan 2 then add 2 notes
                if (plan.Id == 2)
                {
                    var noteHistory = new List<DegreePlanNote>();
                    noteHistory.Add(new DegreePlanNote(514, "1212121", new DateTime(2013, 11, 12, 11, 15, 0), "This is first note."));
                    noteHistory.Add(new DegreePlanNote(564, "3434343", new DateTime(2013, 12, 20, 10, 20, 0), "This is second note."));
                    noteHistory.Add(new DegreePlanNote(999999, "3434343", new DateTime(2013, 12, 20, 10, 20, 0), "This is third note. It has a large ID."));
                    archive2.Notes = noteHistory;
                }
                else
                {
                    archive2.Notes = plan.Notes;
                }

                archive2.ArchivedCourses = archiveCourses;
                archives.Add(archive2);
            }
            return archives;
        }

        private  async Task<IEnumerable<Ellucian.Colleague.Domain.Planning.Entities.ArchivedCourse>> BuildArchiveCoursesAsync(DegreePlan plan)
        {
            List<ArchivedCourse> archiveCourses = new List<ArchivedCourse>();
            foreach (var term in plan.TermIds)
            {
                var plannedCourses = plan.GetPlannedCourses(term).Where(c => !string.IsNullOrEmpty(c.CourseId));
                foreach (var pc in plannedCourses)
                {
                    ArchivedCourse ac = new ArchivedCourse(pc.CourseId);
                    ac.TermCode = term;
                    ac.Credits = pc.Credits;
                    ac.SectionId = pc.SectionId;
                    ac.ApprovedBy = "0000111";
                    ac.ApprovalStatus = "Approved";
                    ac.ApprovalDate = new DateTime(2013, 5, 1, 8, 0, 0);
                    ac.Name = "Course Name";
                    ac.Title = "Course Title";
                    ac.IsPlanned = true;
                    ac.RegistrationStatus = null;
                    ac.AddedBy = "0000112";
                    ac.AddedOn = new DateTime(2013, 4, 30, 13, 15, 0);

                    // find the academic credit for this course (if any)
                    var credit =  (await new TestAcademicCreditRepository().GetAsync()).Where(cr => cr.TermCode == ac.TermCode && (!(cr.Course == null) && cr.Course.Id == ac.CourseId)).FirstOrDefault();
                    if (credit != null)
                    {
                        ac.Credits = credit.AdjustedCredit;
                        if (credit.HasVerifiedGrade)
                        {
                            // I think this only matches up for degree plan 2, course "56", which has a Withdraw grade
                            ac.HasWithdrawGrade = (await new TestGradeRepository().GetAsync()).Where(g => g.Id == credit.VerifiedGrade.Id).First().IsWithdraw;
                        }
                    }
                    archiveCourses.Add(ac);
                }
            }

            if (plan.Id == 2)
            {
                    // And add an archived course with CEUs not credits to archive 2
                    ArchivedCourse acc = new ArchivedCourse("111");
                    acc.TermCode = "2007/SP";
                    acc.ContinuingEducationUnits = 2m;
                    acc.ApprovedBy = "0000111";
                    acc.ApprovalStatus = "Approved";
                    acc.ApprovalDate = new DateTime(2013, 5, 1, 8, 0, 0);
                    acc.Name = "Ceu Course Name";
                    acc.Title = "Ceu Course Title";
                    acc.IsPlanned = true;
                    acc.RegistrationStatus = null;
                    acc.AddedBy = "0000112";
                    acc.AddedOn = new DateTime(2013, 4, 30, 13, 15, 0);
                    archiveCourses.Add(acc);
            }

            // Add archived academic credit that was not planned to the plans that also have archive courses (to not disturb other tests)
            if (archiveCourses.Count() > 0)
            {
                // Add archived academic credit that was not planned
                var ac1CourseId = "47";
                var ac1Term = (await new TestTermRepository().GetRegistrationTermsAsync()).ElementAt(0);
                var course =  (await new TestCourseRepository().GetAsync()).Where(c => c.Id == ac1CourseId).First();
                var section =  (await new TestSectionRepository().GetRegistrationSectionsAsync(new List<Ellucian.Colleague.Domain.Student.Entities.Term>() { ac1Term })).Where(s => s.CourseId == ac1CourseId).First();
                ArchivedCourse ac1 = new ArchivedCourse(ac1CourseId)
                {
                    TermCode = ac1Term.Code,
                    Credits = 3m,
                    SectionId = section.Id,
                    Name = course.Name + section.Number,
                    Title = course.Title,
                    IsPlanned = false,
                    RegistrationStatus = "New"
                };
                archiveCourses.Add(ac1);
            }
            return archiveCourses;
        }
    }
}
