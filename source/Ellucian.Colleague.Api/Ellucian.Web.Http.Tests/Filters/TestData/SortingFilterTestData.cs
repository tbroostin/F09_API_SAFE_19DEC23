// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Ellucian.Web.Http.Tests.Filters.TestData
{
    public class SortingFilterTestData
    {
        public List<Org> listOrgs{get;set;}
        public List<Dept> listDepts { get; set; }
        public List<Student> listStudents { get; set; }
        public List<Course> listCourses { get; set; }
        Org org1;
        Org org2;
        Org org3;
        Org org4;

        public SortingFilterTestData()
        {
            Course course1 = new Course("Algorithms", 123456789, true); 
            Course course2 = new Course("Database", 856452158, null); 
            Course course3 = new Course("Chemistry101", 895656232, false); 
            Course course4 = new Course("Database", 856452158, true); 

            Student student1 = new Student("Chandrababu", new DateTime(1970, 12, 05), true, 001, 4.93m, course1);
            student1.AvgScore = 98.3f;
            student1.Passed = true;

            Student student2 = new Student("Nicki", new DateTime(1990, 07, 3), true, 300, 3.25m, course3);
            student2.AvgScore = -86.23f;

            Student student3 = new Student("Jane Doe", new DateTime(1978, 05, 18), false, 201, 2.15m, course2);
            student3.AvgScore = 45.32f;
            student3.Passed = false;

            Student student4 = new Student("Mary", new DateTime(1990, 07, 3), true, 359, 3.25m, course4);

            Dept dept1 = new Dept("Dept1", null, null, 1010, student2); 
            dept1.SubDept = new SubDept { SubDeptName = "SubDept1" }; 

            Dept dept2 = new Dept("Dept2", 455, new DateTime(1928, 3, 5), 1.2012, student4);
            dept2.SubDept = new SubDept { SubDeptName = "SubDept2" }; 

            Dept dept3 = new Dept("Dept3", 78, DateTime.Now, 192.8, student3); 
            Dept dept4 = new Dept("Dept4", 101, DateTime.Now, 12.323, student1); 
            dept4.SubDept = new SubDept { SubDeptName = "SubDept4" }; 

            org1 = new Org("Org1", 2.666F, dept1);
            org2 = new Org("Org2", 1F, dept3);
            org3 = new Org("Org3", 386.593F, dept2);
            org4 = new Org(null, 26.3F, dept4);

            listOrgs = new List<Org> { org1, org2, org3, org4 };
            listDepts = new List<Dept> { dept1, dept2, dept3 };
            listStudents = new List<Student> { student1, student2, student3, student4 };
            listCourses = new List<Course> { course1, course2, course3 };

        } 
      
        public List<Org> GetAllOrgs()
        {
            return listOrgs;
        }

        public List<Dept> GetAllDepts()
        {
            return listDepts;
        }

        public List<Student> GetAllStudents()
        {
            return listStudents;
        }

        public List<Course> GetAllCourses()
        {
            return listCourses;
        }

      

        //--Methods for testing sorting:
        public List<Org> GetOrgs_SortedBy_OrgName()
        {
            List<Org> expectedList = new List<Org> { org4, org1, org2, org3 };
            return expectedList;
        }

        public List<Org> GetOrgs_SortedBy_OrgNameDesc()
        {
            List<Org> expectedList = new List<Org> { org3, org2, org1, org4 };
            return expectedList;

        }

        public List<Org> GetOrgs_SortedBy_CourseIdDesc()
        {
            List<Org> expectedList = new List<Org> { org1, org2, org3, org4 };
            return expectedList;
        }


        public List<Org> GetOrgs_SortedBy_JsonCourseId_CourseNameDesc_CourseActive()
        {
         
            List<Org> expectedList = new List<Org> { org4, org2, org3, org1 };
            return expectedList;
        }

        public List<Org> GetOrgs_SortedBy_StudentAvgScoreDesc()
        {
            List<Org> expectedList = new List<Org> { org4, org2, org3, org1 };
            return expectedList;
        }

        public List<Org> GetOrgs_SortedBy_SubDeptNameDesc()
        {
            List<Org> expectedList = new List<Org> { org4, org3, org1, org2 };
            return expectedList;
        }

        public List<Org> GetOrgs_SortedBy_DeptDateDesc_DeptIdDesc()
        {
            List<Org> expectedList = new List<Org> { org4, org2, org3, org1 };
            return expectedList;
        }

        public List<Org> GetOrgs_SortedBy_StudentDOBDesc_StudentNameDesc()
        {
            List<Org> expectedList = new List<Org> { org1, org3, org2,  org4 };
            return expectedList;
        }

    }

    public class Org
    {
        public string OrgName { get; set; }
        public float OrgID { get; set; }
        public Dept Dept { get; set; }

        public Org(string orgname, float orgid, Dept dept)
        {
            OrgName = orgname;
            OrgID = orgid;
            Dept = dept;
        }
    }

    public class SubDept
    {
        public String SubDeptName { get; set; }
    }

    public class Dept
    {
        public string DeptName { get; set; }
        public int? DeptID { get; set; }
        public DateTime? SomeDate { get; set; }
        public double DeptCode { get; set; }
        public Student Student { get; set; }
        public SubDept SubDept { get; set; }

        public Dept(string deptname, int? deptid, DateTime? somedate, double deptcode, Student student)
        {
            DeptName = deptname;
            DeptID = deptid;
            SomeDate = somedate;
            DeptCode = deptcode;
            Student = student;
        }
    }

   // [DataContract(Name="StudentClass")]
    public class Student
    {
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public bool Active { get; set; }
        public int ID { get; set; }
        public decimal? GPA { get; set; }
        public Course Course { get; set; }

        [DataMember (Name="fieldAvgScore")]
        public float AvgScore;
        public bool Passed;

        public Student(string name, DateTime dob, bool active, int id, decimal gpa, Course course)
        {
            Name = name;
            DOB = dob;
            Active = active;
            ID = id;
            GPA = gpa;
            Course = course;
        }
    }
    public class Course
    {
        public string CourseName { get; set; }
        [JsonProperty (PropertyName="jsonCourseId")]
        public long CourseID { get; set; }
        public bool? CourseActive { get; set; }

        public Course(string coursename, long courseid, bool? courseactive)
        {
            CourseName = coursename;
            CourseID = courseid;
            CourseActive = courseactive;
        }
    }
  

}
