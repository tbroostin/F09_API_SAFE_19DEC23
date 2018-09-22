// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Domain.Base.Services
{
    public static class PersonNameService
    {
        /// <summary>
        /// Gets an individual name for a person based on an ordered list of name hierarchy types. 
        /// </summary>
        /// <param name="personBaseEntity">PersonBase type of entity</param>
        /// <param name="nameAddressHierarchy">Name and Address Hierarchy to use in the calculation.</param>
        /// <returns>A PersonHierarchyName object calculated for the individual</returns>
        public static PersonHierarchyName GetHierarchyName(PersonBase personBaseEntity, NameAddressHierarchy nameAddressHierarchy)
        {
            if (personBaseEntity == null)
            {
                throw new ArgumentNullException("personBaseEntity", "Must provide a person to Get Hierarchy Name.");
            }
            if (nameAddressHierarchy == null)
            {
                throw new ArgumentNullException("nameAddressHierarchy", "Must provide a name address hierarchy to Get Hierarchy Name.");
            }

            // If ML is not already in the list of name types add it to the end of the list.
            if (!nameAddressHierarchy.NameTypeHierarchy.Contains("ML")) { nameAddressHierarchy.AddNameTypeHierarchy("ML"); }
            Regex trimRegEx = new Regex(@"\s+");

            PersonHierarchyName personHierarchyName = new PersonHierarchyName(nameAddressHierarchy.Code);
            // Loop through the hierarchy name types until one of the types produces a name.  There are a set of "hard coded" types that could
            // be supplied but then the client can define any type and use it on the formatted name fields within Colleague.
            foreach (var nameType in nameAddressHierarchy.NameTypeHierarchy)
            {
                switch (nameType.ToUpper())
                {
                    case "LFM":
                        // Returns a persons name in Lastname, Firstname Middle initial format
                        string first = !string.IsNullOrEmpty(personBaseEntity.FirstName) ? personBaseEntity.FirstName.Trim() : string.Empty;
                        first = first.Length == 1 ? first + "." : first;
                        string middle = !string.IsNullOrEmpty(personBaseEntity.MiddleName) ? personBaseEntity.MiddleName.Trim() : string.Empty;
                        middle = middle.Length > 1 ? middle.Substring(0, 1) + "." : (middle.Length == 1 ? middle + "." : middle);
                        string lfmName = ((personBaseEntity.LastName.Trim() + ", " + first).Trim() + " " + middle).Trim();
                        // Remove any extra blank spaces within the name pieces
                        lfmName = trimRegEx.Replace(lfmName, @" ");
                        personHierarchyName.FullName = lfmName;
                        personHierarchyName.FirstName = personBaseEntity.FirstName;
                        personHierarchyName.MiddleName = personBaseEntity.MiddleName;
                        personHierarchyName.LastName = personBaseEntity.LastName;
                        return personHierarchyName;
                    case "MA":
                        // If MA was requested but no birth name fields are included fall through to next type.
                        if (!string.IsNullOrEmpty(personBaseEntity.BirthNameLast) || !string.IsNullOrEmpty(personBaseEntity.BirthNameFirst) || !string.IsNullOrEmpty(personBaseEntity.BirthNameMiddle))
                        {
                            string birthFirst = !string.IsNullOrEmpty(personBaseEntity.BirthNameFirst) ? personBaseEntity.BirthNameFirst : personBaseEntity.FirstName;
                            string birthMiddle = !string.IsNullOrEmpty(personBaseEntity.BirthNameMiddle) ? personBaseEntity.BirthNameMiddle : personBaseEntity.MiddleName;
                            string birthLast = !string.IsNullOrEmpty(personBaseEntity.BirthNameLast) ? personBaseEntity.BirthNameLast : personBaseEntity.LastName;
                            //return FormatName(string.Empty, birthFirst, birthMiddle, birthLast, string.Empty);
                            personHierarchyName.FullName = FormatName(string.Empty, birthFirst, birthMiddle, birthLast, string.Empty);
                            personHierarchyName.FirstName = birthFirst;
                            personHierarchyName.MiddleName = birthMiddle;
                            personHierarchyName.LastName = birthLast;
                            return personHierarchyName;
                        }
                        break;
                    case "ML":
                        // Returns formal name using the person's mail label name as a basis
                        // If mail lable override is blank use the preferred name overridee
                        personHierarchyName.FullName = personBaseEntity.MailLabelNameOverride;
                        personHierarchyName.FirstName = personBaseEntity.FirstName;
                        personHierarchyName.MiddleName = personBaseEntity.MiddleName;
                        personHierarchyName.LastName = personBaseEntity.LastName;
                        return personHierarchyName;
                    case "PF":
                        // Returns formal name using the preferred name element as a basis for calculation
                        personHierarchyName.FullName = personBaseEntity.PreferredNameOverride;
                        personHierarchyName.FirstName = personBaseEntity.FirstName;
                        personHierarchyName.MiddleName = personBaseEntity.MiddleName;
                        personHierarchyName.LastName = personBaseEntity.LastName;
                        return personHierarchyName;
                    case "FMLS":
                        // The Colleague S.GET.HIERARCHY.NAME routine allows for a type of FMLS but it is currently only used for the TN98 hierarchy, which is for the 1098-T.
                        // It is not a code in the validation code list for name types, however, I've included this case here in the event that 1098-T's are ever produced in .NET.  

                        string prefName = FormatName(string.Empty, personBaseEntity.FirstName, personBaseEntity.MiddleName, personBaseEntity.LastName, personBaseEntity.Suffix);

                        //// Remove all characters from the string except alphanumerics, spaces, dashes (-) and ampersands (&).
                        Regex fmlsRegEx = new Regex(@"[^A-Za-z0-9 \&-]");
                        string restrictedPrefName = fmlsRegEx.Replace(prefName, @"");
                        personHierarchyName.FullName = restrictedPrefName;
                        personHierarchyName.FirstName = personBaseEntity.FirstName;
                        personHierarchyName.MiddleName = personBaseEntity.MiddleName;
                        personHierarchyName.LastName = personBaseEntity.LastName;
                        return personHierarchyName;
                    case "CH":
                        // If any chosen name elements, return chosen name in First Middle Initial Last order
                        if (!string.IsNullOrEmpty(personBaseEntity.ChosenLastName) || !string.IsNullOrEmpty(personBaseEntity.ChosenFirstName) || !string.IsNullOrEmpty(personBaseEntity.ChosenMiddleName))
                        {
                            string chosenFirst = !string.IsNullOrEmpty(personBaseEntity.ChosenFirstName) ? personBaseEntity.ChosenFirstName.Trim() : string.Empty;
                            string chosenMiddle = !string.IsNullOrEmpty(personBaseEntity.ChosenMiddleName) ? personBaseEntity.ChosenMiddleName.Trim() : string.Empty;
                            string chosenLast = !string.IsNullOrEmpty(personBaseEntity.ChosenLastName) ? personBaseEntity.ChosenLastName.Trim() : personBaseEntity.LastName.Trim();
                            //return FormatName(string.Empty, chosenFirst, chosenMiddle, chosenLast, string.Empty);
                            personHierarchyName.FullName = FormatName(string.Empty, chosenFirst, chosenMiddle, chosenLast, string.Empty);
                            personHierarchyName.FirstName = personBaseEntity.ChosenFirstName;
                            personHierarchyName.MiddleName = personBaseEntity.ChosenMiddleName;
                            personHierarchyName.LastName = chosenLast;
                            return personHierarchyName;
                        }
                        break;
                    case "CHL":
                        // If any chosen name elements are present return chosen name in Last, First Middle Initial format
                        if (!string.IsNullOrEmpty(personBaseEntity.ChosenLastName) || !string.IsNullOrEmpty(personBaseEntity.ChosenFirstName) || !string.IsNullOrEmpty(personBaseEntity.ChosenMiddleName))
                        {
                            string cFirst = !string.IsNullOrEmpty(personBaseEntity.ChosenFirstName) ? personBaseEntity.ChosenFirstName.Trim() : string.Empty;
                            cFirst = cFirst.Length == 1 ? cFirst + "." : cFirst;
                            
                            // If there is a middle name convert it to middle initial
                            string cMiddle = !string.IsNullOrEmpty(personBaseEntity.ChosenMiddleName) ? personBaseEntity.ChosenMiddleName.Trim() : string.Empty;
                            cMiddle = cMiddle.Length > 1 ? cMiddle.Substring(0, 1) + "." : (cMiddle.Length == 1 ? cMiddle + "." : cMiddle);

                            string cLast = !string.IsNullOrEmpty(personBaseEntity.ChosenLastName) ? personBaseEntity.ChosenLastName.Trim() : personBaseEntity.LastName.Trim();
                            string clfmName = !string.IsNullOrEmpty(cLast) ? ((cLast + ", " + cFirst).Trim() + " " + cMiddle).Trim() : (cFirst + " " + cMiddle).Trim();
                            // Remove any extra blank spaces within the name pieces
                            clfmName = trimRegEx.Replace(clfmName, @" ");
                            personHierarchyName.FullName = clfmName;
                            personHierarchyName.FirstName = personBaseEntity.ChosenFirstName;
                            personHierarchyName.MiddleName = personBaseEntity.ChosenMiddleName;
                            personHierarchyName.LastName = cLast;
                            return personHierarchyName;
                        }
                        break;
                    default:
                        // If the name type isn't one of the 7 predefined types, see if it is a type in the person's list of formatted names.
                        if (!string.IsNullOrEmpty(nameType))
                        {
                            var formattedName = GetFormattedName(nameType, personBaseEntity.FormattedNames);
                            if (!string.IsNullOrEmpty(formattedName))
                            {
                                personHierarchyName.FullName = formattedName;

                                // Since formatted names are just one long string it unlikely that we can always perfectly determine the last, first, middle name parts
                                // For now, leaving them blank with the expectation that if sorting needs to occur it will have to be on the entire string.
                                return personHierarchyName;
                            }
                        }
                        break;
                }
            }

            // In the highly unlikely event that a name was not returned to this point then default to first middle initial last to match S.GET.HIERARCY.NAME.
            var middleInitial = string.IsNullOrEmpty(personBaseEntity.MiddleName) ? string.Empty : personBaseEntity.MiddleName.Substring(0, 1) + ".";
            personHierarchyName.FullName = FormatName(string.Empty, personBaseEntity.FirstName, middleInitial, personBaseEntity.LastName, string.Empty);
            personHierarchyName.FirstName = personBaseEntity.FirstName;
            personHierarchyName.MiddleName = personBaseEntity.MiddleName;
            personHierarchyName.LastName = personBaseEntity.LastName;
            return personHierarchyName;

        }

        /// <summary>
        /// Used to correctly assemble name pieces into the correct format for the name. If no last name, or first name or middle name is supplied
        /// it will just return an generic string saying name wasn't on file.
        /// </summary>
        /// <param name="namePrefix">Name prefix</param>
        /// <param name="firstname">First name</param>
        /// <param name="middleName">Middle name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="nameSuffix">Name Suffix</param>
        /// <returns></returns>
        public static string FormatName(string namePrefix, string firstname, string middleName, string lastName, string nameSuffix)
        {
            // Last name is required for a person in the domain so it is unlikely that we would need to construct a preferred name without this piece but
            // including a default just in case.
            if (string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstname) && string.IsNullOrEmpty(middleName))
            {
                return "Name not on file.";
            }
            // Supply an empty string if any value is null. And for the first and middle portions, add a period if they are one character.
            string prefix = !string.IsNullOrEmpty(namePrefix) ? namePrefix.Trim() : string.Empty;
            string first = !string.IsNullOrEmpty(firstname) ? firstname.Trim() : string.Empty;
            first = first.Length == 1 ? first + "." : first;
            string middle = !string.IsNullOrEmpty(middleName) ? middleName.Trim() : string.Empty;
            middle = middle.Length == 1 ? middle + "." : middle;
            var formattedSuffix = !string.IsNullOrEmpty(nameSuffix) ? ", " + nameSuffix.Trim() : string.Empty;
            var formattedName = (((prefix + " " + first).Trim() + " " + middle).Trim() + " " + (!string.IsNullOrEmpty(lastName) ? lastName.Trim() : string.Empty) + formattedSuffix).Trim();

            // Remove extra blank spaces within the name pieces - Trim only removes leading and trailing ones.
            Regex regEx = new Regex(@"\s+");
            formattedName = regEx.Replace(formattedName, @" ");

            return formattedName;
        }

        

        /// <summary>
        /// Get the formatted name for a specific type
        /// </summary>
        /// <param name="type">Type of name</param>
        /// <param name="formattedNames">List of a person base entities formatted names</param>
        /// <returns>Formatted name</returns>
        public static string GetFormattedName(string type, List<PersonFormattedName> formattedNames)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type", "Type of formatted name must be specified.");
            }
            if (formattedNames == null || formattedNames.Count() == 0)
            {
                return null;
            }
            var matchingName = formattedNames.Where(f => f.Type == type).FirstOrDefault();
            if (matchingName != null)
            {
                return matchingName.Name;
            }
            else
            {
                return null;
            }
        }


    }
}
