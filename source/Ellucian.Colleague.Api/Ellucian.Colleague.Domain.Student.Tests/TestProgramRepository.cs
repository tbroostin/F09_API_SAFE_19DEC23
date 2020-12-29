using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestProgramRepository : IProgramRepository
    {
        private ICollection<Program> Programs = new List<Program>();
        private IDictionary<string, Program> programs = new Dictionary<string, Program>();

        public async Task<Program> GetAsync(string id)
        {
            Populate();

            if (programs.Keys.Contains(id))
            {
                //return programs[id];
                return await Task.FromResult<Program>(programs[id]);
            }
            else
            {
                // this will catch any variations.  asking for "STSS.MATH.BS.2008.*Sksjak" will get you "MATH.BS"
                // The reason for this is that the TestProgramRequirementsRepository uses descriptive program names to get
                // variations for tests.  

                foreach (var prog in programs.Keys)
                {
                    if (id.Contains(prog))
                    {
                        //return programs[prog];
                        return await Task.FromResult<Program>(programs[prog]);
                    }
                }
            }
            throw new IndexOutOfRangeException("Program " + id + " not found in test repository.");
        }
        
        public async Task<IEnumerable<Program>> GetAsync()
        {
            Populate();
            return await Task.FromResult<IEnumerable<Program>>(programs.Values);
        }

        //TODO: Remove once degree plan is converted to Async
        public Program Get(string id)
        {
            Populate();

            if (programs.Keys.Contains(id))
            {
                return programs[id];
            }
            else
            {
                // this will catch any variations.  asking for "STSS.MATH.BS.2008.*Sksjak" will get you "MATH.BS"
                // The reason for this is that the TestProgramRequirementsRepository uses descriptive program names to get
                // variations for tests.  

                foreach (var prog in programs.Keys)
                {
                    if (id.Contains(prog))
                    {
                        return programs[prog];
                    }
                }
            }
            throw new IndexOutOfRangeException("Program " + id + " not found in test repository.");
        }

        //TODO: Remove once degree plan is converted to Async
        public IEnumerable<Program> Get()
        {
            Populate();
            return programs.Values;
        }
        
        // TODO: implement this.
        public ProgramRequirements GetRequirements(string id, string catalog)
        {
            // note from MBS.  Why?  This data is in the (I)(Test)ProgramRequirementsRepository.
            throw new NotImplementedException();
        }

        private void Populate()
        {
            string acadLevelCode = "UG";
            string gradAcadLevel = "GR";
            CreditFilter cf = new CreditFilter() { AcademicLevels = new List<string>() { acadLevelCode } };
            CreditFilter gcf = new CreditFilter() { AcademicLevels = new List<string>() { gradAcadLevel } };
            CreditFilter scf = new CreditFilter() { Subjects = new List<string>() { "MATH" } };
            CreditFilter additionalCF = new CreditFilter() { AdditionalSelectCriteria = "WITH STC.COURSE.LEVEL EQ '100'" };
            var catalogs = new List<string>() { "2012", "2013", "2014", "2015" };
            programs = new Dictionary<string, Program>();
            programs.Add("AA.UNDC", new Program("AA.UNDC", "Undecided AA", new List<string> { "UNDC" }, true, acadLevelCode, cf, false, "This is for Undecided Associates") { Catalogs = catalogs });
            programs.Add("ANTH.BA", new Program("ANTH.BA", "Anthropology BA", new List<string> { "UNDC" }, true, acadLevelCode, cf, true, "Bachelor of Arts in Anthropology") { Catalogs = catalogs });
            programs.Add("ART.BA", new Program("ART.BA", "Art BA", new List<string> { "ART" }, true, acadLevelCode, cf, true, "Bachelor of Arts in Art") { Catalogs = catalogs });
            programs.Add("ENGL.BA", new Program("ENGL.BA", "English BA", new List<string> { "ENGL" }, true, acadLevelCode, cf, true, "Bachelor of Arts in English") { Catalogs = catalogs });
            programs.Add("HIST.BA", new Program("HIST.BA", "History BA", new List<string> { "HIST" }, true, acadLevelCode, cf, true, "Bachelor of Arts in History") { Catalogs = catalogs });
            programs.Add("ECON.BA", new Program("ECON.BA", "Economics BA", new List<string> { "ECON" }, true, acadLevelCode, cf, true, "Bachelor of Arts in Economics") { Catalogs = catalogs });
            programs.Add("DANC.BA", new Program("DANC.BA", "Dance BA", new List<string> { "PERF", "PHED" }, true, acadLevelCode, cf, true) { Catalogs = catalogs });  // Two depts, Description intentionally blank.

            programs.Add("MATH.BS", new Program("MATH.BS", "Math BS", new List<string> { "MATH" }, true, acadLevelCode, cf, true, "Bachelor of Science in Math") { Catalogs = catalogs, RelatedPrograms = new List<string>() { "ENGR.BS", "PHYS.BS" } });
            programs.Add("GEOL.BS", new Program("GEOL.BS", "Geology BS", new List<string> { "GEOL" }, true, acadLevelCode, cf, true, "Bachelor of Science in Geology") { Catalogs = catalogs });
            programs.Add("ENGR.BS", new Program("ENGR.BS", "Engineering BS", new List<string> { "ENGR" }, true, acadLevelCode, cf, true, "Bachelor of Science in Engineering") { Catalogs = catalogs });
            programs.Add("BIOL.BS", new Program("BIOL.BS", "Biology BS", new List<string> { "BIOL", "BIOC" }, true, acadLevelCode, cf, true, "Bachelor of Science in Biology") { Catalogs = catalogs }); // Two depts
            programs.Add("CHEM.BS", new Program("CHEM.BS", "Chemistry BS", new List<string> { "CHEM" }, true, acadLevelCode, cf, true, "Bachelor of Science in Chemistry") { Catalogs = catalogs });
            programs.Add("PHYS.BS", new Program("PHYS.BS", "Physics BS", new List<string> { "PHYS" }, true, acadLevelCode, cf, true, "Bachelor of Science in Physics") { Catalogs = catalogs });

            programs.Add("HIST.MA", new Program("HIST.MA", "History MA", new List<string> { "HIST" }, true, gradAcadLevel, cf, true, "Master of Science in History") { Catalogs = catalogs });
            programs.Add("ECON.MA", new Program("ECON.MA", "Economics MA", new List<string> { "ECON" }, true, gradAcadLevel, cf, true, "Master of Science in Economics") { Catalogs = catalogs });
            programs.Add("DANC.MA", new Program("DANC.MA,", "Dance MA", new List<string> { "PERF", "PHED" }, true, gradAcadLevel, cf, true, "Master of Science in Dance") { Catalogs = catalogs }); // Two depts

            programs.Add("MATH.MS", new Program("MATH.MS", "Math MS", new List<string> { "MATH" }, true, gradAcadLevel, gcf, true, "Master of Science in Math") { Catalogs = catalogs });
            programs.Add("ENGR.MS", new Program("ENGR.MS", "Engineering MS", new List<string> { "ENGR" }, true, gradAcadLevel, cf, true, "Master of Science in Engineering") { Catalogs = catalogs });
            programs.Add("CHEM.MS", new Program("CHEM.MS", "Chemistry MS", new List<string> { "CHEM" }, true, gradAcadLevel, cf, true, "Master of Science in Chemistry") { Catalogs = catalogs });


            programs.Add("DANC.CERT", new Program("DANC.CERT", "Dance Cert", new List<string> { "PERF", "PHED" }, true, acadLevelCode, cf, false, "Certificate in Dance")); // two depts
            programs.Add("ENGL.CERT", new Program("ENGL.CERT", "English Cert", new List<string> { "ENGL" }, true, acadLevelCode, cf, true, "Certificate in English"));

            programs.Add("DANC.MINOR", new Program("DANC.MINOR", "Dance Minor", new List<string> { "PERF", "PHED" }, true, acadLevelCode, cf, true, "Dance Minor")); // two depts
            programs.Add("ENGL.MINOR", new Program("ENGL.MINOR", "English Minor", new List<string> { "ENGL" }, true, acadLevelCode, cf, true, "English Minor"));

            programs.Add("DANC.Ccd", new Program("DANC.Ccd", "Dance Ccd", new List<string> { "PERF", "PHED" }, true, acadLevelCode, cf, false, "Dance Ccd"));
            programs.Add("ENGL.Ccd", new Program("ENGL.Ccd", "English Ccd", new List<string> { "ENGL" }, true, acadLevelCode, cf, true, "English Ccd"));
            programs.Add("EXP.Ccd", new Program("EXP.Ccd", "Expired Program Ccd", new List<string> { "ENGL" }, false, acadLevelCode, cf, false, "Expired Ccd"));
            programs.Add("ALMOSTEXP.Ccd", new Program("ALMOSTEXP.Ccd", "Almost Expired Program Ccd", new List<string> { "ENGL" }, false, acadLevelCode, cf, true, "Almost Expired Ccd"));
            programs.Add("SIMPLE", new Program("SIMPLE", "Simple Program", new List<string> { "ENGL" }, true, acadLevelCode, cf, false, "Master of Science") { Catalogs = catalogs });
            programs.Add("REPEAT.BB", new Program("REPEAT.BB", "To test repeates and retakes", new List<string> { "MATH" }, true, gradAcadLevel, gcf, false, "Master of Math") { Catalogs = catalogs });
            programs.Add("TRANSCRIPT.GROUPING.FILTER", new Program("TRANSCRIPT.GROUPING.FILTER", "To test transcript grouping filters", new List<string> { "MATH" }, true, gradAcadLevel, cf, false, "Master of Math") { Catalogs = catalogs });
            programs.Add("TRANSCRIPT.GROUPING.FILTER.SUBJECTS", new Program("TRANSCRIPT.GROUPING.FILTER.SUBJECTS", "To test transcript grouping filters for subjects", new List<string> { "MATH" }, true, gradAcadLevel, scf, false, "Master of Math") { Catalogs = catalogs });
            programs.Add("TRANSCRIPT.GROUPING.FILTER.ADDL.SELECT", new Program("TRANSCRIPT.GROUPING.FILTER.ADDL.SELECT", "To test transcript grouping filters for additional criteria", new List<string> { "MATH" }, true, gradAcadLevel, additionalCF, false, "Master of Math") { Catalogs = catalogs });
            programs.Add("PROG.IN.LIST.SORT.ORDER.BB", new Program("PROG.SORT.ORDER.BB", "To test IN.LIST.ORDER syntax added to group", new List<string> { "LANG" }, true, acadLevelCode, cf, false, "Language and Arts") { Catalogs = catalogs });




            // Set flag to make all programs available for selection with one exception, for testing
            foreach (var pgm in programs)
            {
                pgm.Value.IsSelectable = true;
                if (pgm.Value.Code == "SIMPLE") { pgm.Value.IsSelectable = false; }
            }
        }

        public Task<StudentProgram> GetAsync(string id, string programCode)
        {
            throw new NotImplementedException();
        }
    }
}
