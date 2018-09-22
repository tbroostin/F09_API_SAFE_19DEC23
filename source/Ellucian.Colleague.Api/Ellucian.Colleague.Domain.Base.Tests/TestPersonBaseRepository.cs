/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Tests
{
     public class TestPersonBaseRepository
     {
          public List<PersonBase> personBaseList;

          public TestPersonBaseRepository()
          {
               personBaseList = new List<PersonBase>(){
                    new PersonBase("0013828", "Trump"),
                    new PersonBase("0012882", "Cruz"),
                    new PersonBase("0012912", "Bush")
               };
               personBaseList[0].FirstName = "Donald";
               personBaseList[1].FirstName = "Ted";
               personBaseList[2].FirstName = "Jeb!";
          }

          public List<PersonBase> GetPersonBaseEntities()
          {
               return personBaseList;
          }
     }
}
