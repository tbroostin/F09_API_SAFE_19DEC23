using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class TextDocumentTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TextDocument_Constructor_NullText()
        {
            var result = new TextDocument(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TextDocument_Constructor_NoText()
        {
            var text = new List<string>();
            var result = new TextDocument(text);
        }

        [TestMethod]
        public void TextDocument_Constructor_ValidText()
        {
            string text1 = "This is a line of text.";
            string text2 = "This is another line of text.";
            var text = new List<string>();
            text.Add(text1);
            text.Add(text2);
            var result = new TextDocument(text);

            Assert.IsNotNull(result.Text);
            Assert.AreEqual(text.Count, result.Text.Count);
            CollectionAssert.AreEqual(text, result.Text);
        }
    }
}
