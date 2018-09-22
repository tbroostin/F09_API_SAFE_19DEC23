using System;
// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestExternalSystemRepository
    {
        private static List<ExternalSystem> _ExternalSystems = new List<ExternalSystem>();
        public static List<ExternalSystem> ExternalSystems
        {
            get
            {
                if (_ExternalSystems.Count == 0)
                {
                    GenerateEntities();
                }
                return _ExternalSystems;
            }
        }

        private static void GenerateEntities()
        {
            _ExternalSystems.Add(new ExternalSystem("HD", "Housing Director"));
            _ExternalSystems.Add(new ExternalSystem("RMS", "RMS"));
            _ExternalSystems.Add(new ExternalSystem("SR", "StarRez"));
        }
    }
}
