/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Services
{
    public static class StudentChecklistDomainService
    {
        /// <summary>
        /// Build a StudentFinancialAidChecklist with the given Checklist items
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of the student to whom this checklist belongs</param>
        /// <param name="awardYear">Award year to which this Checklist applies</param>
        /// <param name="referenceChecklistItems">Collection of reference ChecklistItem objects</param>
        /// <param name="officeChecklistItems">Checklist items defined on the office/year level</param>
        /// <returns></returns>
        public static StudentFinancialAidChecklist BuildStudentFinancialAidChecklist(string studentId, string awardYear, 
            IEnumerable<ChecklistItem> referenceChecklistItems, IDictionary<string, string> officeChecklistItems)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }

            if (referenceChecklistItems == null || !referenceChecklistItems.Any())
            {
                throw new ArgumentNullException("itemsToAdd");
            }

            StudentFinancialAidChecklist studentChecklist = new StudentFinancialAidChecklist(studentId, awardYear);

            //Assign checklist items only if there is an office/year checklist assignment
            if (officeChecklistItems != null && officeChecklistItems.Any())
            {
                var sortedReferenceItems = referenceChecklistItems.OrderBy(i => i.ChecklistSortNumber);
                string controlStatus;
                foreach (var refItem in sortedReferenceItems)
                {
                    if (officeChecklistItems.ContainsKey(refItem.ChecklistItemCode))
                    {                        
                        officeChecklistItems.TryGetValue(refItem.ChecklistItemCode, out controlStatus);
                        studentChecklist.ChecklistItems.Add(new StudentChecklistItem(refItem.ChecklistItemCode, controlStatus));
                    }                    
                }
            }            
            return studentChecklist;
        }        
    }
}
