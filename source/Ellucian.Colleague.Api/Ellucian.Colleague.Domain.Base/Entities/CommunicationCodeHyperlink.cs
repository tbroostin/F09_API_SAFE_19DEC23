/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class CommunicationCodeHyperlink
    {
        /// <summary>
        /// The URL of the hyperlink. This is a required attribute
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if URL is set to null or empty</exception>
        public string Url { get { return url; }
            set
            {
                if (string.IsNullOrEmpty(value))
                { 
                    throw new ArgumentException("Url is required. It cannot be null or empty");
                }
                url = value;
            }
        }
        private string url;

        /// <summary>
        /// The Title of the hyperlink. This is what will be displayed to end users. This is a required attribute.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the Title is set to null or empty.</exception>
        public string Title { get { return title; }
            set
            { 
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Title is required. It cannot be null or empty");
                }
                title = value;
            }
        }
        private string title;

        /// <summary>
        /// Create a hyperlink object for a communication code
        /// </summary>
        /// <param name="url">Required: The URL of the hyperlink.</param>
        /// <param name="title">Required: The Title of the hyperlink. This is what will be displayed to end users. </param>
        public CommunicationCodeHyperlink(string url, string title)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title");
            }

            Url = url;
            Title = title;
        }

        /// <summary>
        /// Two CommunicationCodeHyperlinks are equal if they have equal Url and Title properties
        /// </summary>
        /// <param name="obj">The CommunicationCodeHyperlink to compare to this</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var hyperlink = obj as CommunicationCodeHyperlink;
            if (hyperlink.Url == this.Url && hyperlink.Title == this.Title)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Computes the HashCode of this CommunicationCodeHyperlink based on the Url and Title
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Url.GetHashCode() ^ Title.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of this CommunicationCodeHyperlink based on the Title
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Title;
        }
    }
}
