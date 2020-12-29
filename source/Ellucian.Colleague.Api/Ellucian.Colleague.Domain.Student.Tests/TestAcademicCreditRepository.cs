// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestAcademicCreditRepository : IAcademicCreditRepository
    {
        public AcademicCredit Hist100 { get { return GetAsync("1").Result; } }
        public AcademicCredit Hist200 { get { return GetAsync("2").Result; } }
        public AcademicCredit Biol100 { get { return GetAsync("3").Result; } }
        public AcademicCredit Biol200 { get { return GetAsync("4").Result; } }
        public AcademicCredit Biol400 { get { return GetAsync("7").Result; } }
        public AcademicCredit Hist400 { get { return GetAsync("10").Result; } }
        public AcademicCredit Math400 { get { return GetAsync("17").Result; } }
        public AcademicCredit Span300 { get { return GetAsync("20").Result; } }

        private IDictionary<string, AcademicCredit> acadcreds = new Dictionary<string, AcademicCredit>();
        private IDictionary<string, AcademicCredit> acadcredsbyid = new Dictionary<string, AcademicCredit>();

        private string[,] credArray = {
    //acad  cred   course                                         gpa                  ----grade----   course   sec           cmpl  midterm grades         Grading  Sec   Ad   Ad gpa
    //      id     name         title                    status  cred   type  term     let  val  pts       id     id    CEUs   cred  MT1 MT2 MT3 MT4 MT5 MT6  Type  No    Cred Cred
        {"1","HIST-100","World History to WWII",        "N","3.00", "I","2009/SP", "A", "4", "12",   "139", "8001","0.00","3.00","A","B","C","A","B","C","G",   "001","",  ""},
        {"2","HIST-200","Intermediate History",         "N","3.00", "I","2009/SP", "B", "3",  "9",    "42", "8002","0.00","3.00","B","C","A","B","C","", "G",   "001","",  ""},
        {"3","BIOL-100","Molecular Biology",            "A","3.00", "C","2009/SP", "B", "3",  "9",   "110", "8003","0.00","3.00","C","A","B","A","", "", "G",   "001","",  ""}, // Status "Add" Type "Cont Ed"
        {"4","BIOL-200","Intermediate Biology",         "A","3.00",  "","2009/SP", "B", "3",  "9",    "21", "8004","0.00","3.00","A","B","C","" ,"" ,"", "G",   "001","",  ""}, // null type (status added 8/2/12)
        {"5","MATH-201","Linear Algebra",               "N","3.00", "I","2009/SP", "A", "4", "12",    "47", "8005","0.00","3.00","B","C","" ," "," ","", "G",   "001","",  ""},
        {"6","MATH-152","Calculus II",                  "N","3.00", "I","2009/SP", "A", "4", "12",   "333", "8006","0.00","3.00","C","" ,"" , "","" ,"", "G",   "001","",  ""},
        {"7","BIOL-400","Advanced Topics in Biology",   "N","4.00", "I","2009/FA", "B", "3", "12",    "64", "8007","0.00","4.00","A","", "A","A","A","A","G",   "002","",  ""}, // MISSING MIDTERM 2
        {"8","MATH-100","Discrete Mathematics",         "N","3.00", "I","2009/FA", "B", "3",  "9",   "143", "8008","0.00","3.00", "", "", "", "", "", "","G",   "002","",  ""},
        {"9","MATH-350","History of Mathmatics",        "N","3.00", "I","2009/FA", "A", "4", "12",   "186", "8009","0.00","3.00", "", "", "", "", "", "","G",   "002","",  ""},
        {"10","HIST-400","American History From WWI",   "N","4.00", "I","2009/FA", "B", "3", "12",    "87", "8010","0.00","4.00", "", "", "", "", "", "","G",   "002","",  ""},
        {"11","MATH-150","Pre-Calculus",                "N","3.00", "I","2009/FA", "B", "3",  "9",   "180", "8011","0.00","3.00", "", "", "", "", "", "","G",   "002","",  ""},
        {"12","COMP-100","Introduction to Composition", "N","3.00", "I","2010/SP", "A", "4", "12",   "117", "8012","0.00","3.00", "", "", "", "", "", "","G",   "003","",  ""},
        {"13","MUSC-100","History of Music",           "TR","4.00","TR","2010/SP", "B", "3", "12",   "148", "8013","0.00","4.00", "", "", "", "", "", "","G",   "003","",  ""}, // TRANSFER
        {"14","MUSC-207","Instrumental Ensemble",      "TR","1.00","TR","2010/SP", "B", "3",  "3",   "355", "8014","0.00","1.00", "", "", "", "", "", "","G",   "003","",  ""}, // TRANSFER
        {"15","SPAN-100","Beginning Spanish",           "N","3.00", "I","2010/SP", "B", "3",  "9",   "160", "8015","0.00","3.00", "", "", "", "", "", "","G",   "003","",  ""},
        {"16","MATH-362","Modern Algebra",              "N","3.00", "I","2010/SP", "A", "4", "12",   "342", "8016","0.00","3.00", "", "", "", "", "", "","G",   "003","",  ""},
        {"17","MATH-400","Axiomatic Systems",           "U","4.00", "I","2010/SP", "A", "4", "16",    "91", "8017","0.00","4.00", "", "", "", "", "", "","G",   "003","",  ""}, // Credit Status Unknown
        {"18","MATH-102","Geometry",                    "W","3.00", "I","2010/SP", "B", "3",  "9",   "306", "8018","0.00","3.00", "", "", "", "", "", "","G",   "003","",  ""}, // Credit Status Withdrawn
        {"19","MATH-103","Trigonometry",                "C","3.00", "I","2010/SP", "B", "3",  "9",   "315", "8019","0.00","3.00", "", "", "", "", "", "","G",   "003","",  ""}, // Credit Status Cancelled
        {"20","SPAN-300","Advanced Spanish",            "N","0.00", "I","2010/FA","AU", "0",  "0",    "84", "8020","0.00","0.00", "", "", "", "", "", "","A",   "004","",  ""}, // Audit Course
        {"21","DANC-100","Ballroom Dancing",            "N","3.00", "I","2010/FA", "A", "4", "12",   "122", "8021","0.00","3.00", "", "", "", "", "", "","G",   "004","",  ""},
        {"22","DANC-200","Dance Theory",                "N","1.00", "I","2010/FA", "P", "0",  "0",    "28", "8022","0.00","1.00", "", "", "", "", "", "","P",   "004","",  ""}, // Pass - no GPA affect
        {"23","SOCI-100","Introduction to Sociology",   "N","3.00", "I","2010/FA", "B", "3",  "9",   "159", "8023","0.00","3.00", "", "", "", "", "", "","G",   "004","",  ""},
        {"24","POLI-100","Intro to Political Science",  "N","3.00", "I","2010/FA", "C", "2",  "6",   "155", "8024","0.00","3.00", "", "", "", "", "", "","G",   "004","",  ""},
        {"25","PSYC-100","Introduction to Psychology",  "N","3.00", "I","2010/FA", "B", "3",  "9",   "156", "8025","0.00","3.00", "", "", "", "", "", "","G",   "004","",  ""},
        {"26","ENGL-102","College Writing II",          "N","3.00", "I","2011/SP", "B", "3",  "9",   "187", "8026","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""},
        {"27","PHYS-100","Introduction to Physics",     "N","3.00", "I","2011/SP", "B", "3",  "9",   "154", "8027","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""},
        {"28","MATH-151","Calculus I",                  "N","3.00", "I","2011/SP", "A", "4", "12",   "324", "8028","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""},
        {"29","MATH-228","Probability and Statistics",  "N","3.00", "I","2011/SP", "B", "3",  "9",   "185", "8029","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""},
        {"30","HIST-415","Major Seminar II",            "N","3.00", "I","2011/SP", "A", "4", "12",   "289", "8030","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""},
        {"31","MATH-372","Differential Equations",      "N","3.00", "I","2011/SP", "A", "4", "12",   "363", "8031","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""},
        {"32","MATH-499","Senior Seminar",              "N","4.00", "I","2011/SP", "A", "4", "16",   "364", "8032","0.00","4.00", "", "", "", "", "", "","G",   "005","",  ""},
        {"33","MATH-460","Intro to Real Analysis",      "N","4.00", "I","2011/S1", "A", "4", "16",   "213", "8033","0.00","4.00", "", "", "", "", "", "","G",   "006","",  ""},
        {"34","MATH-371","Differential Equations",      "N","3.00", "I","2011/S1", "A", "4", "12",   "353", "8034","0.00","3.00", "", "", "", "", "", "","G",   "006","",  ""},
        {"35","MATH-491","Math Seminar",                "N","4.00", "I","2011/S1", "A", "4", "16",   "226",     "","0.00","4.00", "", "", "", "", "", "","G",   "","",""}, // STAC-created credit - no section id
        {"36","ENGL-101","College Writing I",           "N","3.00", "I","2011/FA",  "", "0",  "0",   "130", "8036","0.00","0.00", "", "", "", "", "", "","G",   "007","",  ""}, // Ungraded - in progress (has a grade scheme so it will eventually be graded)
        {"37","PHYS-400","The Theory of Everything",    "N","4.00", "I","2011/FA",  "", "0",  "0",    "93", "8037","0.00","0.00", "", "", "", "", "", "","G",   "007","",  ""}, // Ungraded - in progress (has a grade scheme so it will eventually be graded)
        {"38","MATH-300","Advanced Mathematics",        "N","3.00", "I","2011/FA",  "", "0",  "0",    "78", "8038","0.00","0.00", "", "", "", "", "", "","G",   "007","",  ""}, // Ungraded - but no grade scheme so sort order 2 not 4
        {"39","MATH-200","Intermediate Mathematics",    "N","3.00", "I",       "", "B", "3",  "9",    "46", "8039","0.00","3.00", "", "", "", "", "", "","G",   "008","",  ""}, // Null Term
        {"40","ART-200","Intermediate Art",             "N","3.00", "I",       "", "C", "2",  "6",    "20", "8040","0.00","3.00", "", "", "", "", "", "","G",   "008","",  ""}, // Null Term
        {"41","ACCT-101","Accounting for Business I",   "N","0.00", "C","2009/FA", "B", "0",  "0", "10000", "8041","2.00","0.00", "", "", "", "", "", "","G",   "002","",  ""}, // CE course with CEUs
        {"42","ACCT-102","Accounting for Business II",  "N","0.00", "C","2009/FA", "B", "0",  "0", "10001", "8042","2.00","0.00", "", "", "", "", "", "","G",   "002","",  ""}, // CE course with CEUs
        {"43","ACCT-103","Accounting for Business III", "N","0.00", "C","2009/FA", "A", "0",  "0", "10002", "8043","2.00","0.00", "", "", "", "", "", "","G",   "002","",  ""}, // CE course with CEUs        
        {"44","HIST-198","History of Methodologies",    "D","3.00", "I","2011/SP", "W", "0",  "0", "10003", "8098","0.00","0.00", "", "", "", "", "", "","G",   "005","",  ""}, // Withdrawn
        {"45","HIST-199","Dead Cultures",               "N","3.00", "I","2011/SP", "I", "0",  "0", "10004", "8099","0.00","0.00", "", "", "", "", "", "","G",   "005","",  ""}, // Incomplete
        // Data matching colldev EXCEPT SEC.ID
        {"46","ENGL-1000","Freshman Comp",              "N","3.00", "I","2011/SP", "B", "3",  "9",  "2414", "8100","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // 
        {"47","ENGL-1001","Freshman Grammar ",          "N","3.00", "I","2011/SP", "B", "3",  "9",  "2415", "8101","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // 
        //{"48","MATH-1000","MATH*101",	    		    "N","1.00", "I","2011/SP", "D", "2",  "2", "MATH*101", "8102","0.00","1.00",""}, // non-num course id was breaking a lot
        {"48","MATH-1000","MATH*101",                   "N","1.00", "I","2011/SP", "D", "2",  "2", "10999", "8102","0.00","1.00", "", "", "", "", "", "","G",   "005","",  ""}, // 
        {"49","MATH-1001","Freshman Calc Second Semer", "N","5.00", "I","2011/SP", "A", "4", "20",  "2416", "8103","0.00","5.00", "", "", "", "", "", "","G",   "005","",  ""}, // 
        {"50","HU-1000"  ,"Caring for One's Fellow Man","N","3.00", "I","2011/SP", "F", "0",  "0",  "2420", "8104","0.00","0.00", "", "", "", "", "", "","G",   "005","",  ""}, //  Failed - zero Completed cred
        {"51","COMM-2000","Talking to Folks",           "N","6.00", "I","2011/SP", "B", "3", "18",  "2422", "8105","0.00","6.00", "", "", "", "", "", "","G",   "005","",  ""}, // 
        {"52","MATH-4000","Really Hard Math 4000",      "N","3.00", "I","2011/SP", "A", "4", "12",  "2417", "8106","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // 
        {"53","MATH-4001","Excruciatingly Hard Math",   "N","3.00", "I","2011/SP", "A", "4", "12",  "2418", "8107","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // 
        {"54","MATH-4002","Ridiculously Hard Math",     "N","4.00", "I","2011/SP", "A", "4", "12",  "2419", "8108","0.00","4.00", "", "", "", "", "", "","G",   "005","",  ""}, // 
        {"55","MATH-4003","Really Hard Math 4003",      "X","3.00", "I","2011/SP",  "", "0",  "0",  "2450", "8109","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // Deleted credit
        {"56","MATH-4003","Really Hard Math 4003",      "W","3.00", "I","2011/SP", "W", "0",  "0",  "2450", "8109","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // Withdrawn credit with grade
        {"57","MATH-502","Advanced Mathematical Theory","N","3.00", "I","2011/SP", "B", "3",  "9",  "9878", "8110","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // Graduate course (course acad lev)
         // Noncourses and other oddly-shaped "credits" are added below and listed here for clarity
         // "1001"                                                                                            // Noncourse
         // "1002"                                                                                            // Noncourse
        // Test for academic credit sorting (ProgramEvaluationTests>
        {"58","MATH-201","Linear Algebra",              "N","3.00", "I","2009/SP", "A", "4", "12",    "47", "8111","0.00","3.00","B","C", ""," "," ", "","G",   "001","",  ""}, // transfer
        {"59","SPAN-100","Beginning Spanish",          "TR","3.00","TR","2009/SP", "B", "3", "9",    "160", "8112","0.00","3.00", "", "", "", "", "", "","G",   "003","",  ""}, // Transfer
        {"60","MUSC-207","Instrumental Ensemble",      "TR","3.00","TR","2009/SP", "B", "3",  "3",   "355", "8113","0.00","1.00", "", "", "", "", "", "","G",   "003","",  ""}, // transfer
        {"61","HU-1000","Caring about your fellow man", "N","1.00", "I",       "",  "",  "",  "0",  "2420", "8114","0.00","1.00", "", "", "", "", "", "","G",   "001","",  ""}, // Institutional, in progress
        {"62","MUSC-208","Instrumental Ensemble",       "N","3.00", "I","2010/SP",  "", "3",  "3",  "7717", "8113","0.00","1.00", "", "", "", "", "", "","G",   "003","",  ""}, // Institutional, no credit
        {"63","MUSC-209","Instrumental Ensemble III",   "N","3.00", "I","2010/SP", "B", "3",  "9",  "7718", "8115","0.00","3.00", "", "", "", "", "", "","G",   "002","",  ""}, // No replacement
        {"64","MUSC-209","Instrumental Ensemble III",   "N","3.00", "I","2011/SP", "A", "4",  "12", "7718", "8116","0.00","3.00", "", "", "", "", "", "","G",   "001","",  ""}, // No replacement
        {"65","MUSC-210","Instrumental Ensemble IV",    "N","3.00", "I","2010/SP", "C", "3",  "6",  "7719", "8117","0.00","3.00", "", "", "", "", "", "","G",   "002","",  ""}, // Replaced
        {"66","MUSC-210","Instrumental Ensemble IV",    "N","3.00", "I","2011/SP", "A", "3",  "12", "7719", "8118","0.00","3.00", "", "", "", "", "", "","G",   "001","",  ""}, // Replacement
        {"67","MUSC-211","Instrumental Ensemble V",     "N","3.00", "I","2010/SP", "C", "3",  "6",  "7720", "8119","0.00","3.00", "", "", "", "", "", "","G",   "002","",  ""}, // Possibly Replaced
        {"68","MUSC-211","Instrumental Ensemble V",     "N","3.00", "I","2011/SP",  "", "3",  "12", "7720", "8120","0.00","3.00", "", "", "", "", "", "","G",   "001","",  ""}, // Possible Replacement
        {"69","MUSC-212","Instrumental Ensemble VI",    "N","3.00", "I","2014/FA", "A", "3",  "12", "7721", "765", "0.00","3.00", "", "", "", "", "", "","G",   "001","",  ""}, // Possible Replacement

        // Used by free up extra academic credits tests
        {"70","RELG-100","World Religions",             "N","2.00","I", "2010/SP",  "B", "2", "6",  "7722", "8024","0.00","2.00", "", "", "", "", "", "","G",   "005","",  ""},
        {"71","RELG-101","Beginning Budism",            "N","3.00","I", "2011/SP",  "",  "3", "6",  "7723", "8025","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // Institutional, in progress
        // Another failed Academic Credit to be used in ReqWithExcludingTypeDoesNotSharePlannedCourseIfCourseHasBeenFailedPreviously test
        {"72","LAW-368","Introduction to Bird Law",     "N","3.00", "I", "2009/FA", "F", "0", "0",  "7724", "8026","0.00","0.00", "", "", "", "", "", "","G",   "001","",  ""},// Failed, zero credit
        {"73","noncourse","History of Music transfer",  "TR","4.00","TR","2010/SP", "B", "3", "12", "",     "",    "0.00","4.00", "", "", "", "", "", "","G",   "003","",  ""},// TRANSFER
        {"74","MATH-151","Calculus I",                  "W","3.00", "I", "2011/SP", "",  "4", "12", "324",  "8028","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""},// Withdrawal no grade
        {"80","MATH-4002","Ridiculously Hard Math",     "N","4.00", "I", "2017/SP", "A", "4", "12", "2419", "8108","0.00","4.00", "", "", "", "", "", "","G",   "005","3.00",""}, // Adjusted Credits != Credits
        {"99","JPM2-100","Academid Credit Rule Adapter Test","N","4.00","I","2010/SP","B","3","12", "",     "",    "0.00","4.00", "", "", "", "", "", "","G",   "001","",  ""}  ,   // Academic Credit Rule Adapter Tests
        {"100","MATH-151","Calculus I",                 "D","3.00", "I", "2011/SP", "",  "4", "12", "324",  "8028","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // dropped no grade
        {"101","MATH-151","Calculus I",                 "D","3.00", "I", "2011/SP", "B", "4", "12", "324",  "8028","0.00","3.00", "", "", "", "", "", "","G",   "005","",  ""}, // dropped with grade
        {"102","MATH-460","Intro to Real Analysis",     "D","3.00", "I", "2017/SP", "",  "4", "12", "213",  "8033","0.00","4.00", "", "", "", "", "", "","G",   "006","",  ""}, // dropped with no grade for replacement testing
        {"103","ENGL-200","Engl 200",                   "N","3.00", "I", "2017/SP", "A", "4", "12", "7428", "8034","0.00","3.00","A","B","C","A","B","C","UG",  "001","",  ""},//        //credits added for PROG.COURSE.REUSE student 0000111
        {"104","ENGL-300","engl 300",                   "N","3.00", "I", "2017/SP", "A", "4", "12", "7429", "8035","0.00","3.00","A","B","C","A","B","C","UG",  "001","",  ""},
        {"105","COMM-1321","comm 1321",                 "N","3.00", "I", "2017/SP", "A", "4", "12", "7430", "8036","0.00","3.00","A","B","C","A","B","C","UG",  "001","",  ""},
        {"106","COMM-100","comm 100",                   "N","3.00", "I", "2017/SP", "A", "4", "12", "7431", "8037","0.00","3.00","A","B","C","A","B","C","UG",  "001","",  ""},
        {"107","MATH-1004","math 1004",                 "N","1.00", "I", "2017/SP", "",  "",  "0",  "7432", "8038","0.00","1.00", "", "", "", "", "", "","UG",  "001","",  ""},// Institutional, in progress
        {"108","COMM-100","comm 100",                   "N",null,   "I", "2017/SP", "A", "4", "4",  "7431", "8037","0.00","3.00","A","B","C","A","B","C","UG",  "001","",  ""},//GPACredit is null
        {"109","MATH-1004-G","math 1004-G",             "N","1.00", "I", "2017/SP", "",  "",  "0",  "7432", "8038","0.00","",    "", "", "", "", "", "", "UG",  "001","",  ""}, //CompletedCredit is NULL

        //List of credits used for replace/ retakes tests
        {"110","MATH-300BB","Calculus AP","N","3.00","I","2009/FA","",  "",  "0",  "7435", "9000","0.00","0.00", "", "", "", "", "", "","G",   "001", "","0.00"}, //INPROGRESS course for 2009/SP
        {"111","MATH-300BB","Calculus AP","N","3.00","I","2010/SP","",  "",  "0",  "7435", "9001","0.00","0.00", "", "", "", "", "", "","G",   "001", "","0.00"}, //INPROGRESS course for 2010/SP
        {"112","MATH-300BB","Calculus AP","N","3.00","I","2018/SP","",  "",  "0",  "7435", "9002","0.00","0.00", "", "", "", "", "", "","G",   "001", "","0.00"}, //INPROGRESS course for 2017/SP
        {"113","MATH-300BB","Calculus AP","N","3.00","I","2009/FA","A", "4", "12", "7435", "9000","0.00","3.00", "", "", "", "", "", "","G",   "001", "","3.00"}, //COMPLETED course for 2009/SP
        {"114","MATH-300BB","Calculus AP","N","3.00","I","2010/SP","B", "3", "10", "7435", "9001","0.00","3.00", "", "", "", "", "", "","G",   "001", "","3.00"}, //COMPLETED course for 2010/SP
        {"115","MATH-300BB","Calculus AP","N","3.00","I","2018/SP","C", "2", "8",  "7435", "9002","0.00","3.00", "", "", "", "", "", "","G",   "001", "","3.00"}, //COMPLETED course for 2017/SP

        //List of credits to test for  extra course 
        {"116","DANC-100","Dance 0",      "N","1.00","I","2018/SP","",  "",  "0",  "122",  "9000","0.00","0.00", "", "", "", "", "", "","UG",   "001","",""}, //In-progress course for 2018/SP
        {"117","DANC-101","Dance 1",      "N","2.00","I","2018/SP","",  "",  "0",  "7437", "9001","0.00","0.00", "", "", "", "", "", "","UG",   "001","",""}, //In-progress course for 2018/SP
        {"118","DANC-102","Dance 2",      "N","1.00","I","2018/SP","",  "",  "0",  "7438", "9002","0.00","0.00", "", "", "", "", "", "","UG",   "001","",""}, //In-progress course for 2018/SP
        {"119","ENGL-201","English II",   "N","3.00","I","2018/SP","",  "",  "0",  "7439", "9000","0.00","0.00", "", "", "", "", "", "","UG",   "001","",""}, //In-progress course for 2018/SP

        //More credits used for replace/retakes tests - dropped graded and ungraded
        {"120","MATH-300BB","Calculus AP","D","3.00","I","2009/FA","A", "4", "12", "7435", "9000","0.00","3.00", "", "", "", "", "", "","G",   "001", "","3.00"}, //dropped with Grade 2009/SP
        {"121","MATH-300BB","Calculus AP","D","3.00","I","2010/SP","B", "3", "10", "7435", "9001","0.00","3.00", "", "", "", "", "", "","G",   "001", "","3.00"}, //dropped with Grade 2010/SP
        {"122","MATH-300BB","Calculus AP","D","3.00","I","2018/SP","",  "4", "12", "7435", "9002","0.00","3.00", "", "", "", "", "", "","G",   "001", "","3.00"}, //dropped no grade 2017/SP
        {"123","MATH-300BB","Calculus AP","N","3.00","I","2018/FA","A", "4", "12", "7435", "9002","0.00","3.00", "", "", "", "", "", "","G",   "001", "","3.00"}, //COMPLETED course for 2018/FA
        {"124","MATH-300BB","Calculus AP","N","3.00","I","2019/SP","",  "",  "0",  "7435", "9002","0.00","0.00", "", "", "", "", "", "","G",   "001", "","0.00"}, //INPROGRESS course for 2019/SP

        //credits for in.list.order tests
        {"130","FREN-100","fRENCH BASICS",             "N","3.00","I","2015/FA","",  "",  "0",  "7441", "9003","0.00","0.00", "", "", "", "", "", "","G",   "001", "","0.00"},
        {"131","HIND-100","HINDI BASICS",              "N","3.00", "I","2015/FA","",  "",  "0",     "7443", "9004","0.00","0.00", "", "", "", "", "", "","UG",   "001", "",""},
        {"129","POLI-100","POLI BASICS",               "N","3.00", "I","2015/FA","A",  "4",  "12",     "155", "9005","3.00","3.00", "", "", "", "", "", "","UG",   "001", "","3.00"},
        {"128","HUMT-100","HUMANITY BASICS",           "N","3.00", "I","2015/FA","B",  "3",  "9",     "7445", "9006","3.00","3.00", "A", "B", "C", "", "", "","UG",   "001", "","3.00"},
        {"127","CRIM-100","CRIM BASICS",               "N","3.00", "I","2015/FA","",  "",  "0",     "7446", "9007","0.00","0.00", "", "", "", "", "", "","UG",   "001", "",""},

        };
        public async Task<IEnumerable<AcademicCredit>> GetAsync(ICollection<string> ids, bool bestFit = false, bool filter = true, bool includeDrops = false)
        {
            if (acadcreds.Count() == 0) { await PopulateAsync(); }
            ICollection<AcademicCredit> AcademicCredits = new List<AcademicCredit>();
            foreach (string id in ids)
            {
                try
                {
                    // This is a bit of a mess, but I don't want to rewrite all the tests that depend on this.  The dict
                    // that contains the credits is indexed with coursenames, i.e. SUBJ*100.  There is now a separate
                    // dict by ACAD.CRED.ID.  I don't want to have two methods, so we check both here.

                    // If the incoming course name has a dash, change to asterisk.  Then check to see if that name
                    // is indexed.  If not, try using the incoming ID in the other dictionary.  If that fails, throw.

                    string credname = id;
                    if (credname.Contains('-'))
                    {
                        credname = credname.Replace('-', '*');
                    }
                    // to test the bestFit option, there are two credits that have null terms(39 and 40).  Map these to the 2009/SP term
                    // arbitrarily by setting their credit start and end dates to 01/20/2009 and 05/11/2009 respectively.

                    if (acadcreds.Keys.Contains(credname))
                    {
                        AcademicCredits.Add(acadcreds[credname]);
                    }
                    else
                    {
                        AcademicCredits.Add(acadcredsbyid[id]);
                    }

                }


                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("Academic Credit for Course " + id + " not found.");
                }
            }
            if (filter)
            {
                if (includeDrops)
                {
                    return AcademicCredits.Where(ac =>
                        (ac.Status == CreditStatus.Add ||
                        ac.Status == CreditStatus.New ||
                        ac.Status == CreditStatus.Preliminary ||
                        ac.Status == CreditStatus.Withdrawn ||
                        ac.Status == CreditStatus.TransferOrNonCourse ||
                        ac.Status == CreditStatus.Dropped));
                }
                return AcademicCredits.Where(ac =>
                    (ac.Status == CreditStatus.Add ||
                    ac.Status == CreditStatus.New ||
                    ac.Status == CreditStatus.Preliminary ||
                    ac.Status == CreditStatus.Withdrawn ||
                    ac.Status == CreditStatus.TransferOrNonCourse));
            }
            else
            {
                return AcademicCredits;
            }
        }

        public Task<Dictionary<string, List<AcademicCredit>>> GetAcademicCreditByStudentIdsAsync(IEnumerable<string> studentIds, bool bestFit = false, bool filter = true, bool includeDrops = false)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, List<PilotAcademicCredit>>> GetPilotAcademicCreditsByStudentIdsAsync(IEnumerable<string> studentIds, AcademicCreditDataSubset subset, bool bestFit = false, bool filter = true, string term = null)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, List<PilotAcademicCredit>>> GetPilotAcademicCreditsByStudentIds2Async(IEnumerable<string> studentIds, AcademicCreditDataSubset subset, bool bestFit = false, bool filter = true, string term = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get() with no arguments, formerly GetAllForStudent(), returns all credits in the test repository
        /// </summary>
        public async Task<IEnumerable<AcademicCredit>> GetAsync()
        {
            if (acadcreds.Count() == 0) { await PopulateAsync(); }
            // arguments:
            // list of keys (from the full populate)
            // best fit: set to false to put credits without a term into nonterm list
            // filter: set to false to get ALL credits, not filter out dropped/cancelled items
            return await GetAsync(acadcredsbyid.Keys, false, false);
        }

        /// <summary>
        /// Convenience for tests, returns a single acad cred by id
        /// </summary>
        public async Task<AcademicCredit> GetAsync(string creditid)
        {
            return (await GetAsync(new List<string>() { creditid }, false, false)).FirstOrDefault();
        }

        public async Task<bool> GetPilotCensusBooleanAsync()
        {
            return (await GetPilotCensusBooleanAsync());
        }

        public async Task<CreditStatus> ConvertCreditStatusAsync(string term)
        {
            return (await ConvertCreditStatusAsync(term));
        }

        public async Task<IEnumerable<AcademicCredit>> GetAcademicCreditsBySectionIdsAsync(IEnumerable<string> sectionIds)
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, List<AcademicCredit>>> GetSortedAcademicCreditsBySortSpecificationIdAsync(IEnumerable<AcademicCredit> acadCredits, IEnumerable<string> sortSpecIds)
        {
            throw new NotImplementedException();
        }

        private async Task PopulateAsync()
        {
            try
            {
                TestCourseRepository courserepo = new TestCourseRepository();
                ICollection<Grade> gradelist = await new TestGradeRepository().GetAsync();
                Dictionary<string, Course> courses = new Dictionary<string, Course>();
                IEnumerable<Term> termlist = new TestTermRepository().Get();
                Dictionary<string, Term> terms = new Dictionary<string, Term>();
                IDictionary<string, CreditType> types = new Dictionary<string, CreditType>();
                IDictionary<string, string> localTypes = new Dictionary<string, string>();
                IDictionary<string, CreditStatus> statuses = new Dictionary<string, CreditStatus>();
                IDictionary<string, GradingType> gradingTypes = new Dictionary<string, GradingType>();

                types.Add("I", CreditType.Institutional);
                types.Add("C", CreditType.ContinuingEducation);
                types.Add("TR", CreditType.Transfer);
                types.Add("O", CreditType.Other);

                localTypes.Add("I", "IN");
                localTypes.Add("C", "CE");
                localTypes.Add("TR", "TRN");
                localTypes.Add("O", "OTH");

                statuses.Add("N", CreditStatus.New);
                statuses.Add("A", CreditStatus.Add);
                statuses.Add("D", CreditStatus.Dropped);
                statuses.Add("W", CreditStatus.Withdrawn);
                statuses.Add("X", CreditStatus.Deleted);
                statuses.Add("C", CreditStatus.Cancelled);
                statuses.Add("PR", CreditStatus.Preliminary);
                statuses.Add("TR", CreditStatus.TransferOrNonCourse);
                statuses.Add("NC", CreditStatus.TransferOrNonCourse);

                gradingTypes.Add("G", GradingType.Graded);
                gradingTypes.Add("P", GradingType.PassFail);
                gradingTypes.Add("A", GradingType.Audit);

                foreach (Course c in courserepo.GetAsync().Result)
                {
                    try
                    {
                        courses.Add(c.Id, c);
                    }
                    catch
                    {
                        // Tired of re-running this to figure out which one broke it.
                        throw new Exception("Problem in TestCourseRepository.  Course " + c.Id.ToString() + " exists multiple times.");
                    }
                }

                foreach (Term t in termlist) { terms.Add(t.Code, t); }

                int items = credArray.Length / 24;

                for (int x = 0; x < items; x++)
                {
                    if (credArray[x, 0] == "108")
                    {
                        int k = 0;
                    }

                    var courseId = credArray[x, 10];
                    Course c = null;

                    try
                    {
                        if (!string.IsNullOrEmpty(courseId))
                        {
                            c = courses[courseId];
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception("Could not find " + courseId + " in the courses dictionary");
                    }
                    //if (c.ToString() == "ENGL-101")
                    //{
                    //    Console.WriteLine();
                    //}

                    // Build the academic credit object
                    string acadcredid = credArray[x, 0];
                    string sectionid = credArray[x, 11];
                    AcademicCredit ac = null;
                    if (c != null)
                    {
                        ac = new AcademicCredit(acadcredid, c, sectionid);
                    }
                    else
                    {
                        ac = new AcademicCredit(acadcredid);
                    }

                    // These are all the same for now
                    try
                    {
                        ac.Credit = decimal.Parse(credArray[x, 4]);
                    }
                    catch
                    {
                        ac.Credit = 0m;
                    }
                    // For now only the Passing grade will have attempted = 0. The rest will match the credits.
                    ac.AttemptedCredit = credArray[x, 7] != "P" ? ac.Credit : 0;
                    try
                    {
                        ac.CompletedCredit = decimal.Parse(credArray[x, 13]);
                    }
                    catch
                    {
                        ac.CompletedCredit = null;
                    }
                    try
                    {
                        ac.GpaCredit = decimal.Parse(credArray[x, 4]);
                    }
                    catch
                    {
                        ac.GpaCredit = null;
                    }


                    ac.GradePoints = decimal.Parse(credArray[x, 9]);
                    try
                    {
                        ac.AdjustedCredit = decimal.Parse(credArray[x, 22]);
                    }
                    catch
                    {
                        try
                        {
                            ac.AdjustedCredit = decimal.Parse(credArray[x, 4]);
                        }
                        catch
                        {
                            ac.AdjustedCredit = 0m;
                        }
                    }

                    try
                    {
                        ac.AdjustedGpaCredit = decimal.Parse(credArray[x, 23]);
                    }
                    catch
                    {
                        try
                        {
                            ac.AdjustedGpaCredit = decimal.Parse(credArray[x, 4]);
                        }
                        catch
                        {
                            ac.AdjustedGpaCredit = 0m;
                        }
                    }

                    ac.AdjustedGradePoints = decimal.Parse(credArray[x, 9]);

                    ac.ContinuingEducationUnits = decimal.Parse(credArray[x, 12]);
                    if (c != null)
                    {
                        ac.CourseName = c.ToString();
                    }
                    else
                    {
                        ac.CourseName = "";
                    }

                    // Allow this one to be replaced, and flag one as replaced.
                    // Normally both flags default to false.
                    if (ac.CourseName == "MUSC*210")
                    {
                        ac.CanBeReplaced = true;
                        ac.RepeatAcademicCreditIds = new List<string> { "65", "66" };
                        if (ac.Id == "65")
                        {
                            ac.ReplacedStatus = ReplacedStatus.Replaced;
                        }
                    }

                    // This one can be replaced, replacement is in progress (but the academichistory class figures that out)
                    if (ac.CourseName == "MUSC*211")
                    {
                        ac.CanBeReplaced = true;
                        ac.RepeatAcademicCreditIds = new List<string> { "67", "68" };
                    }
                    // Also mark MATH-460 as a course that can be replaced.  In this case it won't be replaced because the only possibility is a credit that is incomplete and has been dropped.
                    if (ac.CourseName == "MATH*460")
                        ac.CanBeReplaced = true;

                    if (credArray[x, 6] == "")
                        ac.TermCode = null;
                    else ac.TermCode = terms[credArray[x, 6]].Code;

                    if (ac.TermCode == "2009/SP")
                    {
                        ac.EndDate = new DateTime(2009, 5, 11);
                        ac.StartDate = new DateTime(2009, 1, 20);
                    }
                    if (ac.TermCode == "2009/FA")
                    {
                        ac.EndDate = new DateTime(2009, 12, 11);
                        ac.StartDate = new DateTime(2009, 8, 20);
                    }
                    if (ac.TermCode == "2010/SP")
                    {
                        ac.EndDate = new DateTime(2010, 5, 11);
                        ac.StartDate = new DateTime(2010, 1, 20);
                    }

                    if (ac.Id == "61") // HU-1000 institutional, in progress credit
                    {
                        // No term on this item, so give it a start date and leave end date null 
                        ac.StartDate = new DateTime(2009, 10, 1);
                    }

                    if (ac.Id == "62") // MUSC-208 institutional, never graded, must have end date. (Logic above does not put dates on 2011/FA term items)
                    {
                        ac.StartDate = new DateTime(2011, 8, 1);
                        ac.EndDate = new DateTime(2011, 12, 15);
                    }

                    // to test the bestFit, map the start and end dates of the two credits with 
                    // null term codes into 2009/WI arbitrarily since credArray has no date info
                    if (credArray[x, 0] == "39" || credArray[x, 0] == "40")
                    {
                        ac.StartDate = new DateTime(2008, 12, 29);
                        ac.EndDate = new DateTime(2009, 1, 19);
                    }

                    // Because the department of the academic credit's course is no longer used on the academic credit, make some adjustments to the
                    // departments on some specific academic credits so that the tests continue to pass. There will continue to be some academic credits
                    // (such as COMM-200) that have a different department in the acad cred vs the department on the course.
                    switch (credArray[x, 0])
                    {
                        case "1":
                            ac.AddDepartment("POLI");               // HIST-100
                            ac.AddDepartment("HIST");
                            break;
                        case "10":
                            ac.AddDepartment("POLI");               // HIST-400
                            ac.AddDepartment("HIST");
                            break;
                        case "15":
                            ac.AddDepartment("MDLL");               // SPAN-100
                            break;
                        case "20":
                            ac.AddDepartment("MDLL");               // SPAN-300
                            break;
                        case "21":
                            ac.AddDepartment("PERF");               // DANC-100
                            break;
                        case "22":
                            ac.AddDepartment("PERF");               // DANC-200
                            break;
                        default:
                            if (ac.SubjectCode != null)
                            {
                                ac.AddDepartment(ac.SubjectCode);       // All others will just have the subject code also be the department. 
                            }
                            break;
                    }
                    if (c != null)
                    {
                        ac.CourseLevelCode = c.CourseLevelCodes.First();  // in real life this would come from the section, thence to the stc, not from the course directly.
                        ac.AcademicLevelCode = c.AcademicLevelCode;           // ditto
                        ac.SubjectCode = c.SubjectCode;
                    }

                    ac.SectionNumber = credArray[x, 21];

                    if (!string.IsNullOrEmpty(credArray[x, 7]))
                    {
                        ac.VerifiedGrade = gradelist.First(gl => gl.Id == credArray[x, 7]);
                        ac.GradeSchemeCode = ac.VerifiedGrade.GradeSchemeCode;  // duplicate domain objects here?
                        ac.VerifiedGradeTimestamp = new DateTimeOffset(new DateTime(), new TimeSpan(0, 0, 0));
                    }
                    if (ac.Id == "36" || ac.Id == "37")
                    {
                        // These are ungraded credits so they cannot get their grade scheme from their grade.
                        // If grade scheme is not provided their sort order will be 2 not 4 (in progress).
                        ac.GradeSchemeCode = "UG";
                    }

                    // add any midterm grades to the academic credit
                    if (!string.IsNullOrEmpty(credArray[x, 14])) { ac.AddMidTermGrade(new MidTermGrade(1, credArray[x, 14], new DateTime())); }
                    if (!string.IsNullOrEmpty(credArray[x, 15])) { ac.AddMidTermGrade(new MidTermGrade(2, credArray[x, 15], new DateTime())); }
                    if (!string.IsNullOrEmpty(credArray[x, 16])) { ac.AddMidTermGrade(new MidTermGrade(3, credArray[x, 16], new DateTime())); }
                    if (!string.IsNullOrEmpty(credArray[x, 17])) { ac.AddMidTermGrade(new MidTermGrade(4, credArray[x, 17], new DateTime())); }
                    if (!string.IsNullOrEmpty(credArray[x, 18])) { ac.AddMidTermGrade(new MidTermGrade(5, credArray[x, 18], new DateTime())); }
                    if (!string.IsNullOrEmpty(credArray[x, 19])) { ac.AddMidTermGrade(new MidTermGrade(6, credArray[x, 19], new DateTime())); }

                    var xxx = credArray[x, 5];
                    if (types.Keys.Contains(credArray[x, 5]))
                    {
                        ac.Type = types[credArray[x, 5]];
                    }
                    else
                    {
                        ac.Type = CreditType.Other;
                    }

                    if (localTypes.Keys.Contains(credArray[x, 5]))
                    {
                        ac.LocalType = localTypes[credArray[x, 5]];
                    }
                    else
                    {
                        ac.LocalType = "OTH";
                    }

                    // status 
                    if (statuses.Keys.Contains(credArray[x, 3]))
                    {

                        ac.Status = statuses[credArray[x, 3]];
                    }
                    else
                    {
                        ac.Status = CreditStatus.Unknown;
                    }
                    if (gradingTypes.Keys.Contains(credArray[x, 20]))
                    {
                        ac.GradingType = gradingTypes[credArray[x, 20]];
                    }

                    //ac.Title = c.Title;
                    ac.StudentCourseSectionId = credArray[x, 0];


                    // The course name index is outdated and won't get you what you want
                    // in the case of courses that have multiple credits.  It's only
                    // there for backward compatibility for tests written a long time ago.
                    // Caveat emptor.

                    if (!acadcreds.Keys.Contains(ac.CourseName))
                    {
                        acadcreds.Add(ac.CourseName, ac);
                    }
                    acadcredsbyid.Add(ac.Id, ac);

                    // Set additional properties for rule adapter testing
                    switch (credArray[x, 0])
                    {
                        case "99":
                            ac.AcademicLevelCode = "UG";
                            ac.StudentId = "0001234";
                            ac.Mark = "ABCD";
                            ac.FinalGradeId = "A";
                            break;
                        default:
                            break;
                    }

                }

                //Noncourses

                //Reasonable ones
                string noncourseacadcredid1 = "1001";
                AcademicCredit nc = new AcademicCredit(noncourseacadcredid1);
                nc.Status = CreditStatus.Preliminary;
                // nc.Title = "Preliminary equivalent eval for placement test";
                nc.Type = CreditType.Other;
                nc.LocalType = localTypes["O"];
                nc.AddDepartment("HIST");
                nc.Credit = 2m;
                acadcreds.Add("NONCOURSE1", nc);
                acadcredsbyid.Add("1001", nc);

                string noncourseacadcredid2 = "1002";
                AcademicCredit nc2 = new AcademicCredit(noncourseacadcredid2);
                nc2.Status = CreditStatus.TransferOrNonCourse;
                nc2.Type = CreditType.Transfer;
                nc2.LocalType = localTypes["TR"];
                nc.AddDepartment("ENGL");
                nc.Credit = 3m;
                acadcreds.Add("NONCOURSE2", nc2);
                acadcredsbyid.Add("1002", nc2);


                // Strange statuses

                //N    New                    N   1
                //A    Add                    A   2
                //D    Dropped                D   3
                //W    Withdrawn              W   4
                //X    Deleted                X   5
                //C    Cancelled              C   6
                //PR   Preliminary Equiv Eval PR  8
                //TR   Transfer Equiv Eval    TR  7
                //NC   Noncourse Equivalency  NC  7
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public Task<Tuple<IEnumerable<StudentCourseTransfer>, int>> GetStudentCourseTransfersAsync(int offset, int limit, bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<StudentCourseTransfer> GetStudentCourseTransferByGuidAsync(string guid, bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AcademicCreditMinimum>> GetAcademicCreditMinimumAsync(ICollection<string> academicCreditIds, bool filter = true, bool includeDrops = false)
        {
            throw new NotImplementedException();
        }

        public Task<CreditType> GetCreditTypeAsync(string typecode)
        {
            throw new NotImplementedException();
        }
        public Task<AcademicCreditsWithInvalidKeys> GetAcademicCreditsBySectionIdsWithInvalidKeysAsync(IEnumerable<string> sectionIds)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AcademicCredit>> FilterAcademicCreditsAsync(IEnumerable<AcademicCredit> acadCredits, string criteria)
        {
            throw new NotImplementedException();
        }
    }
}
