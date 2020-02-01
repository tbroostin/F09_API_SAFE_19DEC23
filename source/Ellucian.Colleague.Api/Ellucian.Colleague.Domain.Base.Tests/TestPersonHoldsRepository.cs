// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPersonHoldsRepository
    {
        #region Person Holds
        string personHolds = @"[
                                {
                                'person': {
                                    'id': '48285630-3f7f-4b16-b73d-1b1a4bdd27ee'
                                },
                                'type': {
                                    'category': 'financial',
                                    'detail': {
                                    'id': 'd57783ad-8f9e-4805-8e5d-c10452ee375d'
                                    }
                                },
                                'notificationIndicator': 'watch',
                                'startOn': '2012-12-01T04:00:00Z',
                                'endOn': '2012-12-03T04:00:00Z',
                                'comment': null,
                                'metadata': {},
                                'id': '23977f85-f200-479f-9eee-3921bb4667d3'
                                },
                                {
                                'person': {
                                    'id': '1ef4683b-8fea-4358-ac38-511a5e4547af'
                                },
                                'type': {
                                    'category': 'academic',
                                    'detail': {
                                    'id': '4494fbb5-1780-4068-92bf-ac6d67a6e497'
                                    }
                                },
                                'notificationIndicator': 'notify',
                                'startOn': '2014-10-17T04:00:00Z',
                                'endOn': null,
                                'comment': null,
                                'metadata': {},
                                'id': 'ffbd9a82-3e05-4103-a243-995166f1430f'
                                },
                                {
                                'person': {
                                    'id': '895cebf0-e6e8-4169-aac6-e0e14dfefdd4'
                                },
                                'type': {
                                    'category': 'financial',
                                    'detail': {
                                    'id': 'd69f912e-8cf4-4158-8504-5ccf8e613dfe'
                                    }
                                },
                                'notificationIndicator': 'notify',
                                'startOn': '2013-04-15T04:00:00Z',
                                'endOn': null,
                                'comment': 'test',
                                'metadata': {},
                                'id': '65747675-f4ca-4e8b-91aa-d37c3449a82c'
                                },
                                {
                                'person': {
                                    'id': '895cebf0-e6e8-4169-aac6-e0e14dfefdd4'
                                },
                                'type': {
                                    'category': 'academic',
                                    'detail': {
                                    'id': 'd69f912e-8cf4-4158-8504-5ccf8e613dfe'
                                    }
                                },
                                'notificationIndicator': 'notify',
                                'startOn': '2013-04-15T04:00:00Z',
                                'endOn': null,
                                'comment': 'test',
                                'metadata': {},
                                'id': '9a9bdb5f-b827-4ea0-80cc-c8b9ac17325b'
                                },
                                {
                                'person': {
                                    'id': '48285630-3f7f-4b16-b73d-1b1a4bdd27ee'
                                },
                                'type': {
                                    'category': 'financial',
                                    'detail': {
                                    'id': 'd57783ad-8f9e-4805-8e5d-c10452ee375d'
                                    }
                                },
                                'notificationIndicator': 'watch',
                                'startOn': '2012-12-01T04:00:00Z',
                                'endOn': '2012-12-03T04:00:00Z',
                                'comment': null,
                                'metadata': {},
                                'id': '00000000-0000-0000-0000-000000000000'
                                }
                            ]";
        #endregion

        public IEnumerable<Dtos.PersonHold> GetPersonHolds()
        {
            var tempPersonHolds = JsonConvert.DeserializeObject<IEnumerable<Dtos.PersonHold>>(personHolds);
            return tempPersonHolds;
        }
    }
}
