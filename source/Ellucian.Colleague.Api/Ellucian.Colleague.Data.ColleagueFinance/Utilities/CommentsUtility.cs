// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Dmi.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Data.ColleagueFinance.Utilities
{
    /// <summary>
    /// Conversion of Colleague Comments into paragraphs.
    /// </summary>
    public class CommentsUtility
    {
        /// <summary>
        /// Converts Colleague comment data into C sharp formatted string data.
        /// </summary>
        /// <param name="source">the paragraph data from Colleague - string with value marks</param>
        /// <returns></returns>
        public static string ConvertCommentsToParagraphs(string source)
        {
            var replace = source;
            if (!string.IsNullOrEmpty(source))
            {
                char _vm = Convert.ToChar(DynamicArray.VM);
                string paragraphSpacing = "" + Environment.NewLine;
                paragraphSpacing += Environment.NewLine;
                //first replace two _vms with a new line, then replace remaining single vms with a space
                replace = source.Replace("" + _vm + _vm, paragraphSpacing).Replace(_vm, ' ');
            }
            return replace;
        }

        /// <summary>
        /// Converts search string into multiple name types based on delimeiter.
        /// </summary>
        /// <param name="searchKey"></param>
        /// <returns>returns list of strings(lastName,FirstName,MiddleName) respectively </returns>
        public static List<string> FormatStringToNames(string searchKey)
        {
            List<string> names = new List<string>();

            string lastName = null;
            string firstName = null;
            string middleName = null;
            // Regular expression for all punctuation and numbers to remove from name string
            Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
            Regex regexNotSpace = new Regex(@"\s");

            var nameStrings = searchKey.Split(',');
            // If there was a comma, set the first item to last name
            if (nameStrings.Count() > 1)
            {
                lastName = nameStrings.ElementAt(0).Trim();
                if (nameStrings.Count() >= 2)
                {
                    // parse the two items after the comma using a space. Ignore anything else
                    var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                    if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                    if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                }
            }
            else
            {
                // Parse entry using spaces, assume entered (last) or (first last) or (first middle last). 
                // Blank values don't hurt anything.
                nameStrings = searchKey.Split(' ');
                switch (nameStrings.Count())
                {
                    case 1:
                        lastName = nameStrings.ElementAt(0).Trim();
                        break;
                    case 2:
                        firstName = nameStrings.ElementAt(0).Trim();
                        lastName = nameStrings.ElementAt(1).Trim();
                        break;
                    default:
                        firstName = nameStrings.ElementAt(0).Trim();
                        middleName = nameStrings.ElementAt(1).Trim();
                        lastName = nameStrings.ElementAt(2).Trim();
                        break;
                }
            }
            // Remove characters that won't make sense for each name part, including all punctuation and numbers 
            if (lastName != null)
            {
                lastName = regexNotPunc.Replace(lastName, "");
                lastName = regexNotSpace.Replace(lastName, "");
            }
            else lastName = "";
            if (firstName != null)
            {
                firstName = regexNotPunc.Replace(firstName, "");
                firstName = regexNotSpace.Replace(firstName, "");
            }
            else firstName = "";
            if (middleName != null)
            {
                middleName = regexNotPunc.Replace(middleName, "");
                middleName = regexNotSpace.Replace(middleName, "");
            }
            else middleName = "";

            names.Add(lastName);
            names.Add(firstName);
            names.Add(middleName);

            return names;
        }

        public static List<string> ConvertMultiLineTextToList(string source)
        {
            List<string> stringList = new List<string>();
            if (!string.IsNullOrEmpty(source))
            {
                // We may have line break characters in the existing comments. Split them out and add each line separately
                // to preserve any line-to-line formatting the user entered. Note that these characters could be
                // \n or \r\n (two variations of a new line character) or \r (a carriage return). We will change
                // any of the new line or carriage returns to the same thing, and then split the string on that.
                stringList = source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
            }

            return stringList;
        }
    }
}
