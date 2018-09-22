// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestGeographicAreaRepository
    {
        private string[,] geographicAreaTypes = {
                                                //GUID   CODE   DESCRIPTION
                                                {"625c69ff-280b-4ed3-9474-662a43616a8a", "FUND", "Description"},
                                                {"bfea651b-8e27-4fcd-abe3-04573443c04c", "GOV", "Description"},
                                                {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "POST", "Description"},
                                                {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "REC", "Description"}
                                        };
        private string[,] geographicAreas = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "BALT", "Baltimore", "FUND"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "EU", "European Union", "FUND"},
                                            {"dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "ALX", "Alexandria", "GOV"},
                                            {"31d8aa32-dbe6-3b89-a1c4-2cad39e232e4", "ANN", "Annapolis", "GOV"},
                                            {"72b7737b-27db-4a06-944b-97d00c29b3db", "00222", "Zipcode", "POST"},
                                            {"31d8aa32-dbe6-83j7-a1c4-2cad39e232e4", "00333", "Zipcode", "POST"}
                                      };

        private string[,] chapters = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "BALT", "Baltimore"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "EU", "European Union"}
                                      };
        private string[,] counties = {
                                            //GUID   CODE   DESCRIPTION
                                            {"dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "ALX", "Alexandria"}, 
                                            {"31d8aa32-dbe6-3b89-a1c4-2cad39e232e4", "ANN", "Annapolis"}
                                      };
        private string[,] zipcodeXlats = {
                                            //GUID   CODE   DESCRIPTION
                                            {"72b7737b-27db-4a06-944b-97d00c29b3db", "00222", "Zipcode"}, 
                                            {"31d8aa32-dbe6-83j7-a1c4-2cad39e232e4", "00333", "Zipcode"}
                                      };

        public IEnumerable<Chapter> GetChapters()
        {
            var chapterList = new List<Chapter>();

            // There are 3 fields for each chapter in the array
            var items = chapters.Length / 3;

            for (int x = 0; x < items; x++)
            {
                chapterList.Add(new Chapter(chapters[x, 0], chapters[x, 1], chapters[x, 2]));
            }
            return chapterList;
        }

        public IEnumerable<County> GetCounties()
        {
            var countyList = new List<County>();

            // There are 3 fields for each county in the array
            var items = counties.Length / 3;

            for (int x = 0; x < items; x++)
            {
                countyList.Add(new County(counties[x, 0], counties[x, 1], counties[x, 2]));
            }
            return countyList;
        }

        public IEnumerable<ZipcodeXlat> GetZipCodeXlats()
        {
            var zipcodeXlatList = new List<ZipcodeXlat>();

            // There are 3 fields for each zipcode xlat in the array
            var items = zipcodeXlats.Length / 3;

            for (int x = 0; x < items; x++)
            {
                zipcodeXlatList.Add(new ZipcodeXlat(zipcodeXlats[x, 0], zipcodeXlats[x, 1], zipcodeXlats[x, 2]));
            }
            return zipcodeXlatList;
        }

        public IEnumerable<GeographicArea> GetGeographicAreas()
        {
            var geographicAreaList = new List<GeographicArea>();

            // There are 4 fields for each geographic areas in the array
            var items = geographicAreas.Length / 4;

            for (int x = 0; x < items; x++)
            {
                geographicAreaList.Add(new GeographicArea(geographicAreas[x, 0], geographicAreas[x, 1], geographicAreas[x, 2], geographicAreas[x, 3]));
            }
            return geographicAreaList;
        }

        public IEnumerable<GeographicAreaType> Get()
        {
            var geographicAreasList = new List<GeographicAreaType>();

            geographicAreasList.Add(new GeographicAreaType(geographicAreaTypes[0, 0], geographicAreaTypes[0, 1], geographicAreaTypes[0, 2], GeographicAreaTypeCategory.Fundraising));
            geographicAreasList.Add(new GeographicAreaType(geographicAreaTypes[1, 0], geographicAreaTypes[1, 1], geographicAreaTypes[1, 2], GeographicAreaTypeCategory.Governmental));
            geographicAreasList.Add(new GeographicAreaType(geographicAreaTypes[2, 0], geographicAreaTypes[2, 1], geographicAreaTypes[2, 2], GeographicAreaTypeCategory.Postal));
            geographicAreasList.Add(new GeographicAreaType(geographicAreaTypes[3, 0], geographicAreaTypes[3, 1], geographicAreaTypes[3, 2], GeographicAreaTypeCategory.Recruitment));
            
            return geographicAreasList;
        }
    }
}
