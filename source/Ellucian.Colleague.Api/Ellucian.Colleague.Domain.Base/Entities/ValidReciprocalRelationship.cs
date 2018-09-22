// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible valid reciprocal relationship
    /// </summary>
    [Serializable]
    public enum ValidReciprocalRelationship
    {
        /// <summary>
        /// Parent
        /// </summary>
        Parent,
        /// <summary>
        /// Step-Parent
        /// </summary>
        StepParent,
        /// <summary>
        /// Mother
        /// </summary>
        Mother,
        /// <summary>
        /// Step-Mother
        /// </summary>
        StepMother,
        /// <summary>
        /// Father
        /// </summary>
        Father,
        /// <summary>
        /// Step-Father
        /// </summary>
        StepFather,
        /// <summary>
        ///  Child
        /// </summary>
        Child,
        /// <summary>
        /// Stepchild
        /// </summary>
        StepChild,
        /// <summary>
        /// Daughter
        /// </summary>
        Daughter,
        /// <summary>
        /// Step-Daughter
        /// </summary>
        StepDaughter,
        /// <summary>
        /// Son
        /// </summary>
        Son,
        /// <summary>
        /// Step-Son
        /// </summary>
        StepSon,
        /// <summary>
        /// Sibling
        /// </summary>
        Sibling,
        /// <summary>
        /// Step-Sibling
        /// </summary>
        StepSibling,
        /// <summary>
        /// Brother
        /// </summary>
        Brother,
        /// <summary>
        /// StepBrother
        /// </summary>
        StepBrother,
        /// <summary>
        /// Sister
        /// </summary>
        Sister,
        /// <summary>
        /// StepSister
        /// </summary>
        StepSister,
        /// <summary>
        /// Spouse
        /// </summary>
        Spouse,
        /// <summary>
        /// Wife
        /// </summary>
        Wife,
        /// <summary>
        /// Partner
        /// </summary>
        Partner,
        /// <summary>
        /// Grandparent
        /// </summary>
        GrandParent,
        /// <summary>
        /// Grandmother
        /// </summary>
        GrandMother,
        /// <summary>
        /// Grandfather
        /// </summary>
        GrandFather,
        /// <summary>
        /// Grandchild
        /// </summary>
        GrandChild,
        /// <summary>
        /// Granddaughter
        /// </summary>
        GrandDaughter,
        /// <summary>
        /// GrandSon
        /// </summary>
        GrandSon,
        /// <summary>
        /// Parent-In-Law
        /// </summary>
        ParentInLaw,
        /// <summary>
        /// Mother-In-Law
        /// </summary>
        MotherInLaw,
        /// <summary>
        /// Father-In-Law
        /// </summary>
        FatherInLaw,
        /// <summary>
        /// Child-In-Law
        /// </summary>
        ChildInLaw,
        /// <summary>
        /// Daughter-In-Law
        /// </summary>
        DaughterInLaw,
        /// <summary>
        /// Son-In-Law
        /// </summary>
        SonInLaw,
        /// <summary>
        /// Sibling-In-Law
        /// </summary>
        SiblingInLaw,
        /// <summary>
        /// Sister-In-Law
        /// </summary>
        SisterInLaw,
        /// <summary>
        /// Brother-In-Law 
        /// </summary>
        BrotherInLaw,
        /// <summary>
        /// Sibling Of Parent
        /// </summary>
        SiblingOfParent,
        /// <summary>
        /// Aunt
        /// </summary>
        Aunt,
        /// <summary>
        /// Uncle
        /// </summary>
        Uncle,
        /// <summary>
        /// Child Of Sibling
        /// </summary>
        ChildOfSibling,
        /// <summary>
        /// Niece
        /// </summary>
        Niece,
        /// <summary>
        /// Nephew
        /// </summary>
        Nephew,
        /// <summary>
        /// Cousin
        /// </summary>
        Cousin,
        /// <summary>
        /// Friend
        /// </summary>
        Friend,
        /// <summary>
        /// Relative
        /// </summary>
        Relative,
        /// <summary>
        /// Coworker
        /// </summary>
        Coworker,
        /// <summary>
        /// Neighbor
        /// </summary>
        Neighbor,
        /// <summary>
        /// Classmate
        /// </summary>
        Classmate,
        /// <summary>
        /// Caregiver
        /// </summary>
        Caregiver,
        /// <summary>
        /// Husband
        /// </summary>
        Husband,
        /// <summary>
        /// Other
        /// </summary>
        Other,

    }
}