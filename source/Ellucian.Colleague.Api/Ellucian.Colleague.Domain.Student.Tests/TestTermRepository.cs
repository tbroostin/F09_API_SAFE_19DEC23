using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestTermRepository : ITermRepository
    {

        private Dictionary<string, Term> terms = new Dictionary<string, Term>();

        public async Task<Term> GetAsync(string id)
        {
            return (await GetAsync(new List<string>() { id }.AsEnumerable())).FirstOrDefault();

        }

        public async Task <IEnumerable<Term>> GetAsync()
        {
            if (terms.Count() == 0) { Populate(); }
            return await GetAsync(terms.Keys);
        }

        public async Task<IEnumerable<Term>> GetAsync(bool clearCache)
        {
            if (terms.Count() == 0) { Populate(); }
            return await GetAsync(terms.Keys);
        }

       public Term Get(string id)
        {
            var task = this.GetAsync(id);
            return task.Result;
        }

        public async Task<string> GetAcademicPeriodsGuidAsync(string code)
        {
            var task = this.GetAsync();
            return task.Result.FirstOrDefault(c=>c.Code == code).RecordGuid;
        }


        public IEnumerable<Term> Get()
        {
           var task= this.GetAsync();
           return task.Result;
        }
   

        public async Task<IEnumerable<Term>> GetAsync(IEnumerable<string> ids)
        {
            return await Task.FromResult( DoGetMany(ids.Select(ii => ii)));
        }

        public async Task<IEnumerable<Term>> GetRegistrationTermsAsync()
        {
            var regTerms = new List<Term>();
            regTerms.Add(await GetAsync("2012/FA"));
            regTerms.Add(await GetAsync("2013/SP"));
            return regTerms;
        }

        private IEnumerable<Term> DoGetMany(IEnumerable<string> ids)
        {

            if (terms.Count() == 0) { Populate(); }

            ICollection<Term> results = new List<Term>();

            foreach (var id in ids)
            {
                if (terms.Keys.Contains(id))
                {
                    results.Add(terms[id]);
                }

            }

            return results;

        }

        private void Populate()
        {
            terms = new Dictionary<string, Term>();
            string[,] termdata = {

                                    //ID     REPORTYR  SEQ          DESC                     START_DATE     END_DATE   ACAD_LEVEL
                                    //                                                                                         DEFAULT_ON_PLAN
                                    //                                                                                                FOR_PLANNING
                                    //                                                                                                    REPORTING_TERM
                                    //                                                                                                              SESSIONS
                                    //                                                                                                                       YEARS
                                    //                                                                                                                          CATEGORY
                                    //                                                                                                                                  SESSION
                                    {"2000/FA","2000","1","Fall Term                     ","2000-08-28" ,"2000-12-20", "UG   ", "Y", "Y", "2000/FA", "F;FS",  "E", "term", "FA"}, 
                                    {"2000/S1","2000","2","Summer Term 1                 ","2000-05-22" ,"2000-07-01", "UG   ", "N", "Y", "2000RSU", "",    "", "subterm",  "S1"}, 
                                    {"2000/S2","2000","3","Summer Term 2                 ","2000-07-05" ,"2000-08-16", "UG   ", "N", "Y", "2000RSU", "",    "", "subterm", "S2"},
                                    {"2000CFA","2000","4","Continuing Ed Fall            ","2000-08-21" ,"2000-12-20", "CE   ", "N", "N", "2000RSU", "",    "", "subterm", "FA"},
                                    {"2000CS1","2000","5","Continuing Ed Summer 1        ","2000-05-29" ,"2000-07-07", "CE   ", "N", "N", "2000RSU", "",    "", "subterm", "S1"},
                                    {"2000CS2","2000","6","Continuing Ed Summer 2        ","2000-07-10" ,"2000-08-18", "CE   ", "N", "N", "2000RSU", "",    "", "subterm", "S2"},
                                    {"2000RSU","2000","7","Summer Reporting Term         ","2000-05-22" ,"2000-08-16", "     ", "N", "N", "2000RSU", "",    "", "term",    "SU"},
                                    {"2001/FA","2001","1","Fall Term                     ","2001-08-27" ,"2001-12-19", "UG   ", "Y", "Y", "2001/FA", "",    "", "term",    "FA"},
                                    {"2001/S1","2001","2","Summer Term 1                 ","2001-05-21" ,"2001-07-01", "UG   ", "N", "Y", "2001RSU", "",    "", "subterm", "S1"},
                                    {"2001/S2","2001","3","Summer Term 2                 ","2001-07-03" ,"2001-08-15", "UG   ", "N", "Y", "2001RSU", "",    "", "subterm", "S2"},
                                    {"2001/SP","2001","4","Spring Term                   ","2001-01-22" ,"2001-05-18", "UG   ", "Y", "Y", "2001RSP", "",    "", "subterm", "SP"},
                                    {"2001/WI","2001","5","Wintersession                 ","2000-12-27" ,"2001-01-19", "UG   ", "N", "Y", "2001RSP", "",    "", "subterm", "WI"},
                                    {"2001CFA","2001","6","Continuing Ed Fall            ","2001-08-20" ,"2001-12-19", "CE   ", "N", "N", "2001CFA", "",    "", "term",    "FA"},
                                    {"2001CS1","2001","6","Continuing Ed Summer 1        ","2001-05-28" ,"2001-07-06", "CE   ", "N", "N", "2001CS1", "",    "", "term",    "S1"},
                                    {"2001CS2","2001","6","Continuing Ed Summer 2        ","2001-07-09" ,"2001-08-17", "CE   ", "N", "N", "2001CS2", "",    "", "term",    "S2"},
                                    {"2001CSP","2001","6","Continuing Ed Spring          ","2001-01-22" ,"2001-05-18", "CE   ", "N", "N", "2001CSP", "",    "", "term",    "SP"},
                                    {"2001F1 ","2001","6","Fall 8 Week Term              ","2001-09-04" ,"2001-10-26", "UG   ", "N", "N", "2001F1",  "",    "", "term",    "FA"},
                                    {"2001F2 ","2001","6","Fall 8 Week Term              ","2001-10-29" ,"2001-12-21", "UG   ", "N", "N", "2001F2",  "",    "", "term",    "FA"},
                                    {"2001OE ","2001","6","Open Entry/Open Exit Term     ","2001-07-01" ,"2002-06-30", "UG;CE", "N", "N", "2001OE",  "",    "", "term",    "OE"},
                                    {"2001RSP","2001","6","Spring Reporting Term         ","2000-12-27" ,"2001-05-18", "     ", "N", "N", "2001RSP", "",    "", "term",    "SP"},
                                    {"2001RSU","2001","6","Summer Reporting Term         ","2001-05-21" ,"2001-08-15", "     ", "N", "N", "2001RSU", "",    "", "term",    "SU"},
                                    {"2001YL ","2001","7","Year Long Term                ","2001-07-01" ,"2002-06-30", "UG   ", "N", "N", "2001YL",  "",    "", "year",    "YL"},
                                    {"2002/F1","2002","1","Fall Mini Term 1              ","2002-08-26" ,"2002-10-13", "UG   ", "N", "Y", "2002/FA", "",    "", "subterm", "FA"},
                                    {"2002/F2","2002","2","Fall Mini Term 2              ","2002-10-14" ,"2002-12-18", "UG   ", "N", "Y", "2002/FA", "",    "", "subterm", "FA"},
                                    {"2002/FA","2002","3","Fall Term                     ","2002-08-26" ,"2002-12-18", "UG   ", "Y", "Y", "2002/FA", "",    "", "term",    "FA"},
                                    {"2002/S1","2002","4","Summer Term 1                 ","2002-05-20" ,"2002-07-01", "UG   ", "N", "Y", "2002RSU", "",    "", "subterm", "S1"},
                                    {"2002/S2","2002","5","Summer Term 2                 ","2002-07-02" ,"2002-08-14", "UG   ", "N", "Y", "2002RSU", "",    "", "subterm", "S2"},
                                    {"2002/SP","2002","6","Spring Term                   ","2002-01-21" ,"2002-05-17", "UG   ", "Y", "Y", "2002RSP", "",    "", "subterm", "SP"},
                                    {"2002/WI","2002","6","Wintersession                 ","2001-12-27" ,"2002-01-18", "UG   ", "N", "Y", "2002RSP", "",    "", "subterm", "WI"},
                                    {"2002CFA","2002","6","Continuing Ed Fall            ","2002-08-19" ,"2002-12-18", "CE   ", "N", "N", "2002CFA", "",    "", "term",    "FA"},
                                    {"2002CS1","2002","6","Continuing Ed Summer 1        ","2002-05-27" ,"2002-07-05", "CE   ", "N", "N", "2002CS1", "",    "", "term",    "S1"},
                                    {"2002CS2","2002","6","Continuing Ed Summer 2        ","2002-07-08" ,"2002-08-16", "CE   ", "N", "N", "2002CS2", "",    "", "term",    "S2"},
                                    {"2002CSP","2002","6","Continuing Ed Spring          ","2002-01-21" ,"2002-05-17", "CE   ", "N", "N", "2002CSP", "",    "", "term",    "SP"},
                                    {"2002RSP","2002","6","Spring Reporting Term         ","2001-12-27" ,"2002-05-17", "     ", "N", "N", "2002RSP", "",    "", "term",    "SP"},
                                    {"2002RSU","2002","6","Summer Reporting Term         ","2002-05-20" ,"2002-08-14", "     ", "N", "N", "2002RSU", "",    "", "term",    "SU"},
                                    {"2002SP1","2002","6","Spring 8 Week Term 1          ","2002-01-07" ,"2002-03-01", "UG   ", "N", "N", "2002RSP", "",    "", "subterm", "SP"},
                                    {"2002SP2","2002","6","Spring 8 Week Term 2          ","2002-03-04" ,"2002-05-03", "UG   ", "N", "N", "2002RSP", "",    "", "subterm", "SP"},
                                    {"2003/FA","2003","1","Fall Term                     ","2003-08-25" ,"2003-12-17", "UG   ", "Y", "Y", "2003/FA", "",    "", "term",    "FA"},
                                    {"2003/S1","2003","2","Summer Term 1                 ","2003-05-19" ,"2003-06-30", "UG   ", "N", "Y", "2003RSU", "",    "", "subterm", "S1"},
                                    {"2003/S2","2003","6","Summer Term 2                 ","2003-07-01" ,"2003-08-13", "UG   ", "N", "Y", "2003RSU", "",    "", "subterm", "S2"},
                                    {"2003/SP","2003","6","Spring Term                   ","2003-01-20" ,"2003-05-16", "UG   ", "Y", "Y", "2003RSP", "",    "", "subterm", "SP"},
                                    {"2003/WI","2003","6","Wintersession                 ","2002-12-26" ,"2003-01-17", "UG   ", "N", "Y", "2003RSP", "",    "", "subterm", "WI"},
                                    {"2003CFA","2003","6","Continuing Ed Fall            ","2003-08-18" ,"2003-12-17", "CE   ", "N", "N", "2003CFA", "",    "", "term",    "FA"},
                                    {"2003CS1","2003","6","Continuing Ed Summer 1        ","2003-05-25" ,"2003-07-07", "CE   ", "N", "N", "2003CS1", "",    "", "term",    "S1"},
                                    {"2003CS2","2003","6","Continuing Ed Summer 2        ","2003-07-08" ,"2003-08-18", "CE   ", "N", "N", "2003CS2", "",    "", "term",    "S2"},
                                    {"2003CSP","2003","6","Continuing Ed Spring          ","2003-01-20" ,"2003-05-17", "CE   ", "N", "N", "2003CSP", "",    "", "term",    "SP"},
                                    {"2003RSP","2003","6","Spring Reporting Term         ","2002-12-26" ,"2003-05-16", "     ", "N", "N", "2003RSP", "",    "", "term",    "SP"},
                                    {"2003RSU","2003","6","Summer Reporting Term         ","2003-05-19" ,"2003-08-13", "     ", "N", "N", "2003RSU", "",    "", "term",    "SU"},
                                    {"2004/FA","2004","1","Fall Term 2004                ","2004-08-25" ,"2004-12-17", "UG   ", "Y", "Y", "2004/FA", "",    "", "term",    "FA"},
                                    {"2004/S1","2004","2","Summer Term 1 2004            ","2004-05-19" ,"2004-06-30", "UG   ", "N", "Y", "2004RSU", "",    "", "subterm", "SU"},
                                    {"2004/S2","2004","6","Summer Term 2 2004            ","2004-07-01" ,"2004-08-13", "UG   ", "N", "Y", "2004RSU", "",    "", "subterm", "SU"},
                                    {"2004/SP","2004","6","Spring Term                   ","2004-01-20" ,"2004-05-16", "UG   ", "Y", "Y", "2004RSP", "",    "", "subterm", "SP"},
                                    {"2004/WI","2004","6","Winter Session                ","2003-12-26" ,"2004-01-17", "UG   ", "N", "N", "2004RSP", "",    "", "subterm", "WI"},
                                    {"2004CFA","2004","6","Continuing Ed Fall 2004       ","2004-08-18" ,"2004-12-17", "CE   ", "N", "N", "2004CFA", "",    "", "term",    "FA"},
                                    {"2004CS1","2004","6","Continuing Ed Summer 1 2004   ","2004-05-25" ,"2004-07-07", "CE   ", "N", "N", "2004CS1", "",    "", "term",    "S1"},
                                    {"2004CS2","2004","6","Continuing Ed Summer 2 2004   ","2004-07-08" ,"2004-08-18", "CE   ", "N", "N", "2004CS2", "",    "", "term",    "S2"},
                                    {"2004RSP","2004","6","Spring Reporting Term         ","2003-12-26" ,"2004-05-16", "     ", "N", "N", "2004RSP", "",    "", "term",    "SP"},
                                    {"2004RSU","2004","6","Summer Reporting Term 2004    ","2004-05-19" ,"2004-08-13", "     ", "N", "N", "2004RSU", "",    "", "term",    "SU"},
                                    {"2005/FA","2005","1","2005 Fall Term                ","2005-08-26" ,"2005-12-17", "UG   ", "Y", "Y", "2005RFA", "",    "", "subterm", "FA"},
                                    {"2005/S1","2005","2","2005 Summer 1 Term            ","2005-05-23" ,"2005-07-05", "UG   ", "N", "Y", "2005RSU", "",    "", "subterm", "S1"},
                                    {"2005/S2","2005","6","2005 Summer 2 Term            ","2005-07-06" ,"2005-08-16", "UG   ", "N", "Y", "2005RSU", "",    "", "subterm", "S2"},
                                    {"2005/SP","2005","6","2005 Spring                   ","2005-01-15" ,"2005-05-15", "UG   ", "Y", "Y", "2005RSP", "",    "", "subterm", "SP"},
                                    {"2005RFA","2005","6","2005 Fall Reporting Term      ","2005-08-26" ,"2005-12-16", "     ", "N", "N", "2005RFA", "",    "", "term",    "FA"},
                                    {"2005RSP","2005","6","Spring Reporting Term 2005    ","2004-12-26" ,"2005-05-16", "     ", "N", "N", "2005RSP", "",    "", "term",    "SP"},
                                    {"2005RSU","2005","6","2005 Summer Reporting Term    ","2005-05-23" ,"2005-08-16", "     ", "N", "N", "2005RSU", "",    "", "term",    "SU"},
                                    {"2006/FA","2006","1","2006 Fall Term                ","2006-08-25" ,"2006-12-15", "UG   ", "N", "Y", "2006RFA", "",    "", "subterm", "FA"},
                                    {"2006/S1","2006","2","2006 Summer 1 Term            ","2006-05-26" ,"2006-07-06", "UG   ", "N", "Y", "2006RSU", "",    "", "subterm", "S1"},
                                    {"2006/S2","2006","6","2006 Summer 2 Term            ","2006-07-07" ,"2006-08-17", "UG   ", "N", "Y", "2006RSU", "",    "", "subterm", "S2"},
                                    {"2006/SP","2006","6","2006 Spring Term              ","2006-01-24" ,"2006-05-16", "UG   ", "Y", "Y", "2006RSP", "",    "", "subterm", "SP"},
                                    {"2006/WI","2006","6","2006 Winterim Term            ","2005-12-19" ,"2006-01-30", "UG   ", "N", "Y", "2006RSP", "",    "", "subterm", "WI"},
                                    {"2006RFA","2006","6","2006 Fall Reporting Term      ","2006-08-25" ,"2006-12-15", "     ", "N", "N", "2006RFA", "",    "", "term",    "FA"},
                                    {"2006RSP","2006","6","2006 Spring Reporting Term    ","2005-12-19" ,"2006-05-16", "     ", "N", "N", "2006RSP", "",    "", "term",    "SP"},
                                    {"2006RSU","2006","6","2006 Summer Reporting Term    ","2006-05-26" ,"2006-08-17", "     ", "N", "N", "2006RSU", "",    "", "term",    "SU"},
                                    {"2006SP ","2006","6","Do Not Use                    ","2006-01-15" ,"2006-05-15", "     ", "N", "N", "2006SP",  "",    "", "term",    "SP"},
                                    {"2007/F2","2007","1","2007 Fall Term 2              ","2007-09-04" ,"2007-12-31", "UG   ", "N", "N", "2007RFA", "",    "", "subterm", "F2"},
                                    {"2007/FA","2007","2","2007 Fall Term                ","2007-08-24" ,"2007-12-14", "UG   ", "N", "Y", "2007RFA", "",    "", "subterm", "FA"},
                                    {"2007/S1","2006","6","2007 Summer 1 Term            ","2007-05-24" ,"2007-07-05", "UG   ", "N", "Y", "2007RSU", "",    "", "subterm", "S1"},
                                    {"2007/S2","2006","6","2007 Summer 2 Term            ","2007-07-06" ,"2007-08-16", "UG   ", "N", "Y", "2007RSU", "",    "", "subterm", "S2"},
                                    {"2007/SP","2006","6","2007 Spring Term              ","2007-01-23" ,"2007-05-15", "UG   ", "N", "Y", "2007RSP", "",    "", "subterm", "SP"},
                                    {"2007/WI","2006","6","2007 Winterim Term            ","2006-12-18" ,"2007-01-29", "UG   ", "N", "Y", "2007RSP", "",    "", "subterm", "WI"},
                                    {"2007RFA","2007","6","2007 Fall Reporting Term      ","2007-08-24" ,"2007-12-14", "     ", "N", "N", "2007RFA", "",    "", "term",    "FA"},
                                    {"2007RSP","2006","6","2007 Spring Reporting Term    ","2006-12-18" ,"2007-05-15", "     ", "N", "N", "2007RSP", "",    "", "term",    "SP"},
                                    {"2007RSU","2006","6","2007 Summer Reporting Term    ","2007-05-24" ,"2007-08-16", "     ", "N", "N", "2007RSU", "",    "", "term",    "SU"},
                                    {"2008/FA","2008","1","2008 Fall Term                ","2008-08-25" ,"2008-12-15", "UG   ", "Y", "Y", "2008RFA", "",    "", "subterm", "FA"},
                                    {"2008/S1","2007","2","2008 Summer 1 Term            ","2008-05-21" ,"2008-07-01", "UG   ", "N", "Y", "2008RSU", "",    "", "subterm", "S1"},
                                    {"2008/S2","2007","6","2008 Summer 2 Term            ","2008-07-02" ,"2008-08-12", "UG   ", "N", "Y", "2008RSU", "",    "", "subterm", "S2"},
                                    {"2008/SP","2007","6","2008 Spring Term              ","2008-01-23" ,"2008-05-14", "UG   ", "Y", "Y", "2008RSP", "",    "", "subterm", "SP"},
                                    {"2008/WI","2007","6","2008 Winterim Term            ","2007-12-17" ,"2008-01-29", "UG   ", "N", "Y", "2008RSP", "",    "", "subterm", "WI"},
                                    {"2008RFA","2008","6","2008 Fall Reporting Term      ","2008-08-25" ,"2008-12-15", "     ", "N", "N", "2008RFA", "",    "", "term",    "FA"},
                                    {"2008RSP","2007","6","2008 Spring Reporting Term    ","2007-12-17" ,"2008-05-14", "     ", "N", "N", "2008RSP", "",    "", "term",    "SP"},
                                    {"2008RSU","2007","6","2008 Summer Reporting Term    ","2008-05-21" ,"2008-08-12", "     ", "N", "N", "2008RSU", "",    "", "term",    "SU"},
                                    {"2009/FA","2009","1","2009 Fall Term                ","2009-08-25" ,"2009-12-14", "UG   ", "Y", "Y", "2009RFA", "",    "", "subterm", "FA"},
                                    {"2009/S1","2009","2","2009 Summer 1 Term            ","2009-05-20" ,"2009-07-01", "UG   ", "N", "Y", "2009RSU", "",    "", "subterm", "S1"},
                                    {"2009/S2","2009","6","2009 Summer 2 Term            ","2009-07-02" ,"2009-08-12", "UG   ", "N", "Y", "2009RSU", "",    "", "subterm", "S2"},
                                    {"2009/SP","2009","6","2009 Spring Term              ","2009-01-20" ,"2009-05-11", "UG   ", "Y", "Y", "2009RSP", "",    "", "subterm", "SP"},
                                    {"2009/WI","2009","6","2009 Winterim Term            ","2008-12-22" ,"2009-01-26", "UG   ", "N", "Y", "2009RSP", "",    "", "subterm", "WI"},
                                    {"2009RFA","2009","6","2009 Fall Reporting Term      ","2009-08-25" ,"2009-12-14", "     ", "N", "N", "2009RFA", "",    "", "term",    "FA"},
                                    {"2009RSP","2009","6","2009 Spring Reporting Term    ","2008-12-22" ,"2009-05-11", "     ", "N", "N", "2009RSP", "",    "", "term",    "SP"},
                                    {"2009RSU","2009","6","2009 Summer Reporting Term    ","2009-05-20" ,"2009-08-12", "     ", "N", "N", "2009RSU", "",    "", "term",    "SU"},
                                    {"2010/FA","2010","1","2010 Fall Term                ","2010-08-26" ,"2010-12-15", "UG   ", "Y", "Y", "2010RFA", "",    "", "subterm", "FA"},
                                    {"2010/S1","2010","2","2010 Summer 1 Term            ","2010-05-24" ,"2010-07-05", "UG   ", "N", "Y", "2010RSU", "",    "", "subterm", "S1"},
                                    {"2010/S2","2010","6","2010 Summer 2 Term            ","2010-07-06" ,"2010-08-17", "UG   ", "N", "Y", "2010RSU", "",    "", "subterm", "S2"},
                                    {"2010/SP","2010","6","2010 Spring Term              ","2010-01-18" ,"2010-05-14", "UG   ", "Y", "Y", "2010RSP", "",    "", "subterm", "SP"},
                                    {"2010/WI","2010","6","2010 Winterim Term            ","2009-12-18" ,"2010-01-29", "UG   ", "N", "Y", "2010RSP", "",    "", "subterm", "WI"},
                                    {"2010RFA","2010","6","2010 Fall Reporting Term      ","2010-08-26" ,"2010-12-15", "     ", "N", "N", "2010RFA", "",    "", "term",    "FA"},
                                    {"2010RSP","2010","6","2010 Spring Reporting Term    ","2009-12-18" ,"2010-05-14", "     ", "N", "N", "2010RSP", "",    "", "term",    "SP"},
                                    {"2010RSU","2010","6","2010 Summer Reporting Term    ","2010-05-24" ,"2010-08-17", "     ", "N", "N", "2010RSU", "",    "", "term",    "SU"},
                                    {"2011/FA","2011","1","2011 Fall Term                ","2011-08-24" ,"2011-12-13", "UG   ", "Y", "Y", "2011RFA", "",    "", "subterm", "FA"},
                                    {"2011/S1","2011","4","2011 Summer 1 Term            ","2011-05-18" ,"2011-07-05", "UG   ", "N", "Y", "2011RSU", "",    "", "subterm", "S1"},
                                    {"2011/S2","2011","5","2011 Summer 2 Term            ","2011-07-06" ,"2011-08-16", "UG   ", "N", "Y", "2011RSU", "",    "", "subterm", "S2"},
                                    {"2011/SP","2011","3","2011 Spring Term              ","2011-01-20" ,"2011-05-11", "UG   ", "Y", "Y", "2011RSP", "",    "", "subterm", "SP"},
                                    {"2011/WI","2011","2","2011 Winterim Term            ","2010-12-20" ,"2011-01-31", "UG   ", "N", "Y", "2011RSP", "",    "", "subterm", "WI"},
                                    {"2011RFA","2011","6","2011 Fall Reporting Term      ","2011-08-24" ,"2011-12-13", "     ", "N", "N", "2011RFA", "",    "", "term",    "FA"},
                                    {"2011RSP","2011","6","2011 Spring Reporting Term    ","2010-12-20" ,"2011-05-11", "     ", "N", "N", "2011RSP", "",    "", "term",    "SP"},
                                    {"2011RSU","2011","6","2011 Summer Reporting Term    ","2011-05-18" ,"2011-08-16", "     ", "N", "N", "2011RSU", "",    "", "term",    "SU"},
                                    {"2012/FA","2012","1","2012 Fall Term                ","2012-08-23" ,"2012-12-12", "UG   ", "Y", "Y", "2012RFA", "",    "", "subterm", "FA"},
                                    {"2012/S1","2012","4","2012 Summer 1 Term            ","2012-05-23" ,"2012-07-03", "UG   ", "N", "Y", "2012RSU", "",    "", "subterm", "S1"},
                                    {"2012/S2","2012","5","2012 Summer 2 Term            ","2012-07-05" ,"2012-08-15", "UG   ", "N", "Y", "2012RSU", "",    "", "subterm", "S2"},
                                    {"2012/SP","2012","3","2012 Spring Term              ","2012-01-25" ,"2012-05-15", "UG   ", "Y", "Y", "2012RSP", "",    "", "subterm", "SP"},
                                    {"2012/WI","2012","2","2012 Winterim Term            ","2011-12-19" ,"2012-01-30", "UG   ", "N", "Y", "2012RSP", "",    "", "subterm", "SP"},
                                    {"2012RFA","2012","6","2012 Fall Reporting Term      ","2012-08-23" ,"2012-12-12", "     ", "N", "N", "2012RFA", "",    "", "term",    "FA"},
                                    {"2012RSP","2012","6","2012 Spring Reporting Term    ","2011-12-19" ,"2012-05-15", "     ", "N", "N", "2012RSP", "",    "", "term",    "SP"},
                                    {"2012RSU","2012","6","2012 Summer Reporting Term    ","2012-05-23" ,"2012-08-15", "     ", "N", "N", "2012RSU", "",    "", "term",    "SU"},
                                    {"2013/FA","2013","1","2013 Fall Term                ","2013-08-23" ,"2013-12-12", "UG   ", "Y", "Y", "2013RFA", "",    "", "subterm", "FA"},
                                    {"2013/S1","2013","1","2013 Summer 1 Term            ","2013-05-23" ,"2013-07-03", "UG   ", "N", "Y", "2013RSU", "",    "", "subterm", "S1"},
                                    {"2013/S2","2013","2","2013 Summer 2 Term            ","2013-07-05" ,"2013-08-15", "UG   ", "N", "Y", "2013RSU", "",    "", "subterm", "S2"},
                                    {"2013/SP","2013","3","2013 Spring Term              ","2013-01-23" ,"2013-05-15", "UG   ", "Y", "Y", "2013RSP", "",    "", "subterm", "SP"},
                                    {"2013/WI","2013","4","2013 Winterim Term            ","2013-12-19" ,"2013-01-30", "UG   ", "N", "Y", "2013RSP", "",    "", "subterm", "WI"},
                                    {"2013RFA","2013","6","2013 Fall Reporting Term      ","2013-08-23" ,"2013-12-12", "     ", "N", "N", "2013RFA", "",    "", "term",    "FA"},
                                    {"2013RSP","2013","5","2013 Spring Reporting Term    ","2012-12-19" ,"2013-05-15", "     ", "N", "N", "2013RSP", "",    "", "term",    "SP"},
                                    {"2013RSU","2013","6","2013 Summer Reporting Term    ","2013-05-23" ,"2013-08-15", "     ", "N", "N", "2013RSU", "",    "", "term",    "SU"},
                                    {"2014RFA","2014","1","2014 Fall Reporting Term      ","2014-08-23", "2014-12-12", "     ", "N", "N", "2014RFA", "",    "", "term",    "FA"},
                                    {"2014/FA","2014","2","2014 Fall Term                ","2014-08-23" ,"",           "UG   ", "Y", "Y", "2014RFA", "F;FS", "E", "subterm", "FA"},
                                    {"2014/GR","2014","3","2014 Fall Term Graduate       ","2014-08-24" ,"2014-12-11", "GR   ", "Y", "Y", "2014RFA", "GR",    "", "subterm", "FA"},
                                    {"2014/SP","2014","4","2014 Spring Term              ","2014-01-23", "2014-05-12", "     ", "Y", "Y", "2014/SP", "",    "", "term",    "SP"},
                                    {"2015/SP","2014","2","2015 Spring Term              ","2015-01-23" ,"2015-05-12", "     ", "Y", "Y", "2015/SP", "",    "", "term",    "SP"},
                                    {"2015/FA","2015","1","2015 Fall Term                ","2015-08-23" ,"2015-12-12", "     ", "Y", "Y", "2015/FA", "F",   "", "term",    "FA"},
                                    {"2016/SP","2015","2","2016 Spring Term              ","2016-01-23" ,"2016-05-12", "     ", "Y", "Y", "2016/SP", "",   "E,O", "term",  "SP"},
                                    {"2016/FA","2016","1","2016 Fall Term                ","2016-08-23" ,""          , "     ", "Y", "Y", "2016/FA", "F",  "E;O", "term",  "SP"},
                                    {"2017/SP","2016","2","2017 Fall Term                ","2030-01-23" ,"2030-05-12", "     ", "Y", "Y", "",        "",    "",  "term",  ""},
                                     {"2018/SP","2018","2","2018 spring Term                ","2030-01-23" ,"2030-05-12", "     ", "Y", "Y", "",        "",    "",  "term",  ""}
                                     };
            int termcnt = termdata.Length / 14;

            for (int x = 1; x < termcnt; x++)
            {
                var termId = termdata[x, 0].TrimEnd();
                var reportingYear = Int32.Parse(termdata[x, 1].TrimEnd());
                var sequence = Int32.Parse(termdata[x, 2].TrimEnd());
                var termDesc = termdata[x, 3].TrimEnd();
                var startDate = DateTime.Parse(termdata[x, 4]);
                // If end date is not provided for any term (currently only 2014/FA), default to the current date plus 1 to make sure it's always current--so that tests will not fail.
                var endDate = DateTime.Today.AddDays(1);
                if (termdata[x, 5].Length > 0)
                {
                    endDate = DateTime.Parse(termdata[x, 5]);
                }
                var strAcadLevels = termdata[x, 6].TrimEnd();
                var acadLevels = string.IsNullOrEmpty(strAcadLevels) ? new string[0] : strAcadLevels.Split(';');
                var onPlan = termdata[x, 7].TrimEnd() == "Y";
                var forPlanning = termdata[x, 8].TrimEnd() == "Y";
                var reportingTerm = termdata[x, 9].TrimEnd();
                var strSessCycles = termdata[x, 10].TrimEnd();
                var sessionCycles = string.IsNullOrEmpty(strSessCycles) ? new string[0] : strSessCycles.Split(';');
                var strYearCycles = termdata[x, 11].TrimEnd();
                var yearlyCycles = string.IsNullOrEmpty(strYearCycles) ? new string[0] : strYearCycles.Split(';');
                var category = termdata[x, 12].TrimEnd();
                var sessionId = termdata[x, 13].TrimEnd();
                if (String.IsNullOrEmpty(reportingTerm))
                {
                    reportingTerm = termId;
                }
                // Manipulate start and end date of 2017/SP so that it will always be later than the current date
                // (Needed for a degree plan domain test)
                if (termId == "2017/SP" && DateTime.Today >= startDate)
                {
                    // Add the number of years needed to make the term dates later than today's date
                    var addYears = DateTime.Today.Year - startDate.Year + 1;
                    startDate.AddYears(addYears);
                    endDate.AddYears(addYears);
                }
                Term newterm = new Term(Guid.NewGuid().ToString(), termId, termDesc, startDate, endDate, reportingYear, sequence, onPlan, forPlanning, reportingTerm, true)
                    {
                        FinancialPeriod = (endDate < DateTime.Today.AddDays(-30)) ? PeriodType.Past :
                        (endDate > DateTime.Today.AddDays(30)) ? PeriodType.Future : PeriodType.Current,
                        Category = category,
                        SessionId = sessionId
                    };
                if (newterm.Code == "2012/FA")
                {
                    // Give it some registration dates.
                    newterm.RegistrationDates.Add(new RegistrationDate(null, new DateTime(2012, 10, 1), new DateTime(2012, 10, 5), new DateTime(2013, 1, 1), new DateTime(2013, 1, 15), new DateTime(2013, 2, 1), new DateTime(2013, 2, 15), null, null, null, null));
                } 
                foreach (string level in acadLevels)
                {
                    newterm.AddAcademicLevel(level);
                }
                foreach (string cycle in sessionCycles)
                {
                    newterm.AddSessionCycle(cycle);
                }
                foreach (string yc in yearlyCycles)
                {
                    newterm.AddYearlyCycle(yc);
                }
                terms.Add(termId, newterm);
            }

        }

        public  IEnumerable<AcademicPeriod> GetAcademicPeriods(IEnumerable<Term> term)
        {
            throw new NotImplementedException();
        }
    }
}
