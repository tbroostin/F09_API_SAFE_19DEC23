// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Dmi.Runtime;
using System;

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
    }
}
