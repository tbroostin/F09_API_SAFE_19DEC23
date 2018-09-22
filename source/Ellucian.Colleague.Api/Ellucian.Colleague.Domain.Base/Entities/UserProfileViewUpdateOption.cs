// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible  user profile property permissions
    /// </summary>
    [Serializable]
    public enum UserProfileViewUpdateOption
    {
        /// <summary>
        /// User can only view the user profile property but cannot modify it
        /// </summary>
        Viewable,

        /// <summary>
        /// User can view and modify the user profile property
        /// </summary>
        Updatable,

        /// <summary>
        /// User cannot view or  modify the user profile property
        /// </summary>
        NotAllowed
    }
}
