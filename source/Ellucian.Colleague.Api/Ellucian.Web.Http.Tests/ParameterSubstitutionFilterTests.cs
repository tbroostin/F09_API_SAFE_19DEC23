// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Ellucian.Web.Http.Tests
{
    [TestClass]
    public class ParameterSubstitutionFilterTests
    {
        [TestMethod]
        public void FilterWithNoSubstitution()
        {
            string arg1 = "stringone";
            string arg2 = "stringtwo";
            int arg3 = 1;

            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("one-string", arg1);
            arguments.Add("two-string", arg2);
            arguments.Add("three-int", arg3);

            HttpActionContext httpActionContext = new HttpActionContext();
            foreach (KeyValuePair<string, object> argument in arguments)
            {
                httpActionContext.ActionArguments[argument.Key] = argument.Value;
            }

            ParameterSubstitutionFilter filter = new ParameterSubstitutionFilter();
            filter.OnActionExecuting(httpActionContext);

            Assert.AreEqual(arg1, httpActionContext.ActionArguments["one-string"]);
            Assert.AreEqual(arg2, httpActionContext.ActionArguments["two-string"]);
            Assert.AreEqual(arg3, httpActionContext.ActionArguments["three-int"]);
        }


        [TestMethod]
        public void FilterWithSubstitution_AllArgs()
        {
            string arg1 = "string-forwardslash_char-one";
            string arg1Expected = "string/one";
            string arg2 = "string-colon_char-two";
            string arg2Expected = "string:two";
            int arg3 = 1;

            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("one-string", arg1);
            arguments.Add("two-string", arg2);
            arguments.Add("three-int", arg3);

            HttpActionContext httpActionContext = new HttpActionContext();
            foreach (KeyValuePair<string, object> argument in arguments)
            {
                httpActionContext.ActionArguments[argument.Key] = argument.Value;
            }

            ParameterSubstitutionFilter filter = new ParameterSubstitutionFilter();
            filter.OnActionExecuting(httpActionContext);

            Assert.AreEqual(arg1Expected, httpActionContext.ActionArguments["one-string"]);
            Assert.AreEqual(arg2Expected, httpActionContext.ActionArguments["two-string"]);
            Assert.AreEqual(arg3, httpActionContext.ActionArguments["three-int"]);
        }


        [TestMethod]
        public void FilterWithSubstitution_OneArg()
        {
            string arg1 = "string-forwardslash_char-one";
            string arg1Expected = "string/one";
            string arg2 = "string-colon_char-two";
            string arg2Expected = arg2;
            int arg3 = 1;

            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("one-string", arg1);
            arguments.Add("two-string", arg2);
            arguments.Add("three-int", arg3);

            HttpActionContext httpActionContext = new HttpActionContext();
            foreach (KeyValuePair<string, object> argument in arguments)
            {
                httpActionContext.ActionArguments[argument.Key] = argument.Value;
            }

            ParameterSubstitutionFilter filter = new ParameterSubstitutionFilter();
            filter.ParameterNames = new string[] { "one-string" };
            filter.OnActionExecuting(httpActionContext);

            Assert.AreEqual(arg1Expected, httpActionContext.ActionArguments["one-string"]);
            Assert.AreEqual(arg2Expected, httpActionContext.ActionArguments["two-string"]);
            Assert.AreEqual(arg3, httpActionContext.ActionArguments["three-int"]);
        }

        [TestMethod]
        public void FilterWithSubstitution_AllArgsSpecified()
        {
            string arg1 = "string-forwardslash_char-one";
            string arg1Expected = "string/one";
            string arg2 = "string-colon_char-two";
            string arg2Expected = "string:two";
            int arg3 = 1;

            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add("one-string", arg1);
            arguments.Add("two-string", arg2);
            arguments.Add("three-int", arg3);

            HttpActionContext httpActionContext = new HttpActionContext();
            foreach (KeyValuePair<string, object> argument in arguments)
            {
                httpActionContext.ActionArguments[argument.Key] = argument.Value;
            }

            ParameterSubstitutionFilter filter = new ParameterSubstitutionFilter();
            filter.ParameterNames = new string[] { "one-string", "two-string", "three-int" };
            filter.OnActionExecuting(httpActionContext);

            Assert.AreEqual(arg1Expected, httpActionContext.ActionArguments["one-string"]);
            Assert.AreEqual(arg2Expected, httpActionContext.ActionArguments["two-string"]);
            Assert.AreEqual(arg3, httpActionContext.ActionArguments["three-int"]);
        }

    }
}
