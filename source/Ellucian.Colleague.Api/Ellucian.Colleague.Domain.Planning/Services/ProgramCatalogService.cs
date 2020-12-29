using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Planning.Services
{
    public class ProgramCatalogService
    {
        public static string DeriveDefaultCatalog(Program program, IEnumerable<StudentProgram> studentPrograms, ICollection<Catalog> allCatalogs, CatalogPolicy catalogPolicy)
        {
            if (program == null)
            {
                throw new ArgumentNullException("program");
            }
            if (studentPrograms == null || studentPrograms.Count() == 0)
            {
                throw new ArgumentNullException("studentPrograms");
            }
            if (allCatalogs == null || allCatalogs.Count() == 0)
            {
                throw new ArgumentNullException("allCatalogs");
            }
            
            // Using the DefaultCatalogPolicy parameter from Colleague determine whether to use the catalog year
            // from the student's active programs, or the most recent catalog year in the program
            if (catalogPolicy == CatalogPolicy.StudentCatalogYear)
            {
                // If the student has just one active program (the norm) just use that catalog year.
                if (studentPrograms.Count() == 1)
                {
                    return studentPrograms.ElementAt(0).CatalogCode;
                }
                // Otherwise determine the earliest catalog from the student's multiple programs.
                IEnumerable<string> studentCatalogCodes = studentPrograms.Select(sp => sp.CatalogCode);
                List<Catalog> studentCatalogs = new List<Catalog>();
                foreach (var catalogCode in studentCatalogCodes)
                {
                    Catalog cat = allCatalogs.Where(c => c.Code == catalogCode).FirstOrDefault();
                    if (cat != null)
                    {
                        studentCatalogs.Add(cat);
                    }
                }
                Catalog studentEarliestCatalog = studentCatalogs.Where(c => c.StartDate <= DateTime.Now).OrderBy(s => s.StartDate).FirstOrDefault();
                if (studentEarliestCatalog != null)
                {
                    return studentEarliestCatalog.Code;
                }

                // If none of the student's catalogs have a start date less than today just take first catalog year. 
                return studentPrograms.ElementAt(0).CatalogCode;

            }
            else
            {
                // Get the most recent (not future) catalog year from the program 
                string defaultCatalog = null;
                defaultCatalog = program.GetCurrentCatalogCode(allCatalogs);
                if (string.IsNullOrEmpty(defaultCatalog))
                {
                    // if we don't have a year from the program, just use student's first program's catalog year.
                    defaultCatalog = studentPrograms.ElementAt(0).CatalogCode;
                }
                return defaultCatalog;
            }
        }
    }
}
