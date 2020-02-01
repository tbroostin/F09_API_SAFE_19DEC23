// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using Moq;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestCourseRepository : BaseColleagueRepository, ICourseRepository
    {
        public Course Biol100 { get { return GetAsync("110").Result; } }
        public Course Biol200 { get { return GetAsync("21").Result; } }
        public Course Math100 { get { return GetAsync("143").Result; } }
        public Course Hist400 { get { return GetAsync("87").Result; } }
        public Course Hist100 { get { return GetAsync("139").Result; } }

        public ICacheProvider cacheProvider;
        public IColleagueTransactionFactory transactionFactory;


        public Dictionary<string, string> EthosExtendedDataDictionary
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        private string[,] courseArray = {
                //index, courseId, subj-number, dept, title, course level, acad level, description, location
                {"1","139","HIST-100","HIST,POLI","World History to WWII","100,200", "UG", "Detailed history of the world up to WWII",""},
                {"2","42","HIST-200","HIST","Intermediate History","200", "UG", "Lots of intermediate history","MAIN"}, 
                {"3","110","BIOL-100","BIOL","Molecular Biology","100", "UG","Biology of molecules","MAIN"},
                {"4","21","BIOL-200","BIOL","Intermediate Biology","200", "A1","Lots of biology terminology","SC"},
                {"5","47","MATH-201","MATH","Linear Algebra","200", "UG","Pretty complicated algebra","NW"},
                {"6","333","MATH-152","MATH","Calculus II","100", "UG","Advanced study of Calculus","SC"},
                {"7","64","BIOL-400","BIOL","Advanced Topics in Biology","400", "UG","Lots more biology terminology",""},
                {"8","143","MATH-100","MATH","Discrete Mathematics","100", "UG","Very complicated math",""},
                {"9","186","MATH-350","MATH","History of Mathmatics","300","UG", "The history of the development of mathematical theory",""},
                {"10","87","HIST-400","HIST,POLI","American History From WWI","300,400","UG", "History since world war two",""}, // Used for CourseRepositoryTests
                {"11","180","MATH-150","MATH","Pre-Calculus","100","UG", "Introduction to calculus",""},
                {"12","117","COMP-100","COMP","Introduction to Composition","100","UG", "This course teaches you to write",""},
                {"13","148","MUSC-100","MUSC","History of Music","100","UG", "Composers and style since the beginning of time",""} ,
                {"14","355","MUSC-207","MUSC","Instrumental Ensemble","200","UG", "Required for orchestral students",""}, 
                {"15","160","SPAN-100","MDLL","Beginning Spanish","100","UG", "Introduction to this latin-based language",""} ,
                {"16","342","MATH-362","MATH","Modern Algebra","300","UG", "New ways of thinking about math",""},
                {"17","91","MATH-400","MATH","Axiomatic Systems","400","UG", "Mathematical Logical thought",""},
                {"18","306","MATH-102","MATH","Geometry","100","UG", "Introduction to geometrical theory and practice",""},
                {"19","315","MATH-103","MATH","Trigonometry","100","UG", "Introduction to tools needed for calculus",""},
                {"20","84","SPAN-300","MDLL","Advanced Spanish","300","UG", "Conversational spanish",""},
                {"21","122","DANC-100","PERF","Ballroom Dancing","100","UG", "Learn to dance like the Greatest Generation",""},
                {"22","28","DANC-200","PERF","Dance Theory","200","UG", "Study of dance","MAIN"},
                {"23","159","SOCI-100","SOCI","Introduction to Sociology","100","UG", "Study of the world societies",""},
                {"24","155","POLI-100","POLI","Intro to Political Science","100","UG", "Politics, complexities, insights",""},
                {"25","156","PSYC-100","PSYC","Introduction to Psychology","100","UG", "Learn about human motivation",""},
                {"26","187","ENGL-102","ENGL","College Writing II","100","UG", "Improve on what was learned in intro to composition",""},
                {"27","154","PHYS-100","PHYS","Introduction to Physics","100","UG", "The merging of science and math",""},
                {"28","324","MATH-151","MATH","Calculus I","100","UG", "First level calculus theory",""},
                {"29","185","MATH-228","MATH","Probability and Statistics","200","UG", "Practical application of algebra",""},
                {"30","289","HIST-415","HIST","Major Seminar II","400","UG", "Capstone for history majors",""},
                {"31","363","MATH-372","MATH","Differential Equations","300","UG","Study of differential equations",""},
                {"32","364","MATH-499","MATH","Senior Seminar","400","UG", "Capstone seminar for math majors",""},
                {"33","213","MATH-460","MATH","Intro to Real Analysis","400","UG", "Analysis of real numbers",""},
                {"34","353","MATH-371","MATH","Differential Equations","300","UG", "Introduction to differential equations",""},
                {"35","226","MATH-491","MATH","Math Seminar","400","UG", "Senior capstone for math majors",""},
                {"36","130","ENGL-101","ENGL","College Writing I","100","UG", "First writing for freshman",""},
                {"37","93","PHYS-400","PHYS","The Theory of Everything","400","UG", "Study the physical universe",""},
                {"38","78","MATH-300","MATH","Advanced Mathematics","300","UG", "Study of complex mathematical topics",""},
                {"39","46","MATH-200","MATH","Intermediate Mathematics","200","UG", "Study of intermediate mathematical topics","MAIN,WEST"}, 
                {"40","20","ART-200","ART","Intermediate Art","200","UG", "Intermediate artistic endeavors",""}, 
                {"41","9877","PHYS-501","PHYS","The Theory of Physics","500", "GR","Study the physical universe, graduate",""},
                {"42","9878","MATH-502","MATH","Advanced Mathematical Theory","500","GR", "Matters of mathematical topics",""},
                // at least 41 courses expected for CourseRespositoryTests

                // following items used by course search tests.
                //counter, ID,     subject-number,  dept,     title,     crs lvl, acad lvl, description,     location
                {"43","9879", "AAAA-BBB",      "CCCC",   "DDDDD",   "EEE",   "FF",     "GGGGGGGGGGG",   "HHH"},                
                {"44","9880", "XAAAAX-XBBBX",  "XCCCCX", "XDDDDDX", "XEEEX", "XFFX",   "XGGGGGGGGGGGX", "XHHHX"},                
                {"45","188","COMP-103","ENGL","College Writing III","100","UG", "Improve on what was learned in introductory and college writing II and composition","MAIN"},

                {"46","10000", "ACCT-101","ACCT","Accounting for Business I",	"100","CE", "Practical Accounting for Business Owners I"  ,""},
                {"47","10001", "ACCT-102","ACCT","Accounting for Business II",	"100","CE", "Practical Accounting for Business Owners II" ,""},
                {"48","10002", "ACCT-103","ACCT","Accounting for Business III", "100","CE", "Practical Accounting for Business Owners III",""},

                // Test courses used to test status/start/end dates and corequisite testing (see TestSectionRepository)
                {"49","7701", "DENT-101","DENT","Dental Tech 1",	"100","UG", "Dental Technition Practicum"  ,""},
                {"50","7702", "DENT-102","DENT","Dental Tech 2",	"100","UG", "Dental Technition Practicum"  ,""},
                {"51","7703", "DENT-103","DENT","Dental Tech 3",	"100","UG", "Dental Technition Practicum"  ,""},
                {"52","7704", "DENT-104","DENT","Dental Tech 4",	"100","UG", "Dental Technition Practicum"  ,""},
                {"53","7705", "DENT-105","DENT","Dental Tech 5",	"100","UG", "Dental Technition Practicum"  ,""},
                {"54","7706", "DENT-106","DENT","Dental Tech 6",	"100","UG", "Dental Technition Practicum"  ,""},
                
                {"55","10003", "HIST-198","HIST","History of Methodologies",	"100","UG", "Scrum, eXtreme, and other methodologies"  ,""},
                {"56","10004", "HIST-199","HIST","Dead Cultures",               "100","UG", "Abandon all hope" ,""},

                // Test data to match what is in colldev
                {"57","2414", "ENGL-1000","ENGL","Freshman Comp",	"100","UG", "Freshman Composition"  ,""},
                {"58","2415", "ENGL-1001","ENGL","Freshman Grammar",	"100","UG", "Freshman Grammar"  ,""},
                {"59","MATH*101", "MATH-1002","MATH","Freshman Calc",	"100","UG", "Freshman Calc"  ,""},  // this nonnumeric course id was breaking a lot of tests
                {"59","10999", "MATH-1000","MATH","Freshman Calc",	"100","UG", "Freshman Calc"  ,""},
                {"60","2416", "MATH-1001","MATH","Freshman Calc 2",	"100","UG", "Freshman Calc second semester"  ,""},
                {"61","2420", "HU-1000","ENGL",  "Caring About Your Fellow Man",	"100","UG", "Caring About Your Fellow Man"  ,""},
                {"62","2422", "COMM-2000","ENGL","Talkin with Folks",	"200","UG", "Talkin with Folks"  ,""},
                {"63","2417", "MATH-4000","MATH","Really Hard Math 4000",	"400","UG", "Really Hard Math 4000"  ,""},
                {"64","2418", "MATH-4001","MATH","Excruciatingly Hard Math 4001",	"400","UG", "Excruciatingly Hard Math 4001"  ,""},
                {"65","2419", "MATH-4002","MATH","Ridiculously Hard Math 4002",	"400","UG", "Ridiculously Hard Math 4002"  ,""},
                {"66","2450", "MATH-4003","MATH","Really Hard Math 4003",	"400","UG", "Really Hard Math 4003"  ,""},

                // Test courses for time conflict checking (See TestSectionRepository for section meeting time details
                {"66","7710", "DENT-110","DENT","Dental Tech 10",	"200","UG", "Dental Technition Practicum"  ,""},
                {"67","7711", "DENT-111","DENT","Dental Tech 12",	"200","UG", "Dental Technition Practicum"  ,""},
                {"68","7712", "DENT-112","DENT","Dental Tech 13",	"200","UG", "Dental Technition Practicum"  ,""},
                {"69","7713", "DENT-113","DENT","Dental Tech 14",	"200","UG", "Dental Technition Practicum"  ,""},
                {"70","7714", "DENT-114","DENT","Dental Tech 15",	"200","UG", "Dental Technition Practicum"  ,""},
                {"71","7715", "DENT-115","DENT","Dental Tech 16",	"200","UG", "Dental Technition Practicum"  ,""},

                // Test course for Intelligent Learning Platform data
                {"72","7272", "HIST-4070", "HIST", "History of ILP", "400", "UG", "History of Intelligent Learning Platform", ""},
                // Test course for subject that should not display
                {"73","7716", "NOSHOW-115","NODEPT","No show course","200", "UG", "No show course"  ,""},

                // test courses for course types (and pseudo courses)
                {"74","200", "CAMP-100","CAMP","Camping 100","100","UG","Introduction to Camping",""},
                {"75","201", "CAMP-150","CAMP","Camping 150","100","UG","Tent Camping",""},
                {"76","202", "CAMP-200","CAMP","Camping 200","200","UG","National Park Etiquette",""},
                // Added for academic credit sequencing
                {"77","7717","MUSC-208","MUSC","Instrumental Ensemble II","200","UG", "Required for orchestral students",""}, 
                {"78","7718","MUSC-209","MUSC","Instrumental Ensemble III","200","UG", "Yearly ensemble",""},
                {"79","7719","MUSC-210","MUSC","Instrumental Ensemble IV","200","UG", "Optional ensemble",""},
                {"80","7720","MUSC-211","MUSC","Instrumental Ensemble V","200","UG","Second optional ensemble",""},
                {"80","7721","MUSC-212","MUSC","Instrumental Ensemble VI","200","UG","Second optional ensemble",""},

                // Added for free up extra academic credits tests
                {"81","7722","RELG-100","RELG","World Religions","100","UG", "World Religions",""}, 
                {"82","7723","RELG-101","RELG","Beginning Budism","100","UG", "Beginning Budism",""},
                // Added for ReqWithExcludingTypeDoesNotSharePlannedCourseIfCourseHasBeenFailedPreviously test
                {"83","7724","LAW-368","LAW","Introduction to Bird Law","300","UG", "Bird Law",""},
                // Added for tests to hide "nonviewable" sections - some sections on the following 4 courses are non-viewable
                {"84","7424","CRIM-360","CRIM","Justice System Policies","300","UG", "Justice System Policies","MAIN,PR"},
                {"85","7425","CRIM-361","CRIM","Justice System and the Criminal Mind","300","UG", "Justice System and the Criminal Mind","MAIN,PR"},
                {"86","7426","CRIM-362","CRIM","Justice System Ethics","300","UG", "Justice System Ethics","MAIN,PR"},
                {"87","7427","CRIM-363","CRIM","Justice System Within","300","UG", "Justice System Within","MAIN,PR"},
                {"88","7428","ENGL-200","ENGL","English", "200","UG", "English part 2","MAIN,PR" },
                {"89","7429","ENGL-300","ENGL","English", "300","UG", "English part 3","MAIN,PR" },
                {"90","7430","COMM-1321","COMM","Communications", "100","UG", "Communications part 13","MAIN,PR" },
                {"91","7431","COMM-100","COMM","Communications", "100","UG", "Communications part 1","MAIN,PR" },
                {"92","7432","MATH-1004","MATH","Mathematics", "200","UG", "Mathematics part 2","MAIN,PR" },
                {"93","7433","COMM-1315","COMM","Communications", "100","UG", "Communications part 1315","" },
                // Added for testing sections' attendance tracking type 
                {"94","7434","COMM-123","COMM","Communications", "100","UG", "Communications 123","" },
                //test data for retake and replace testing
                {"95","7435","MATH-300BB","MATH","Mathematics","300","GR","Calculus AP","" },
                //test data for release extra courses from one requirement to other tests
                 //index, courseId, subj-number, dept, title, course level, acad level, description, location
                  {"97","7437","DANC-101","PERF","dance-101","100","UG","LOCAL Dance 2","" },
                   {"98","7438","DANC-102","PERF","dance-102","100","UG","LOCAL Dance 3","" },
                    {"99","7439","ENGL-201","ENGL","ENGLISH-201","200","UG","ENGLISH II","" },

                 // test data for course constructor failure (missing short title)
                 {"100","7440","ENGL-201BAD","ENGL","","200","UG","ENGLISH II","" },

                };

        public TestCourseRepository(ICacheProvider cacheProvider = null, IColleagueTransactionFactory transactionFactory = null, ILogger logger = null) : base(new Mock<ICacheProvider>().Object, new Mock<IColleagueTransactionFactory>().Object, logger)
        {
        }

        private ICollection<Course> BuildCourses(IEnumerable<string> ids)
        {
            
                ICollection<Course> courses = new List<Course>();
            try
            {
                // There are 9 fields for each course in the array
                var items = courseArray.Length / 9;

                for (int x = 0; x < items; x++)
                {
                    // course ID
                    var cId = courseArray[x, 1];

                    // Subject, split from course name
                    var cSubject = courseArray[x, 2].Split('-')[0];

                    // Number, split from course name
                    string cNumber = courseArray[x, 2].Split('-')[1];

                    // Department, may be multiple values separated by comma
                    var cDepts = new List<OfferingDepartment>();
                    List<string> depts = courseArray[x, 3].Split(',').ToList();
                    foreach (var d in depts)
                    {
                        cDepts.Add(new OfferingDepartment(d, 100 / depts.Count));
                    }

                    // Title
                    string cTitle = courseArray[x, 4];

                    string cLongTitle = courseArray[x, 7];

                    // Course Level, may be multiple values separated by comma
                    var cLevels = new List<string>();
                    cLevels.AddRange(courseArray[x, 5].Split(','));

                    // Academic level
                    string cAcadLevel = courseArray[x, 6];

                    // Course status and dates
                    CourseStatus cStatus = CourseStatus.Active;
                    DateTime? cStartDate = new DateTime();
                    DateTime? cEndDate = new DateTime();
                    List<CourseApproval> approvals = new List<CourseApproval>();
                    string guid = Guid.NewGuid().ToString();
                    switch (cId)
                    {
                        case "7701":
                            // Not current: Status active but Start date is after today
                            cStatus = CourseStatus.Active;
                            cStartDate = new DateTime(2035, 12, 01);
                            cEndDate = null;
                            approvals.Add(new CourseApproval("A", cStartDate.Value, "0000043", "0003315", cStartDate.Value) { Status = CourseStatus.Active });
                            guid = "f2718e7d-5132-4f5a-8a3f-38586e0db139";
                            break;
                        case "7702":
                            // Not current: Status active but End date ends before today
                            cStatus = CourseStatus.Active;
                            cStartDate = new DateTime(2010, 12, 01);
                            cEndDate = new DateTime(2010, 12, 03);
                            approvals.Add(new CourseApproval("A", cStartDate.Value, "0000043", "0003315", cStartDate.Value) { Status = CourseStatus.Active });
                            break;
                        case "7703":
                            // Current: Status active, Start date before today, end date null
                            cStatus = CourseStatus.Active;
                            cStartDate = new DateTime(2010, 12, 01);
                            cEndDate = null;
                            approvals.Add(new CourseApproval("A", cStartDate.Value, "0000043", "0003315", cStartDate.Value) { Status = CourseStatus.Active });
                            break;
                        case "7704":
                            // Current: Status active, current date within start/end dates
                            cStatus = CourseStatus.Active;
                            cStartDate = new DateTime(2010, 12, 01);
                            cEndDate = new DateTime(2035, 12, 31);
                            approvals.Add(new CourseApproval("A", cStartDate.Value, "0000043", "0003315", cStartDate.Value) { Status = CourseStatus.Active });
                            break;
                        case "7705":
                            // Not Current: Status of not active, null end date
                            cStatus = CourseStatus.Unknown;
                            cStartDate = new DateTime(2010, 12, 01);
                            cEndDate = null;
                            approvals.Add(new CourseApproval("U", cStartDate.Value, "0000043", "0003315", cStartDate.Value) { Status = CourseStatus.Unknown });
                            break;
                        case "7706":
                            // Not Current: Status of not active, end date later than current date
                            cStatus = CourseStatus.Terminated;
                            cStartDate = new DateTime(2010, 12, 01);
                            cEndDate = new DateTime(2035, 12, 31);
                            approvals.Add(new CourseApproval("T", cStartDate.Value, "0000043", "0003315", cStartDate.Value) { Status = CourseStatus.Terminated });
                            break;

                        

                        default:
                            // Current: status active, current date after start date
                            cStatus = CourseStatus.Active;
                            cStartDate = new DateTime(2001, 01, 01);
                            cEndDate = null;
                            approvals.Add(new CourseApproval("A", cStartDate.Value, "0000043", "0003315", cStartDate.Value) { Status = CourseStatus.Active });
                            break;
                    }

                    Course c;
                    switch (cId)
                    {
                        case "87":
                            c = new Course(cId, cTitle, cTitle + " and some extra text", cDepts, cSubject, cNumber, cAcadLevel, cLevels, 3.0m, 1.0m, approvals)
                            {
                                TermSessionCycle = "A",
                                TermYearlyCycle = "B",
                                GradeSchemeCode = "UG",
                                FederalCourseClassification = "ABCD",
                                ExternalSource = "EXT"
                            };
                            break;

                        
                        default:
                            c = new Course(cId, cTitle, cLongTitle, cDepts, cSubject, cNumber, cAcadLevel, cLevels, 3.0m, 1.0m, approvals);
                            break;
                    }
                    c.Name = courseArray[x, 2];

                    // GUID
                    c.Guid = guid ?? Guid.NewGuid().ToString().ToLowerInvariant();

                    // DescriptionSearch
                    c.Description = courseArray[x, 7];

                    // Location
                    if (courseArray[x, 8] != "")
                    {
                        c.LocationCodes = courseArray[x, 8].Split(',').ToList();
                    }

                    // Max credits
                    c.MaximumCredits = 4.0m;

                    // Variable Credit Increment
                    c.VariableCreditIncrement = 1.0m;

                    // Start/End date
                    c.StartDate = cStartDate;
                    c.EndDate = cEndDate;
                    // Topic Code
                    c.TopicCode = "T1";

                    // Set a few other specific topic codes for topic filter testing
                    if ((c.Id == "7701") || (c.Id == "7703") || (c.Id == "7704"))
                    {
                        c.TopicCode = "T2";
                    }
                    if ((c.Id == "186") || (c.Id == "87") || (c.Id == "21"))
                    {
                        c.TopicCode = "";
                    }
                    if (c.Id == "333")
                    {
                        c.TopicCode = "T3";
                    }

                    // Corequisites (CAUTION: Degree plan tests heavily depend upon this setup)
                    if ((c.Id == "7701") || (c.Id == "7702") || (c.Id == "7703") || (c.Id == "7704"))
                    {
                        c.Requisites = new List<Requisite>() { new Requisite("PREREQ1", true, RequisiteCompletionOrder.Previous, false), new Requisite("110", true), new Requisite("143", false) };
                    }

                    // Prerequisites
                    if (c.Id == "186")
                    {
                        c.Requisites = new List<Requisite>() { new Requisite("PREREQ1", true, RequisiteCompletionOrder.Previous, false) };
                    }
                    if (c.Id == "87")
                    {
                        c.Requisites = new List<Requisite>() { new Requisite("PREREQ2", false, RequisiteCompletionOrder.Previous, true) };
                    }

                    // Two new style Requisites - one recommended concurrent, one either required.
                    if (c.Id == "21")
                    {
                        c.Requisites = new List<Requisite>() { new Requisite("COREQ2", false, RequisiteCompletionOrder.Concurrent, false), new Requisite("REQ1", true, RequisiteCompletionOrder.PreviousOrConcurrent, false) };
                    }
                    // New style Concurrent Requisite - required
                    if (c.Id == "333")
                    {
                        c.Requisites = new List<Requisite>() { new Requisite("COREQ1", true, RequisiteCompletionOrder.Concurrent, false) };
                    }

                    // Course equate codes for degree plan domain tests
                    if (c.Id == "42")
                    {
                        c.AddEquatedCourseId("155"); // POLI 100 is an equated course of HIST 200
                    }
                    if (c.Id == "155")
                    {
                        c.AddEquatedCourseId("42"); // HIST 200 is an equated course of POLI 100
                    }
                    if (c.Id == "7701")
                    {
                        c.AddEquatedCourseId("7702");
                        c.AddEquatedCourseId("7703"); // Set up DENT 102 and DENT 103 as equates to this course
                    }

                    if (c.Id == "46")
                    {
                        c.AddLocationCycleRestriction(new LocationCycleRestriction("LOC1", "FA", "OY"));
                        c.AddLocationCycleRestriction(new LocationCycleRestriction("LOC2", "FA", ""));
                        c.AddLocationCycleRestriction(new LocationCycleRestriction("LOC3", "", ""));
                    }

                    if (c.Id == "7435")//MATH.BB COURSE for retake/ replace tests
                    {
                        c.AllowToCountCourseRetakeCredits = false;

                    }
                           
                    // Add course types
                    c.AddType("TYPEA");

                    // Add local credit type - the local string value of the credit type.
                    c.LocalCreditType = "IN";

                    // Add to course to returned list of courses
                    courses.Add(c);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }
            return courses;

        }

        public async Task<IEnumerable<Course>> GetAsync()
        {
            return await Task.FromResult(BuildCourses(Enumerable.Empty<string>()));
        }

        public async Task<Course> GetAsync(string id)
        {
            try
            {
                var courses = await GetAsync();
                return courses.Where(c => c.Id == id).First();
            }
            catch
            {
                throw new KeyNotFoundException("Course " + id.ToString() + "not found");
            }
        }

        public async Task<IEnumerable<Course>> GetAsync(ICollection<string> ids)
        {
            ICollection<Course> courses = new List<Course>();

            if (ids == null)
            {
                throw new ArgumentNullException("Course Ids may not be null");
            }
            else
            {
                List<Course> allCourses = (await GetAsync()).ToList();

                foreach (var id in ids)
                {
                    var crs = allCourses.Where(c => c.Id == id).FirstOrDefault();
                    if (crs != null)
                    {
                        courses.Add(crs);
                    }
                }
            }

            return courses;
        }

        public Task<IEnumerable<Course>> GetCoursesByIdAsync(IEnumerable<string> courseIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetCoursesIdsByActivityDate(DateTime? modifiedBefore, DateTime? modifiedAfter)
        {
            throw new NotImplementedException();
        }

        public async Task<Course> GetCourseByGuidAsync(string guid)
        {
            try
            {
                var courses = await GetAsync();
                return courses.Where(c => c.Id == guid).First();
            }
            catch
            {
                throw new KeyNotFoundException("Course " + guid.ToString() + "not found");
            }
        }

        public Task<Course> CreateCourseAsync(Course course, string source, string version)
        {
            return null;
        }

        public Task<Course> UpdateCourseAsync(Course course, string source, string version)
        {
            return null;
        }

        public Task<string> GetCourseGuidFromIdAsync(string courseId)
        {
            return null;
        }

        public Task<IEnumerable<Course>> GetNonCacheAsync()
        {
            return null;
        }

        public Task<IEnumerable<Course>> GetNonCacheAsync(string subject, string number, string academicLevel, string owningInstitutionUnit, string title, string instructionalMethods, string startOn, string endOn, string topic, string categories)
        {
            return null;
        }

        public Task<IEnumerable<Course>> GetAsync(string newSubject, string number, string newAcademicLevel, string newOwningInstitutionUnit, string title, string newInstructionalMethods, string newStartOn, string newEndOn, string newTopic, string newCategories)
        {
            return null;
        }

        public async Task<Tuple<IEnumerable<Course>, int>> GetPagedCoursesAsync(int offset, int limit, string subject, string number, List<string> academicLevel, List<string> owningInstitutionUnit, string title, List<string> instructionalMethods, string startOn, string endOn, string topic, string categories)
        {
            try
            {
                var courses = await GetAsync();
                var totalCount = courses.Count();
                return new Tuple<IEnumerable<Course>, int>(courses, totalCount);
            }
            catch
            {
                throw new KeyNotFoundException("Courses list not found");
            }
        }
        
        public async Task<Tuple<IEnumerable<Course>, int>> GetPagedCoursesAsync(int offset, int limit, string subject, string number, List<string> academicLevel, List<string> owningInstitutionUnit, List<string> title, List<string> instructionalMethods, string startOn, string endOn, string topic = "", List<string> categories = null, string activeOn = "")
        {
            try
            {
                var courses = await GetAsync();
                var totalCount = courses.Count();
                return new Tuple<IEnumerable<Course>, int>(courses, totalCount);
            }
            catch
            {
                throw new KeyNotFoundException("Courses list not found");
            }
        }

        public Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename)
        {
            throw new NotImplementedException();
        }

        public Task<Course> GetCourseByGuid2Async(string guid, bool addToCollection = false)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Course>, int>> GetPagedCoursesAsync(int offset, int limit, string subject, string number, List<string> academicLevel, List<string> owningInstitutionUnit, string title, List<string> instructionalMethods, string startOn, string endOn, string topic = "", string category = "", bool addToCollection = false)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Course>, int>> GetPagedCoursesAsync(int offset, int limit, string subject, string number, List<string> academicLevel, List<string> owningInstitutionUnit, List<string> titles, List<string> instructionalMethods, string startOn, string endOn, string topic = "", List<string> categories = null, string activeOn = "", bool addToCollection = false)
        {
            throw new NotImplementedException();
        }
    }
}
