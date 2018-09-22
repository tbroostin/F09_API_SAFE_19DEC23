using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;



namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestSubjectRepository
    {
        public Subject Get(string code)
        {
            return Get().Where(s => s.Code == code).FirstOrDefault();
        }

        public IEnumerable<Subject> Get()
        {
            List<Subject> Subjects = new List<Subject> {
                                            new Subject(GetGuid(), "ACCT",	"Accounting", true),
                                            new Subject(GetGuid(), "AGBU",	"Agriculture Business", true),
                                            new Subject(GetGuid(), "AGME",	"Agriculture Mechanics", true),
                                            new Subject(GetGuid(), "ANSC",	"Animal Science", true),
                                            new Subject(GetGuid(), "ANTH",	"Anthropology", true),
                                            new Subject(GetGuid(), "AOJU",	"Administration of Justice", true),
                                            new Subject(GetGuid(), "ART",	"Art", true),
                                            new Subject(GetGuid(), "ARTH",	"Art History", true),
                                            new Subject(GetGuid(), "AUTO",	"Automotive Technology", true),
                                            new Subject(GetGuid(), "AVIA",	"Aviation", true),
                                            new Subject(GetGuid(), "BIOC",	"BioChemistry", true),
                                            new Subject(GetGuid(), "BIOL",	"Biology", true),
                                            new Subject(GetGuid(), "BUSN",	"Business Administration", true),
                                            new Subject(GetGuid(), "CHEM",	"Chemistry", true),
                                            new Subject(GetGuid(), "CLAS",	"Classical Studies", true),
                                            new Subject(GetGuid(), "CNED",	"Continuing Education", true),
                                            new Subject(GetGuid(), "COMM",	"Communications", true),
                                            new Subject(GetGuid(), "COMP",	"Computer Science", true),
                                            new Subject(GetGuid(), "CRIM",	"Criminology", true),
                                            new Subject(GetGuid(), "CROP",	"Crop Science", true),
                                            new Subject(GetGuid(), "CULS",	"Culinary Studies", true),
                                            new Subject(GetGuid(), "DANC",	"Dance", true),
                                            new Subject(GetGuid(), "DENT",	"Dental Hygeine", true),
                                            new Subject(GetGuid(), "DRAM",	"Drama", true),
                                            new Subject(GetGuid(), "ECED",	"Early Childhood Education", true),
                                            new Subject(GetGuid(), "ECON",	"Economics", true),
                                            new Subject(GetGuid(), "EDUC",	"Education", true),
                                            new Subject(GetGuid(), "ENGL",	"English", true),
                                            new Subject(GetGuid(), "ENGR",	"Engineering", true),
                                            new Subject(GetGuid(), "FILM",	"Film Studies", true),
                                            new Subject(GetGuid(), "FORE",	"Forestry", true),
                                            new Subject(GetGuid(), "FREN",	"French", true),
                                            new Subject(GetGuid(), "GEOL",	"Geology", true),
                                            new Subject(GetGuid(), "GERM",	"German", true),
                                            new Subject(GetGuid(), "HEBR",	"Hebrew", true),
                                            new Subject(GetGuid(), "HIND",	"Hindu", true),
                                            new Subject(GetGuid(), "HIST",	"History", true),
                                            new Subject(GetGuid(), "HLTH",	"Health", true),
                                            new Subject(GetGuid(), "HU",	"Humanities", true),
                                            new Subject(GetGuid(), "ITAL",	"Italian", true),
                                            new Subject(GetGuid(), "LANG",	"Languages", true),
                                            new Subject(GetGuid(), "LAW" ,	"Legal Studies", true),
                                            new Subject(GetGuid(), "MATH",	"Mathematics", true),
                                            new Subject(GetGuid(), "MDLL",	"Modern Language & Literature", true),
                                            new Subject(GetGuid(), "MEDT",	"Medical Lab Technology", true),
                                            new Subject(GetGuid(), "MGMT",	"Graduate Business Management", true),
                                            new Subject(GetGuid(), "MKTG",	"Marketing", true),
                                            new Subject(GetGuid(), "MORT",	"Mortuary Science", true),
                                            new Subject(GetGuid(), "MUSC",	"Music", true),
                                            new Subject(GetGuid(), "NOSHOW","Do not show in catalog", false),
                                            new Subject(GetGuid(), "NURS",	"Nursing", true),
                                            new Subject(GetGuid(), "PARA",	"Paralegal Technology", true),
                                            new Subject(GetGuid(), "PARK",	"Park Ranger Technology", true),
                                            new Subject(GetGuid(), "PERF",	"Performing Arts", true),
                                            new Subject(GetGuid(), "PHED",	"Physical Education", true),
                                            new Subject(GetGuid(), "PHIL",	"Philosophy", true),
                                            new Subject(GetGuid(), "PHYS",	"Physics", true),
                                            new Subject(GetGuid(), "POLI",	"Political Science", true),
                                            new Subject(GetGuid(), "PSYC",	"Psychology", true),
                                            new Subject(GetGuid(), "R2OR",	"R25 Testing", true),
                                            new Subject(GetGuid(), "REAL",	"Real Estate/Land Appraisal", true),
                                            new Subject(GetGuid(), "RELG",	"Religious Studies", true),
                                            new Subject(GetGuid(), "SOCI",	"Sociology", true),
                                            new Subject(GetGuid(), "SPAN",	"Spanish", true),
                                            new Subject(GetGuid(), "SRCH", "Special Subject for SearchTestOne", true), 
                                            new Subject(GetGuid(), "STAT",	"Statistics", true),
                                            new Subject(GetGuid(), "SZI",	"Sziede Test Subject", true),
                                            new Subject(GetGuid(), "TECH",	"Technology", true),
                                            new Subject(GetGuid(), "TRUK",	"Truck Driving Technology", true),
                                            new Subject(GetGuid(), "WELD",	"Welding", true),
                                            new Subject(GetGuid(), "AAAA", "AAAAaasubject", true),
                                            new Subject(GetGuid(), "XAAAAX", "XAAAAXsubject", true)
                                  };
            return Subjects;
        }

        private string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}