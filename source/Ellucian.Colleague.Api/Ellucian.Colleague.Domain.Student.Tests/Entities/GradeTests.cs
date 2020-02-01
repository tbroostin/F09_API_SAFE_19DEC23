// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class GradeTests
    {
        [TestClass]
        public class GradeConstructor
        {
            private string guid;
            private string id;
            private string letterGrade;
            private string credit;
            private string desc;
            private string schema;
            private Grade grade;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                id = "1";
                letterGrade = "A";
                credit = "4";
                desc = "Undergraduate";
                schema = "UG";
                grade = new Grade(guid, id, letterGrade, credit, desc, schema);
            }

            [TestMethod]
            public void GradeGuid()
            {
                Assert.AreEqual(guid, grade.Guid);
            }

            [TestMethod]
            public void GradeId()
            {
                Assert.AreEqual(id, grade.Id);
            }

            [TestMethod]
            public void GradeLetterGrade()
            {
                Assert.AreEqual(letterGrade, grade.LetterGrade);
            }

            [TestMethod]
            public void GradeCredit()
            {
                Assert.AreEqual(credit, grade.Credit);
            }

            [TestMethod]
            public void GradeCode()
            {
                Assert.AreEqual(schema, grade.GradeSchemeCode);
            }

            [TestMethod]
            public void GradeDescription()
            {
                Assert.AreEqual(desc, grade.Description);
            }

            [TestMethod]
            public void GradeExcludeFromFacultyGrading_DefaultValue()
            {
                Assert.IsFalse(grade.ExcludeFromFacultyGrading);
            }

            [TestMethod]
            public void Grade_IncludeInWebFinalGradesList_DefaultValue()
            {
                Assert.IsFalse(grade.IncludeInWebFinalGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebFinalGradesList_DefaultValue_2()
            {
                grade = new Grade(letterGrade, desc, schema);
                Assert.IsFalse(grade.IncludeInWebFinalGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebFinalGradesList_DefaultValue_3()
            {
                grade = new Grade(id, letterGrade, desc, schema);
                Assert.IsFalse(grade.IncludeInWebFinalGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebFinalGradesList_False()
            {
                grade = new Grade(id, letterGrade, desc, schema, false);
                Assert.IsFalse(grade.IncludeInWebFinalGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebFinalGradesList_False_2()
            {
                grade = new Grade(letterGrade, desc, schema, false);
                Assert.IsFalse(grade.IncludeInWebFinalGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebFinalGradesList_False_3()
            {
                grade = new Grade(guid, id, letterGrade, credit, desc, schema, false);
                Assert.IsFalse(grade.IncludeInWebFinalGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebFinalGradesList_True()
            {
                grade = new Grade(id, letterGrade, desc, schema, true);
                Assert.IsTrue(grade.IncludeInWebFinalGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebFinalGradesList_True_2()
            {
                grade = new Grade(letterGrade, desc, schema, true);
                Assert.IsTrue(grade.IncludeInWebFinalGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebFinalGradesList_True_3()
            {
                grade = new Grade(guid, id, letterGrade, credit, desc, schema, true);
                Assert.IsTrue(grade.IncludeInWebFinalGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebMidtermGradesList_DefaultValue()
            {
                Assert.IsFalse(grade.IncludeInWebMidtermGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebMidtermGradesList_DefaultValue_2()
            {
                grade = new Grade(letterGrade, desc, schema);
                Assert.IsFalse(grade.IncludeInWebMidtermGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebMidtermGradesList_DefaultValue_3()
            {
                grade = new Grade(id, letterGrade, desc, schema);
                Assert.IsFalse(grade.IncludeInWebMidtermGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebMidtermGradesList_False()
            {
                grade = new Grade(id, letterGrade, desc, schema, true, false);
                Assert.IsFalse(grade.IncludeInWebMidtermGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebMidtermGradesList_False_2()
            {
                grade = new Grade(letterGrade, desc, schema, true, false);
                Assert.IsFalse(grade.IncludeInWebMidtermGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebMidtermGradesList_False_3()
            {
                grade = new Grade(guid, id, letterGrade, credit, desc, schema, true, false);
                Assert.IsFalse(grade.IncludeInWebMidtermGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebMidtermGradesList_True()
            {
                grade = new Grade(id, letterGrade, desc, schema, false, true);
                Assert.IsTrue(grade.IncludeInWebMidtermGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebMidtermGradesList_True_2()
            {
                grade = new Grade(letterGrade, desc, schema, false, true);
                Assert.IsTrue(grade.IncludeInWebMidtermGradesList);
            }

            [TestMethod]
            public void Grade_IncludeInWebMidtermGradesList_True_3()
            {
                grade = new Grade(guid, id, letterGrade, credit, desc, schema, false, true);
                Assert.IsTrue(grade.IncludeInWebMidtermGradesList);
            }

            [TestMethod]
            public void Grade_CanBeUsedAfterDropGradeRequiredDate_DefaultValue()
            {
                Assert.IsFalse(grade.CanBeUsedAfterDropGradeRequiredDate);
            }

            [TestMethod]
            public void Grade_CanBeUsedAfterDropGradeRequiredDate_DefaultValue_2()
            {
                grade = new Grade(letterGrade, desc, schema);
                Assert.IsFalse(grade.CanBeUsedAfterDropGradeRequiredDate);
            }

            [TestMethod]
            public void Grade_CanBeUsedAfterDropGradeRequiredDate_DefaultValue_3()
            {
                grade = new Grade(id, letterGrade, desc, schema);
                Assert.IsFalse(grade.CanBeUsedAfterDropGradeRequiredDate);
            }

            [TestMethod]
            public void Grade_CanBeUsedAfterDropGradeRequiredDate_False()
            {
                grade = new Grade(id, letterGrade, desc, schema, true, true, false);
                Assert.IsFalse(grade.CanBeUsedAfterDropGradeRequiredDate);
            }

            [TestMethod]
            public void Grade_CanBeUsedAfterDropGradeRequiredDate_False_2()
            {
                grade = new Grade(letterGrade, desc, schema, true, true, false);
                Assert.IsFalse(grade.CanBeUsedAfterDropGradeRequiredDate);
            }


            [TestMethod]
            public void Grade_CanBeUsedAfterDropGradeRequiredDate_False_3()
            {
                grade = new Grade(guid, id, letterGrade, credit, desc, schema, true, true, false);
                Assert.IsFalse(grade.CanBeUsedAfterDropGradeRequiredDate);
            }

            [TestMethod]
            public void Grade_CanBeUsedAfterDropGradeRequiredDate_True()
            {
                grade = new Grade(id, letterGrade, desc, schema, false, false, true);
                Assert.IsTrue(grade.CanBeUsedAfterDropGradeRequiredDate);
            }

            [TestMethod]
            public void Grade_CanBeUsedAfterDropGradeRequiredDate_True_2()
            {
                grade = new Grade(letterGrade, desc, schema, false, false, true);
                Assert.IsTrue(grade.CanBeUsedAfterDropGradeRequiredDate);
            }

            [TestMethod]
            public void Grade_CanBeUsedAfterDropGradeRequiredDate_True_3()
            {
                grade = new Grade(guid, id, letterGrade, credit, desc, schema, false, false, true);
                Assert.IsTrue(grade.CanBeUsedAfterDropGradeRequiredDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeGuidNullException()
            {
                new Grade(null, id, letterGrade, credit, desc, schema);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeIdNullException()
            {
                new Grade(guid, null, letterGrade, credit, desc, schema);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeCodeNullException()
            {
                new Grade(guid, id, letterGrade, credit, desc, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeDescNullException()
            {
                new Grade(guid, id, letterGrade, credit, null, schema);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeLetterGradeNullException()
            {
                new Grade(guid, id, null, credit, desc, schema);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeGuidEmptyException()
            {
                new Grade(string.Empty, id, letterGrade, credit, desc, schema);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeIdEmptyException()
            {
                new Grade(guid, string.Empty, letterGrade, credit, desc, schema);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeDescEmptyException()
            {
                new Grade(guid, id, letterGrade, credit, string.Empty, schema);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeLetterGradeEmptyException()
            {
                new Grade(guid, id, string.Empty, credit, desc, schema);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void GradeId2()
            {
                Grade grade = new Grade("2", letterGrade, desc, schema);
                grade.Id = "1";
            }

            [TestMethod]
            public void GradeIdNull2Exception()
            {
                Grade grade = new Grade(null, letterGrade, desc, schema);
                grade.Id = "1";
            }


        }

        [TestClass]
        public class ComparisonGradeTests
        {
            // Various Grade objects for use in the tests below. Populated in Initialize
            private Grade gradeCMPVal1;
            private Grade gradeCMPVal4;
            private Grade gradeCMPVal5;

            private Grade gradeUGVal2NoComp;
            private Grade gradeUGVal3CompVal1;
            private Grade gradeUGVal2CompVal5;

            private Grade gradeGRVal3NoComp;
            private Grade gradeGRVal3CompVal5;
            private Grade gradeGRVal5CompVal4;
            private Grade gradeGRVal6CompVal4;

            [TestInitialize]
            public void Initialize()
            {
                // Setup various Grade objects for use in the tests below

                // The CMP grade scheme is the comparison grade scheme. There are also objects
                // from a UG and a GR grade scheme.

                gradeCMPVal1 = new Grade("CMP1", "V1", "Val1 in CMP scheme", "CMP");
                gradeCMPVal1.GradeValue = 1;

                gradeCMPVal4 = new Grade("CMP4", "V4", "Val4 in CMP scheme", "CMP");
                gradeCMPVal4.GradeValue = 4;

                gradeCMPVal5 = new Grade("CMP5", "V5", "Val5 in CMP scheme", "CMP");
                gradeCMPVal5.GradeValue = 5;

                gradeUGVal2NoComp = new Grade("UG2", "V2", "Val 2 in UG scheme", "UG");
                gradeUGVal2NoComp.GradeValue = 2;

                gradeUGVal3CompVal1 = new Grade("UG3C", "V3C", "Val 3 in UG scheme with V1 comp", "UG");
                gradeUGVal3CompVal1.GradeValue = 3;
                gradeUGVal3CompVal1.SetComparisonGrade("CMP1", 1, "CMP");

                gradeUGVal2CompVal5 = new Grade("UG2C", "V2C", "Val 2 in UG scheme with V5 comp", "UG");
                gradeUGVal2CompVal5.GradeValue = 2;
                gradeUGVal2CompVal5.SetComparisonGrade("CMP5", 5, "CMP");

                gradeGRVal3NoComp = new Grade("GR3", "V3", "Val 3 in GR scheme", "GR");
                gradeGRVal3NoComp.GradeValue = 3;

                gradeGRVal3CompVal5 = new Grade("GR3C", "V3C", "Val 3 in GR scheme with V5 comp", "GR");
                gradeGRVal3CompVal5.GradeValue = 3;
                gradeGRVal3CompVal5.SetComparisonGrade("CMP5", 5, "CMP");

                gradeGRVal6CompVal4 = new Grade("GR6C", "V6C", "Val 6 in GR scheme with V4 comp", "GR");
                gradeGRVal6CompVal4.GradeValue = 6;
                gradeGRVal6CompVal4.SetComparisonGrade("CMP4", 4, "CMP");

                gradeGRVal5CompVal4 = new Grade("GR5C", "V5C", "Val 5 in GR scheme with V4 comp", "GR");
                gradeGRVal5CompVal4.GradeValue = 5;
                gradeGRVal5CompVal4.SetComparisonGrade("CMP4", 4, "CMP");

            }

            /// <summary>
            /// Given two Grade objects from the same scheme with comparison grades
            /// When the two objects are compared by grade value
            /// Then the comparsion grades are not used
            /// </summary>
            [TestMethod]
            public void ComparisonGradeIgnoredInValueWhenSameSchemes()
            {
                // The objects are setup so that only the base value of the first grade is >= the base value
                // of the second grade. Any other comparison between the two (base of first against comparison of second, etc.) 
                // will not be >=
                Assert.IsTrue(gradeUGVal3CompVal1.IsGreaterOrEqualUsingComparisonGrade(gradeUGVal2CompVal5));
            }

            /// <summary>
            /// Given two Grade objects from different schemes with comparison grades
            /// When the two objects are compared by grade value
            /// Then the comparsion grades are used
            /// </summary>
            [TestMethod]
            public void ComparisonGradeUsedInValueWhenDifferentSchemes()
            {
                // The objects are setup so that only the comparision value of the first grade is >= the comparison value
                // of the second grade. Any other comparison between the two (base of first against comparison of second, etc.) 
                // will not be >=
                Assert.IsTrue(gradeUGVal2CompVal5.IsGreaterOrEqualUsingComparisonGrade(gradeGRVal6CompVal4));
            }

            /// <summary>
            /// Given two Grade objects from different schemes with comparison grades, one has a comparison grade and the other does not
            /// When the two objects are compared by grade value
            /// Then the comparison grade that is present is compared to the non-comparison grade of the other.
            /// </summary>
            [TestMethod]
            public void ComparisonGradeAgainstRegularValueWhenDifferentSchemes()
            {
                // The objects are setup so that only the comparision value of the first grade is >= the base value
                // of the second grade. The base grade of the first is not >= the base value of the second grade.
                Assert.IsTrue(gradeUGVal2CompVal5.IsGreaterOrEqualUsingComparisonGrade(gradeGRVal3NoComp));
            }

            /// <summary>
            /// Given two different Grade objects from different schemes scheme, they are considered equal when they
            /// have the same comparison grade.
            /// </summary>
            [TestMethod]
            public void ComparisonGradeUsedInEqualityWhenDifferentSchemes()
            {
                Assert.IsTrue(gradeUGVal2CompVal5.IsEquivalentUsingComparisonGrade(gradeGRVal3CompVal5));
            }

            /// <summary>
            /// Given two different Grade objects from the same scheme, but each with the same comparison grade
            /// When the two objects are compared for grade equality
            /// Then the comparison grades are ignored and they are found not equal.
            /// </summary>
            [TestMethod]
            public void ComparisonGradeIgnoredInEqualityWhenSamechemes()
            {
                Assert.IsFalse(gradeGRVal5CompVal4.IsEquivalentUsingComparisonGrade(gradeGRVal6CompVal4));
            }

            /// <summary>
            /// Given two grades with no comparison garde
            /// When the value of the first is greater than the second
            /// Then the >= comparison will pass.
            /// </summary>
            [TestMethod]
            public void GEComparisonPassesWithNoComparisonGrades()
            {
                Assert.IsTrue(gradeGRVal3NoComp.IsGreaterOrEqualUsingComparisonGrade(gradeUGVal2NoComp));
            }

            /// <summary>
            /// Given two grades with no comparison garde
            /// When the value of the first is less than the second
            /// Then the >= comparison will fail.
            /// </summary>
            [TestMethod]
            public void GEComparisonFailsWithNoComparisonGrades()
            {
                Assert.IsFalse(gradeUGVal2NoComp.IsGreaterOrEqualUsingComparisonGrade(gradeGRVal3NoComp));
            }

            /// <summary>
            /// Given two grades with no comparison garde
            /// When the grade ID of the first is the same as that of the second
            /// Then the equal grade comparison will pass
            /// </summary>
            [TestMethod]
            public void EqualGradeComparisonPassesWithNoComparisonGrades()
            {
                Assert.IsTrue(gradeGRVal3NoComp.IsEquivalentUsingComparisonGrade(gradeGRVal3NoComp));
            }

            /// <summary>
            /// Given two grades with no comparison garde
            /// When the grade ID of the first is different from that of the second
            /// Then the equal grade comparison will fail
            /// </summary>
            [TestMethod]
            public void EqualGradeComparisonFailsWithNoComparisonGrades()
            {
                Assert.IsFalse(gradeGRVal3NoComp.IsEquivalentUsingComparisonGrade(gradeUGVal2NoComp));
            }

            /// <summary>
            /// Tests various scenarios of null grade values.
            /// The comparison of null values should produce the same result as the comparison
            /// of blank values in Envision, so that My Progress Min Grade comparisons are equivalent
            /// to the same in Envision EVAL.
            /// </summary>
            [TestMethod]
            public void ComparisonOfNullGradeValues()
            {
                Grade grdNullValue = new Grade("A", "An A", "UG");
                grdNullValue.GradeValue = null;
                Grade grdWithValue = new Grade("B", "A B", "UG");
                grdWithValue.GradeValue = 3;

                // A null is >= another null
                Assert.IsTrue(grdNullValue.IsGreaterOrEqualUsingComparisonGrade(grdNullValue));

                // A value is always >= a null
                Assert.IsTrue(grdWithValue.IsGreaterOrEqualUsingComparisonGrade(grdNullValue));

                // A null is always < a value (separate test, because separate code path)
                Assert.IsFalse(grdNullValue.IsGreaterOrEqualUsingComparisonGrade(grdWithValue));

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ComparisonGradeCannotHaveSameGradeScheme()
            {
                Grade grdInUGScheme = new Grade("A", "An A", "UG");
                grdInUGScheme.SetComparisonGrade("B", 2.0M, "UG");
            }



        }        
    }

    [TestClass]
    public class OtherTests
    {
        /// <summary>
        /// Very that the DeepCopy method copies all properties.
        /// </summary>
        [TestMethod]
        public void DeepCopyTest()
        {
            // Create a Grade object with all properties set
            Grade srcGrade = new Grade("d874e05d - 9d97 - 4fa3 - 8862 - 5044ef2384d0", "GD_ID", "A", "Y", "An A Grade", "UG");
            srcGrade.SetComparisonGrade("X", 2M, "GR");
            srcGrade.ExcludeFromFacultyGrading = true;
            srcGrade.GradePriority = 1M;
            srcGrade.GradeValue = 32.0M;
            srcGrade.IncompleteGrade = "F";
            srcGrade.IsWithdraw = false;
            srcGrade.RequireLastAttendanceDate = false;

            Grade newGrade = srcGrade.DeepCopy();

            Assert.AreEqual(srcGrade.ComparisonGrade.ComparisonGradeId, newGrade.ComparisonGrade.ComparisonGradeId);
            Assert.AreEqual(srcGrade.ComparisonGrade.ComparisonGradeSchemeCode, newGrade.ComparisonGrade.ComparisonGradeSchemeCode);
            Assert.AreEqual(srcGrade.ComparisonGrade.ComparisonGradeValue, newGrade.ComparisonGrade.ComparisonGradeValue);
            Assert.AreEqual(srcGrade.Credit, newGrade.Credit);
            Assert.AreEqual(srcGrade.Description, newGrade.Description);
            Assert.AreEqual(srcGrade.ExcludeFromFacultyGrading, newGrade.ExcludeFromFacultyGrading);
            Assert.AreEqual(srcGrade.GradePriority, newGrade.GradePriority);
            Assert.AreEqual(srcGrade.GradeSchemeCode, newGrade.GradeSchemeCode);
            Assert.AreEqual(srcGrade.GradeValue, newGrade.GradeValue);
            Assert.AreEqual(srcGrade.Guid, newGrade.Guid);
            Assert.AreEqual(srcGrade.Id, newGrade.Id);
            Assert.AreEqual(srcGrade.IncompleteGrade, newGrade.IncompleteGrade);
            Assert.AreEqual(srcGrade.IsWithdraw, newGrade.IsWithdraw);
            Assert.AreEqual(srcGrade.LetterGrade, newGrade.LetterGrade);
            Assert.AreEqual(srcGrade.RequireLastAttendanceDate, newGrade.RequireLastAttendanceDate);
        }

    }

}
