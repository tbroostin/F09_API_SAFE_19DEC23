/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Services
{
    public static class CommunicationDomainService
    {
        /// <summary>
        /// Adds or updates the communication to the list of communications by searching for a duplicate.
        /// If a duplicate is found, the index of the communication in the list is updated with the given communication.
        /// If a duplicate is not found, the communication is appended to the end of the list.
        /// </summary>
        /// <param name="communications">The list of communications to add to or update</param>
        /// <param name="comm">The communication that will be added to or updated into the list</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="comm"/> is null</exception>
        public static void AddOrUpdate(this List<Communication> communications, Communication comm)
        {
            if (comm == null)
            {
                throw new ArgumentNullException("comm");
            }
            var index = communications.IndexOfDuplicate(comm);
            if (index >= 0)
            {
                communications[index] = comm;
            }
            else
            {
                communications.Add(comm);
            }
        }

        /// <summary>
        /// Finds the index in the communications list of the matching communication. If no duplicate exists,
        /// this method returns a value less than zero.
        /// </summary>
        /// <param name="communications">The list of communications to search for the duplicate</param>
        /// <param name="duplicate">Method tries to find a duplicate the in the input communication in the list of communications</param>
        /// <returns>The zero-based index of the duplicate communication. -1 if no duplicate is found.</returns>
        public static int IndexOfDuplicate(this List<Communication> communications, Communication duplicate)
        {
            return communications.IndexOf(duplicate);
        }

        /// <summary>
        /// Determines whether the list of communications contains a match of the given communication.
        /// </summary>
        /// <param name="communications">The list of communications to search for the duplicate</param>
        /// <param name="duplicate">The communication to find in the list</param>
        /// <returns></returns>
        public static bool ContainsDuplicate(this IEnumerable<Communication> communications, Communication duplicate)
        {
            return communications.ToList().IndexOfDuplicate(duplicate) >= 0;
        }

        /// <summary>
        /// Return the communication in the list that equals the given communication. <see cref="Communication.Equals"/>
        /// </summary>
        /// <param name="communications">The list of communications to search for the duplicate</param>
        /// <param name="duplicate">The communication to find in the list</param>
        /// <returns></returns>
        public static Communication GetDuplicate(this IEnumerable<Communication> communications, Communication duplicate)
        {
            var index = communications.ToList().IndexOfDuplicate(duplicate);
            return index >= 0 ? communications.ToList()[index] : null;
        }

        /// <summary>
        /// When creating or updating Communications, it's recommended that you call this method to compare the communication against
        /// existing communications in order to property assign the date properties. This ensures equivalency with legacy Colleague updates.
        /// </summary>
        /// <param name="comm">The new or updated communication</param>
        /// <param name="existingCommunications">The existing communications</param>
        /// <returns>A new communication object with revised dates. This object is safe to submit to the database.</returns>
        public static Communication ReviseDatesForCreateOrUpdate(this Communication comm, IEnumerable<Communication> existingCommunications)
        {
            var existingCommunicationList = existingCommunications;
            if (existingCommunicationList == null) { existingCommunicationList = new List<Communication>(); }
            var existingComm = existingCommunicationList.GetDuplicate(comm);
            var dateCheckComm = comm.DeepCopy();
            if (existingComm != null)
            {
                //do update date checks
                if (!existingComm.StatusDate.HasValue)
                {
                    if (!dateCheckComm.AssignedDate.HasValue)
                    {
                        dateCheckComm.AssignedDate = existingComm.AssignedDate;
                    }
                    if (!dateCheckComm.ActionDate.HasValue)
                    {
                        dateCheckComm.ActionDate = existingComm.ActionDate;
                    }
                }

                if (!existingComm.AssignedDate.HasValue && !dateCheckComm.AssignedDate.HasValue)
                {
                    if (dateCheckComm.StatusDate.HasValue)
                    {
                        dateCheckComm.AssignedDate = dateCheckComm.StatusDate;
                    }
                    else
                    {
                        dateCheckComm.AssignedDate = DateTime.Today;
                    }
                }
            }
            else
            {
                //do create date checks
                if (!dateCheckComm.AssignedDate.HasValue)
                {
                    if (dateCheckComm.StatusDate.HasValue && dateCheckComm.StatusDate.Value.Date < DateTime.Today)
                    {
                        dateCheckComm.AssignedDate = dateCheckComm.StatusDate;
                    }
                    else
                    {
                        dateCheckComm.AssignedDate = DateTime.Today;
                    }
                }
            }

            return dateCheckComm;
        }

        /// <summary>
        /// Create a Deep Copy of the Communication object
        /// </summary>
        /// <param name="comm"></param>
        /// <returns></returns>
        public static Communication DeepCopy(this Communication comm)
        {
            return new Communication(comm.PersonId, comm.Code)
            {
                ActionDate = comm.ActionDate,
                AssignedDate = comm.AssignedDate,
                CommentId = comm.CommentId,
                InstanceDescription = comm.InstanceDescription,
                StatusCode = comm.StatusCode,
                StatusDate = comm.StatusDate
            };
        }
    }
}
