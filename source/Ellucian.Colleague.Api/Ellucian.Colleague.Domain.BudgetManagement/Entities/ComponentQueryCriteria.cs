﻿// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Component query filter criteria.
    /// </summary>
    [Serializable]
    public class ComponentQueryCriteria
    {
        /// <summary>
        /// Component name.
        /// </summary>
        public string ComponentName { get { return componentName; } }
        private readonly string componentName;

        /// <summary>
        /// A list of individual component filter values.
        /// </summary>
        public List<string> IndividualComponentValues { get; set; }

        /// <summary>
        /// A list of component filter range objects.
        /// </summary
        public List<ComponentRangeQueryCriteria> RangeComponentValues { get; set; }

        /// <summary>
        /// Constructor that initializes a cost center component query criteria domain entity.
        /// </summary>
        /// <param name="componentName">Name of the cost center component filter query criteria.</param>
        public ComponentQueryCriteria(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                throw new ArgumentNullException("componentName", "Component name is a required field.");
            }
            this.componentName = componentName;
            this.IndividualComponentValues = new List<string>();
            this.RangeComponentValues = new List<ComponentRangeQueryCriteria>();
        }
    }
}
