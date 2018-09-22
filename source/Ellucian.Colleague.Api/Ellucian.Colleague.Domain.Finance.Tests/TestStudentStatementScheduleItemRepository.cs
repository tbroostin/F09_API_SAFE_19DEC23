// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestStudentStatementScheduleItemRepository
    {
        private static List<StudentStatementScheduleItem> _studentStatementSchedule;
        public static List<StudentStatementScheduleItem> StudentStatementSchedule(string personId)
        {
            GenerateEntities(personId);
            return _studentStatementSchedule;
        }

        private static void GenerateEntities(string personId)
        {
            var acadCreditRepo = new TestAcademicCreditRepository();
            var secRepo = new TestSectionRepository();
            var sections = new List<Section>()
            {
                new Section(acadCreditRepo.Hist100.SectionId, "1", "01", DateTime.Today.AddDays(-30), 3m, null, "History 100", "IN", 
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") }, new List<string>() { "100" }, "UG", 
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) }),
                new Section(acadCreditRepo.Hist200.SectionId, "2", "01", DateTime.Today.AddDays(-30), 4m, null, "History 200", "IN", 
                    new List<OfferingDepartment>() { new OfferingDepartment("HIST") }, new List<string>() { "200" }, "UG", 
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) }),
                new Section(acadCreditRepo.Biol100.SectionId, "3", "01", DateTime.Today.AddDays(-30), 3m, null, "Biology 100", "IN", 
                    new List<OfferingDepartment>() { new OfferingDepartment("BIOL") }, new List<string>() { "100" }, "UG", 
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) }),
                new Section(acadCreditRepo.Biol200.SectionId, "4", "01", DateTime.Today.AddDays(-30), 4m, null, "Biology 200", "IN", 
                    new List<OfferingDepartment>() { new OfferingDepartment("BIOL") }, new List<string>() { "200" }, "UG", 
                    new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-60)) }),
            };
            _studentStatementSchedule = new List<StudentStatementScheduleItem>()
            {
                new StudentStatementScheduleItem(acadCreditRepo.Hist100, sections[0]),
                new StudentStatementScheduleItem(acadCreditRepo.Hist200, sections[1]),
                new StudentStatementScheduleItem(acadCreditRepo.Biol100, sections[2]),
                new StudentStatementScheduleItem(acadCreditRepo.Biol200, sections[3]),
            };
        }
    }
}
