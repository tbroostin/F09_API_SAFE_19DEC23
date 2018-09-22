using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RuleTests
    {
        [TestMethod]
        public void Passes()
        {
            Department context = new TestDepartmentRepository().Get().FirstOrDefault(dd => dd.Code == "BIOL");

            ParameterExpression deptParam = Expression.Parameter(typeof(Department), "d");
            Expression lhs = Expression.Property(deptParam, "Code");
            Expression rhs = Expression.Constant("BIOL");
            Expression eq = Expression.Equal(lhs, rhs);

            var lambda = Expression.Lambda<Func<Department, bool>>(eq, deptParam);
            var del = lambda.Compile();

            Assert.IsTrue(del.Invoke(context));
            Assert.IsFalse(del.Invoke(new TestDepartmentRepository().Get().FirstOrDefault(dd => dd.Code == "MATH")));
        }
    }
}
