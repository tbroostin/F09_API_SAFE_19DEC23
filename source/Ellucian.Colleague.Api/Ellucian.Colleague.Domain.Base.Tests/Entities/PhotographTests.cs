// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PhotographTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Photograph_Constructor_NullStream()
        {
            var photo = new Photograph(null, "application/octet-stream");
        }

        [TestMethod]
        public void Photograph_Constructor_PhotoStream()
        {
            string s = "abcd";
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            var photo = new Photograph(stream, "JPG");
            Assert.AreEqual(stream, photo.PhotoStream);
        }

        [TestMethod]
        public void Photograph_Constructor_ContentType()
        {
            string s = "abcd";
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            var photo = new Photograph(stream, "JPG");
            Assert.AreEqual("JPG", photo.ContentType);
        }
    }
}
