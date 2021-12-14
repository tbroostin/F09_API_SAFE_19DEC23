// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Sort Section Entity on different fields
    /// </summary>
    [Serializable]
    public class SectionSortHelper : IComparer<Section>
    {
        /// <summary>
        /// Sort type- identifies which field to sort on
        /// </summary>
        private CatalogSortType SortType;
        /// <summary>
        /// Sort Direction- identifies direction of sorting (ascending or descending)
        /// </summary>
        private CatalogSortDirection SortDirection;


        public SectionSortHelper(CatalogSortType sortType, CatalogSortDirection sortDirection)
        {
            SortDirection = sortDirection;
            SortType = sortType;
        }
        public int Compare(Section toCompare, Section compareWith)
        {
            int comparisonResult = 0;
            switch (this.SortType)
            {
                case CatalogSortType.SectionName:
                    {
                        if (string.Compare(toCompare.SortableSectionName, compareWith.SortableSectionName) > 0)
                        {
                            comparisonResult = 1;
                        }
                        else if (string.Compare(toCompare.SortableSectionName, compareWith.SortableSectionName) < 0)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.Status:
                    {
                        if (toCompare.AvailabilityStatus > compareWith.AvailabilityStatus)
                        {
                            comparisonResult = 1;
                        }
                        else if (toCompare.AvailabilityStatus < compareWith.AvailabilityStatus)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.SeatsAvailable:
                    {
                        int toCompareSeats = toCompare.Available.HasValue ? toCompare.Available.Value : 0;
                        int compareWithSeats = compareWith.Available.HasValue ? compareWith.Available.Value : 0;
                        if (toCompareSeats > compareWithSeats)
                        {
                            comparisonResult = 1;
                        }
                        else if (toCompareSeats < compareWithSeats)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.Location:
                    {
                        if (string.Compare(toCompare.Location, compareWith.Location) > 0)
                        {
                            comparisonResult = 1;
                        }
                        else if (string.Compare(toCompare.Location, compareWith.Location) < 0)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.Term:
                    {
                        //section with no term should be last in order otherwise should be in order of reporting year followed by sequence
                        if (string.IsNullOrEmpty(toCompare.TermId) && !string.IsNullOrEmpty(compareWith.TermId))
                        {
                            comparisonResult = 1;
                        }
                        else if (!string.IsNullOrEmpty(toCompare.TermId) && string.IsNullOrEmpty(compareWith.TermId))
                        {
                            comparisonResult = -1;
                        }
                        else
                        {

                            if (toCompare.SectionTerm.ReportingYear > compareWith.SectionTerm.ReportingYear)
                            {
                                comparisonResult = 1;
                            }
                            else if (toCompare.SectionTerm.ReportingYear < compareWith.SectionTerm.ReportingYear)
                            {
                                comparisonResult = -1;
                            }
                            else
                            {
                                if (toCompare.SectionTerm.Sequence > compareWith.SectionTerm.Sequence)
                                {
                                    comparisonResult = 1;
                                }
                                else if (toCompare.SectionTerm.Sequence < compareWith.SectionTerm.Sequence)
                                {
                                    comparisonResult = -1;
                                }
                            }
                        }
                    }
                    break;
                case CatalogSortType.Title:
                    {
                        if (string.Compare(toCompare.Title, compareWith.Title) > 0)
                        {
                            comparisonResult = 1;
                        }
                        else if (string.Compare(toCompare.Title, compareWith.Title) < 0)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.Dates:
                    {
                        if (toCompare.StartDate > compareWith.StartDate)
                        {
                            comparisonResult = 1;
                        }
                        else if (toCompare.StartDate < compareWith.StartDate)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.InstructionalMethod:
                    {
                        if (string.Compare(toCompare.SortableInstructionalMethod, compareWith.SortableInstructionalMethod) > 0)
                        {
                            comparisonResult = 1;
                        }
                        else if (string.Compare(toCompare.SortableInstructionalMethod, compareWith.SortableInstructionalMethod) < 0)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.FacultyName:
                    {
                        if (string.Compare(toCompare.SortableFacultyName, compareWith.SortableFacultyName) > 0)
                        {
                            comparisonResult = 1;
                        }
                        else if (string.Compare(toCompare.SortableFacultyName, compareWith.SortableFacultyName) < 0)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.Credits:
                    {
                        if (toCompare.SortableCredits > compareWith.SortableCredits)
                        {
                            comparisonResult = 1;
                        }
                        else if (toCompare.SortableCredits < compareWith.SortableCredits)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.CourseType:
                    {
                        if (string.Compare(toCompare.SortableCourseType, compareWith.SortableCourseType) > 0)
                        {
                            comparisonResult = 1;
                        }
                        else if (string.Compare(toCompare.SortableCourseType, compareWith.SortableCourseType) < 0)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.AcademicLevel:
                    {
                        if (string.Compare(toCompare.AcademicLevelCode, compareWith.AcademicLevelCode) > 0)
                        {
                            comparisonResult = 1;
                        }
                        else if (string.Compare(toCompare.AcademicLevelCode, compareWith.AcademicLevelCode) < 0)
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                case CatalogSortType.MeetingInformation:
                    {
                        if (toCompare.SortableSectionMeetings.Any() && compareWith.SortableSectionMeetings.Any())
                        {
                            if (toCompare.SortableSectionMeetings[0].CompareTo(compareWith.SortableSectionMeetings[0]) > 0)
                            {
                                comparisonResult = 1;
                            }
                            else if (toCompare.SortableSectionMeetings[0].CompareTo(compareWith.SortableSectionMeetings[0]) < 0)
                            {
                                comparisonResult = -1;
                            }
                        }
                        else if (toCompare.SortableSectionMeetings.Any() && !compareWith.SortableSectionMeetings.Any())
                        {
                            comparisonResult = 1;
                        }
                        else if (!toCompare.SortableSectionMeetings.Any() && compareWith.SortableSectionMeetings.Any())
                        {
                            comparisonResult = -1;
                        }
                    }
                    break;
                
            }
            if (this.SortDirection == CatalogSortDirection.Descending)
            {
                comparisonResult = comparisonResult * -1;
            }
            if (comparisonResult == 0)
            {
                if (string.Compare(toCompare.SortableSectionName, compareWith.SortableSectionName) > 0)
                {
                    comparisonResult = 1;
                }
                else if (string.Compare(toCompare.SortableSectionName, compareWith.SortableSectionName) < 0)
                {
                    comparisonResult = -1;
                }
            }

            return comparisonResult;
        }
    }
}
