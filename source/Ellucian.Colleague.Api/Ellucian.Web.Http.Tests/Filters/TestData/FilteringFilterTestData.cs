// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Web.Http.Tests.Filters.TestData
{
    public class FilteringFilterTestData
    {
        public List<TestOrg> ListOrgs{get;set;}
        public List<TestDept> ListDepts { get; set; }
        public List<TestStudent> ListStudents { get; set; }
        public List<TestCourse> ListCourses { get; set; }
       
        TestOrg Org1 { get; set; }
        TestOrg Org2 { get; set; }
        TestOrg Org3 { get; set; }

        public FilteringFilterTestData()
        {
            TestCourse Course1 = new TestCourse("Algorithms", 12645873358, true);
            Course1.RelatedSubCourseIds = new List<decimal> { 1.11m, 1.21m , 1.31m};

            TestCourse Course2 = new TestCourse("Database", 986245748145256, null);
            Course2.RelatedSubCourseIds = new List<decimal> { 2.11m, 2.12m, 2.13m, 2.14m };

            TestCourse Course3 = new TestCourse("Chemistry101", 6562456323, false);
            //Course3.RelatedSubCourseIds = new List<decimal> { 3.1m, 2.12m, 3.13m , 2.14m, 2.13m};

            TestStudent Student1 = new TestStudent("Chandrababu", new DateTime(1970, 12, 05), true, 001, 4.93m, Course1);
            Student1.HomeCountry = Country.Sweden;
            //Student1.Hobbies = new List<string> { "Act", "Knit" };

            TestStudent Student2 = new TestStudent("Jane Doe", new DateTime(1978, 05, 18), false, 201, 2.15m, Course2);
            Student2.Hobbies = new List<string> { "Knit", "Sew", "Hockey" };
            Student2.HomeCountry = Country.Singapore;

            TestStudent Student3 = new TestStudent("Mary", new DateTime(1990, 07, 2), true, 359, 3.25m, Course3);
            Student3.HomeCountry = Country.Sweden;
            Student3.Hobbies = new List<string> { "Sew", "Baseball", "Ballet", "Hockey", null, "Knit" };

            TestDept Dept1 = new TestDept("Dept1", null, null, 1010, Student2);
            TestDept Dept2 = new TestDept("Dept2", 455, new DateTime(1928, 3, 5), 1.2012, Student1);
            TestDept Dept3 = new TestDept("Dept3", 78, DateTime.Now, 192.8, Student3);

            Org1 = new TestOrg("Org1", 2.666F, Dept1);
            Org1.Colors = new List<string> { "red", "blue" };

            Org2 = new TestOrg("Org2", 1F, Dept3);
            //Org2.Colors = new List<string> { "blue", "green" };

            Org3 = new TestOrg("Org3", 386.593F, Dept2);
            Org3.Colors = new List<string> { "yellow", "red", "green", "blue" };

            ListOrgs = new List<TestOrg> { Org1, Org2, Org3 };
            ListDepts = new List<TestDept> { Dept1, Dept2, Dept3 };
            ListStudents = new List<TestStudent> { Student1, Student2, Student3 };
            ListCourses = new List<TestCourse> { Course1, Course2, Course3 };
            
        }

        public List<TestOrg> GetAllOrgs()
        {
            return ListOrgs;
        }

        public List<TestDept> GetAllDepts()
        {
            return ListDepts;
        }

        public List<TestStudent> GetAllStudents()
        {
            return ListStudents;
        }

        public List<TestCourse> GetAllCourses()
        {
            return ListCourses;
        }

        //Methods to test filtering logic
        public TestCourse ExpectedResult_TestEqualOp()
        {
            return new TestCourse("Algorithms", 12645873358, true);
        }

        public List<TestCourse> ExpectedResult_TestGTOp()
        {
            List<TestCourse> listCourse = new List<TestCourse>();
            listCourse.Add(new TestCourse("Algorithms", 12645873358, true));
            listCourse.Add(new TestCourse("Database", 986245748145256, null));
            return listCourse;
        }

        public List<TestCourse> ExpectedResult_TestLTOp()
        {
            List<TestCourse> listCourse = new List<TestCourse>();
            listCourse.Add(new TestCourse("Algorithms", 12645873358, true));
            listCourse.Add(new TestCourse("Chemistry101", 6562456323, false));
            return listCourse;
        }

        public List<TestCourse> ExpectedResult_TestGTEOp()
        {
            List<TestCourse> listCourse = new List<TestCourse>();
            listCourse.Add(new TestCourse("Algorithms", 12645873358, true));
            listCourse.Add(new TestCourse("Database", 986245748145256, null));
            listCourse.Add(new TestCourse("Chemistry101", 6562456323, false));
            return listCourse;
        }

        public List<TestCourse> ExpectedResult_TestLTEOp()
        {
            List<TestCourse> listCourse = new List<TestCourse>();
            listCourse.Add(new TestCourse("Algorithms", 12645873358, true));
            listCourse.Add(new TestCourse("Chemistry101", 6562456323, false));
            return listCourse;
        }

        public List<TestCourse> ExpectedResult_TestNEOp()
        {
            List<TestCourse> listCourse = new List<TestCourse>();
            listCourse.Add(new TestCourse("Algorithms", 12645873358, true));
            listCourse.Add(new TestCourse("Chemistry101", 6562456323, false));
            return listCourse;
        }

        public List<TestCourse> ExpectedResult_TestOROp()
        {
            List<TestCourse> listCourse = new List<TestCourse>();
            listCourse.Add(new TestCourse("Algorithms", 12645873358, true));
            listCourse.Add(new TestCourse("Chemistry101", 6562456323, false));
            return listCourse;
        }

        public List<TestCourse> ExpectedResult_TestANDOp()
        {
            List<TestCourse> listCourse = new List<TestCourse>();
            listCourse.Add(new TestCourse("Database", 986245748145256, null));
            return listCourse;
        }

        public List<TestOrg> ExpectedResult_TestNestedPropWithNULLValues()
        {
            //List<TestOrg> listOrg = new List<TestOrg>();
            //TestCourse Course1 = new TestCourse("Algorithms", 12645873358, true);
            //TestStudent Student1 = new TestStudent("Chandrababu", new DateTime(1970, 12, 05), true, 001, 4.93m, Course1);
            //TestDept Dept2 = new TestDept("Dept2", 455, new DateTime(1928, 3, 5), 1.2012, Student1);
            //TestOrg Org3 = new TestOrg("Org3", 386.593F, Dept2);
            //listOrg.Add(Org3);
            //return listOrg;
            return new List<TestOrg> { Org3 };
        }

        public List<TestOrg> ExpectedResult_TestNestedPropWithMultipleValues()
        {
            return new List<TestOrg> { Org2 };
        }

        public List<TestOrg> ExpectedResult_TestNestedPropWithSingleValue()
        {
            return new List<TestOrg> { Org3 };
        }

        public List<TestOrg> ExpectedResult_TestDataMember()
        {
            return new List<TestOrg> { Org1 };
        }

        public List<TestOrg> ExpectedResult_TestAndOrOp()
        {
            return new List<TestOrg>{ Org2 };
        }

        public List<TestOrg> ExpectedList_MatchArrayExact()
        {
            return new List<TestOrg> { Org1 };
        }

        public List<TestOrg> ExpectedList_MatchArrayAll()
        {
            return new List<TestOrg> { Org1, Org2 };
           // return new List<TestOrg> { Org1, Org3 };
        }

        public List<TestOrg> ExpectedList_MatchArrayAll_Decimal()
        {
            return new List<TestOrg> { Org1 };
        }

        public List<TestOrg> ExpectedList_MatchArrayExact_Decimal()
        {
            return new List<TestOrg> { Org3 };
        }

        public List<TestOrg> ExpectedList_FindNullValues()
        {
            return new List<TestOrg> { Org3 };
        }

        public List<TestOrg> ExpectedList_FindEnum()
        {
            return new List<TestOrg> { Org1 };
        }

        public List<TestOrg> ExpectedList_FindEnumWithEnumMember()
        {
            return new List<TestOrg> { Org2, Org3 };
        }


        public List<TestOrg> ExpectedList_TestANDOpWithMultipleDuplicateKeysWithEqualityComparer()
        {
            return new List<TestOrg> { Org2 };
        }

        public List<TestOrg> ExpectedList_TestANDOpWithDuplicateArraysMatchAll()
        {
            return new List<TestOrg> { Org2};
        }
    }

    public class TestOrg
    {
        public string OrgName { get; set; }
        public float OrgID { get; set; }
        public TestDept Dept { get; set; }
        public List<string> Colors { get; set; }

        public TestOrg(string orgname, float orgid, TestDept dept)
        {
            OrgName = orgname;
            OrgID = orgid;
            Dept = dept;
        }
    }

    public class TestDept
    {
        [DataMember(Name = "Department")]
        public string DeptName { get; set; }
        public int? DeptID { get; set; }
        public DateTime? SomeDate { get; set; }
        public double DeptCode { get; set; }
        public TestStudent Student { get; set; }

        public TestDept(string deptname, int? deptid, DateTime? somedate, double deptcode, TestStudent student)
        {
            DeptName = deptname;
            DeptID = deptid;
            SomeDate = somedate;
            DeptCode = deptcode;
            Student = student;
        }
    }
    public class TestStudent
    {
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public bool Active { get; set; }
        public int ID { get; set; }
        public decimal? GPA { get; set; }
        public TestCourse Course { get; set; }
        public List<string> Hobbies { get; set; }
        public Country HomeCountry { get; set; }

        public TestStudent(string name, DateTime dob, bool active, int id, decimal gpa, TestCourse course)
        {
            Name = name;
            DOB = dob;
            Active = active;
            ID = id;
            GPA = gpa;
            Course = course;
        }
    }
    public class TestCourse
    {
        public string CourseName { get; set; }
        public long CourseID { get; set; }
        public bool? CourseActive { get; set; }
        public List<decimal> RelatedSubCourseIds { get; set; }

        public TestCourse(string coursename, long courseid, bool? courseactive)
        {
            CourseName = coursename;
            CourseID = courseid;
            CourseActive = courseactive;
        }
    }
  

    public enum Country
    {
        [EnumMember(Value = "united states")]
        USA,
        [EnumMember(Value = "uk")]
        UK,
        [EnumMember(Value = "uae")]
        UAE,
        [EnumMember(Value = "malaysia")]
        Malaysia,
        Singapore,
        [EnumMember(Value = "sweden")]
        Sweden
    }

}
