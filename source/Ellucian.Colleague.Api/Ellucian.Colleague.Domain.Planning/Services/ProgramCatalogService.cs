// Copyright 2013-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using slf4net;

namespace Ellucian.Colleague.Domain.Planning.Services
{
    public class ProgramCatalogService
    {
        public static string DeriveDefaultCatalog(Program program, IEnumerable<StudentProgram> studentPrograms, ICollection<Catalog> allCatalogs, CatalogPolicy catalogPolicy, ILogger logger = null)
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
                if (logger != null)
                {
                    logger.Debug(string.Format("Student {0}: Load Sample Course Plan - Catalog Policy = Student Program Catalog Year; identifying catalog year to use from student's academic programs...",
                        studentPrograms.First().StudentId));
                }
                // If the student has just one active program (the norm) just use that catalog year.
                if (studentPrograms.Count() == 1)
                {
                    if (logger != null)
                    {
                        logger.Debug(string.Format("Student {0}: only one active student program ({1}) - catalog year from student program will be used: {2}",
                            studentPrograms.First().StudentId,
                            studentPrograms.First().ProgramCode + "*" + studentPrograms.First().CatalogCode),
                            studentPrograms.First().CatalogCode);
                    }

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
                    if (logger != null)
                    {
                        logger.Debug(string.Format("Student {0}: multiple active student programs ({1}) - earliest catalog year from these program will be used: {2}",
                            studentPrograms.ElementAt(0).StudentId,
                            string.Join(",", studentPrograms.Select(sp => sp.ProgramCode + "*" + sp.CatalogCode)),
                            studentEarliestCatalog.Code));
                    }

                    return studentEarliestCatalog.Code;
                }

                // If none of the student's catalogs have a start date less than today just take first catalog year. 
                if (logger != null)
                {
                    logger.Debug(string.Format("Student {0}: no active student programs - catalog year from first student program ({1}) will be used: {2}",
                        studentPrograms.ElementAt(0).StudentId),
                        studentPrograms.ElementAt(0).ProgramCode + "*" + studentPrograms.ElementAt(0).CatalogCode,
                        studentPrograms.ElementAt(0).CatalogCode);
                }
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
                    if (logger != null)
                    {
                        logger.Debug(string.Format("Student {0}: Load Sample Course Plan - Catalog Policy = Current Catalog Year; after reviewing catalog years for program {1}, no current catalog code was found. Catalog year from first student program ({2}) will be used: {3}",
                            studentPrograms.ElementAt(0).StudentId,
                            program.Code,
                            studentPrograms.ElementAt(0).ProgramCode + "*" + studentPrograms.ElementAt(0).CatalogCode,
                            studentPrograms.ElementAt(0).CatalogCode));
                    }
                }
                else
                {
                    if (logger != null)
                    {
                        logger.Debug(string.Format("Student {0}: Load Sample Course Plan - Catalog Policy = Current Catalog Year; after reviewing catalog years for program {1}, catalog {2} was found to be the current catalog code.",
                            studentPrograms.ElementAt(0).StudentId,
                            studentPrograms.ElementAt(0).ProgramCode,
                            defaultCatalog));
                    }
                }
                if (logger != null)
                {
                    logger.Debug(string.Format("Student {0}: Load Sample Course Plan - catalog year used: {1}",
                        studentPrograms.First().StudentId,
                        defaultCatalog));
                }
                return defaultCatalog;
            }
        }
    }
}
