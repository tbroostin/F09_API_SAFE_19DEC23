// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.Student.Tests
{
    public static class TestStudentCourseSecRepository
    {
        private static Collection<StudentCourseSec> _studentCourseSecs = new Collection<StudentCourseSec>();
        public static Collection<StudentCourseSec> StudentCourseSecs
        {
            get
            {
                if (_studentCourseSecs.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _studentCourseSecs;
            }
        }

        private static void GenerateDataContracts()
        {
            string[,] studentCourseSecData = GetStudentCourseSecData();
            int regApprovalsCount = studentCourseSecData.Length / 3;
            for (int i = 0; i < regApprovalsCount; i++)
            {
                string id = studentCourseSecData[i, 0].Trim();
                string courseSec = studentCourseSecData[i, 1].Trim();
                string acadCred = studentCourseSecData[i, 2].Trim();
                StudentCourseSec scs = new StudentCourseSec()
                {
                    Recordkey = id,
                    ScsCourseSection = courseSec,
                    ScsStudentAcadCred = acadCred,
                };
                // Added for testing attendance hours/minutes
                if (id == "16000")
                {
                    scs.ScsAttendanceEntityAssociation = new List<StudentCourseSecScsAttendance>()
                    {
                        new StudentCourseSecScsAttendance()
                        {
                            ScsAttendanceMinutesAssocMember = 60,
                            ScsAttendanceStartTimesAssocMember = DateTime.Today.AddDays(-7).AddHours(2),
                            ScsAttendanceEndTimesAssocMember = DateTime.Today.AddDays(-7).AddHours(3),
                            ScsAttendanceInstrMethodsAssocMember = "LEC",
                            ScsAbsentDatesAssocMember = DateTime.Today.AddDays(-7),
                            ScsAttendanceReasonAssocMember = "Because...",
                        }
                    };
                }
                _studentCourseSecs.Add(scs);
            }
        }
        private static string[,] GetStudentCourseSecData()
        {
            string[,] scsData = {
                                                // ID   ScsCourseSection  StuAcadCred
                                                {"12221","16234","12570"},
                                                {"13509","15186","13951"},
                                                {"13510","15393","13952"},
                                                {"13511","15516","13953"},
                                                {"13512","15720","13954"},
                                                {"13623","15633","14066"},
                                                {"13624","15714","14067"},
                                                {"13632","15532","14075"},
                                                {"11660","16896","11969"},
                                                {"11661","16768","11970"},
                                                {"11715","15003","12037"},
                                                {"11717","15006","12039"},
                                                {"11721","15003","12043"},
                                                {"11722","15131","12044"},
                                                {"11736","15003","12058"},
                                                {"11740","15150","12062"},
                                                {"11745","15066","12067"},
                                                {"11755","14875","12077"},
                                                {"11758","16804","12080"},
                                                {"11831","16047","12155"},
                                                {"11833","16428","12157"},
                                                {"14336","15190","14779"},
                                                {"14337","15188","14780"},
                                                {"14818","15544","15263"},
                                                {"15009","15636","15457"},
                                                // Added for testing attendance hours/minutes
                                                {"16000","16000","16000"}
                                            };
            return scsData;
        }
    }
}
