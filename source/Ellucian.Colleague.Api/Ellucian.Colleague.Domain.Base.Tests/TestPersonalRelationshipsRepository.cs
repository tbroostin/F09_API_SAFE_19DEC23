// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPersonalRelationshipsRepository
    {
        #region Person Relationships
        
        string personalRelationships = @"[
                                        {
                                        'subjectPerson': {
                                            'id': 'f3836d0e-ca45-455a-a873-d0771b8f089e'
                                        },
                                        'relatedPerson': {
                                            'id': 'e963d192-00ba-4327-aa00-9630b86e0533'
                                        },
                                        'directRelationship': {
                                            'type': 'mother',
                                            'detail': {
                                            'id': '7989a936-f41d-4c08-9fda-dd41314a9e34'
                                            }
                                        },
                                        'reciprocalRelationship': {
                                            'type': 'daughter',
                                            'detail': {
                                            'id': '2c27b01e-fb4e-4884-aece-77dbfce45250'
                                            }
                                        },
                                        'startOn': '2015-03-13T13:14:36+00:00',
                                        'endOn': '2015-05-13T13:14:36+00:00',
                                        'status': {
                                            'id': 'c21bede5-fafd-4f78-a162-6d2efea52692'
                                          },
                                        'comment': 'This is a test comment This is another comment Another comment on fathers record',
                                        'id': 'be6304e5-409d-4345-b4ad-4d34e1730e14'
                                        },
                                        {
                                        'subjectPerson': {
                                            'id': 'f3836d0e-ca45-455a-a873-d0771b8f089e'
                                        },
                                        'relatedPerson': {
                                            'id': '93fd7cc0-2f4a-4e04-a239-b8b59a7575b4'
                                        },
                                        'directRelationship': {
                                            'type': 'mother',
                                            'detail': {
                                            'id': '7989a936-f41d-4c08-9fda-dd41314a9e34'
                                            }
                                        },
                                        'reciprocalRelationship': {
                                            'type': 'son',
                                            'detail': {
                                            'id': '2c27b01e-fb4e-4884-aece-77dbfce45250'
                                            }
                                        },
                                        'status': {
                                            'id': 'd21bede5-fafd-4f78-a162-6d2efea52675'
                                          },
                                        'comment': 'This is a test comment This is another comment Another comment on fathers record',
                                        'id': '9eeb5365-9478-4b40-8463-1e1d0ecf8956'
                                        },
                                        {
                                        'subjectPerson': {
                                            'id': 'f3836d0e-ca45-455a-a873-d0771b8f089e'
                                        },
                                        'relatedPerson': {
                                            'id': 'e9e8e973-3d65-4c14-8155-6efc91100ae3'
                                        },
                                        'directRelationship': {
                                            'type': 'mother',
                                            'detail': {
                                            'id': '7989a936-f41d-4c08-9fda-dd41314a9e34'
                                            }
                                        },
                                        'reciprocalRelationship': {
                                            'type': 'son',
                                            'detail': {
                                            'id': '2c27b01e-fb4e-4884-aece-77dbfce45250'
                                            }
                                        },
                                        'status': {
                                            'id': 'h21bede5-fafd-4f78-a162-6d2efea52122'
                                          },
                                        'comment': 'This is a test comment This is another comment Another comment on fathers record',
                                        'id': '5d96d550-ba60-49fd-8401-e1a8094a4dc5'
                                        }
                                    ]";
        #endregion

        public IEnumerable<Dtos.PersonalRelationship> GetPersonalRelationships()
        {
            var tempPersonalRelationships = JsonConvert.DeserializeObject<IEnumerable<Dtos.PersonalRelationship>>(personalRelationships);
            return tempPersonalRelationships;
        }

        public IEnumerable<Relationship> GetPersonalRelationshipsEnities()
        {
            List<Relationship> relationships = new List<Relationship>() 
            {
                new Relationship("1", "2", "Parent", true, new DateTime(2015,03, 14), new DateTime(2015, 05, 14))
                {
                    Guid = "be6304e5-409d-4345-b4ad-4d34e1730e14",
                    Comment = "Comment 1",
                    Status = "A"
                },
                 new Relationship("3", "4", "Parent", true, new DateTime(2016,10, 09), new DateTime(2016, 12, 12))
                {
                    Guid = "9eeb5365-9478-4b40-8463-1e1d0ecf8956",
                    Comment = "Comment 2",
                    Status = ""
                },
                 new Relationship("5", "6", "Parent", true, new DateTime(2016,04, 14), new DateTime(2016, 05, 15))
                {
                    Guid = "5d96d550-ba60-49fd-8401-e1a8094a4dc5",
                    Comment = "Comment 3"
                },
                 new Relationship("7", "8", "Child", true, new DateTime(2016,04, 14), new DateTime(2016, 05, 15))
                {
                    Guid = "6d96d550-ba60-49fd-8401-e1a8094a4dc7",
                    Comment = "Comment 4"
                }
            };
            return relationships;
        }
    }
}
