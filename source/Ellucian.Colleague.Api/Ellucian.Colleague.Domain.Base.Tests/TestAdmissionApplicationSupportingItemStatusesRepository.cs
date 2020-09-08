﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestAdmissionApplicationSupportingItemStatusesRepository
    {
        private readonly string[,] _corrStatuses =
        {
            //   GUID                                   CODE    DESCRIPTION         OFFICE CODE
            {
                "b0eba383-5acf-4050-949d-8bb7a17c5012", "CcCode1", "Description1",     "ADM", "1"
            },
            {
                "b2cb62b5-936f-4456-b29e-e49242f70e5c", "CcCode2", "Description2",     "REG", "0"
            },
            {
                "51dcb8a4-6d47-4429-9769-d11a0725d3f6", "CcCode3", "Description3",     "PUR", "1"
            },
            {
                "b886c618-fd24-49e0-ac5a-c300d4554a39", "CcCode4",  "Description4",    "FAC", "2"
            }
        };


        public IEnumerable<CorrStatus> Get()
        {
            var supportingItemTypes = new List<CorrStatus>();
            var items = _corrStatuses.Length / 5;

            for( var x = 0; x < items; x++ )
            {
                supportingItemTypes.Add( new CorrStatus( _corrStatuses[ x, 0 ], _corrStatuses[ x, 1 ], _corrStatuses[ x, 2 ], _corrStatuses[ x, 3 ] ) { Action = _corrStatuses[ x, 4 ] } );
            }
            return supportingItemTypes;
        }
    }
}