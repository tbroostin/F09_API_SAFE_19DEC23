using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Web.Http.TestUtil {
    public class TestBuildingRepository {

        private const int CODE = 0;
        private const int DESC = 1;
        private const int LOCN = 2;
        private const int BLDGTYPE = 3;
        private const int LONGDESC = 4;
        private const int ADDR = 5;
        private const int CITY = 6;
        private const int STATE = 7;
        private const int ZIP = 8;
        private const int COUNTRY = 9;
        private const int LATITUDE = 10;
        private const int LONGITUDE = 11;
        private const int IMAGE = 12;
        private const int SVCS = 13;
        private const int VISIBLE = 14;
        private const int GUID = 15;

        private string[,] buildings = {   // Code   Desc             LocId   BldgType  LongDesc,            Addr,       City,      State, Zip,     Country, Lat,         Long,          ImageUrl,        AddnlSvcs,        Visible/Mobile
                                          {"AND",   "Anderson Bdlg", "MAIN", "CLASS",  "More AND info",     "123 Main", "Fairfax", "VA",  "22033", "USA",   "77.123454", "-114.987653", "some AND url",   "AND addnl svcs",   "Y",          "28a52594-1f3e-44e1-9f8c-26cb7aa6a7aa"},
                                          {"EIN",   "Einstein Bldg", "MAIN", "OFFICE", "More EIN info",     "234 Main", "Fairfax", "VA",  "22033", "USA",   "77.123452", "-114.987651", "some EIN url",   "EIN addnl svcs",   "N",          "4950a5ef-d542-48e9-bf07-a06b4de2f663"},
                                          {"SPORT", "Sports Center", "SC",   "SPORT",  "More SPORT info",   "345 2nd",  "Fairfax", "VA",  "22033", "USA",   "77.000454", "-114.000653", "some SPORT url", "SPORT addnl svcs", "Y",          "db7e3c81-e40e-4d9f-b3ed-1318f5c34607"},
                                          {"DIN",   "Dining Hall",   "NW",   "DINING", "More DIN info",     "456 3rd",  "Fairfax", "VA",  "22033", "USA",   "77.111454", "-114.111653", "some DIN url",   "DIN addnl info",   "Y",          "8025fd73-2a03-455a-8578-9d65817a0eb5"},
                                          {"MAINT", "Maintenance",   "SAT",  "MAINT",  "Maintanance Shed",  null,       "Fairfax", "VA",  "22033", "USA",   "",          "",            "",               "",                 "N",          "31593337-722e-47d9-813e-784bb6cd6e64"},
                                      };

        public IEnumerable<Building> Get() {
            var bldgs = new List<Building>();
            var items = buildings.Length / (VISIBLE + 1);
            for (int x = 0; x < items; x++) {
                decimal dLat; Decimal.TryParse(buildings[x, LATITUDE], out dLat);
                decimal dLong; Decimal.TryParse(buildings[x, LONGITUDE], out dLong);
                //var guid = Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(buildings[x, ADDR])) {
                    bldgs.Add(new Building(buildings[x, GUID], 
                                           buildings[x, CODE],
                                           buildings[x, DESC],
                                           buildings[x, LOCN],
                                           buildings[x, BLDGTYPE],
                                           buildings[x, LONGDESC],
                                           null,
                                           buildings[x, CITY],
                                           buildings[x, STATE],
                                           buildings[x, ZIP],
                                           buildings[x, COUNTRY],
                                           dLat,
                                           dLong,
                                           buildings[x, IMAGE],
                                           buildings[x, SVCS],
                                           buildings[x, VISIBLE]));
                } else {
                    List<string> temp = new List<string>(); temp.Add(buildings[x, ADDR]);
                    bldgs.Add(new Building(buildings[x, GUID],
                                           buildings[x, CODE],
                                           buildings[x, DESC],
                                           buildings[x, LOCN],
                                           buildings[x, BLDGTYPE],
                                           buildings[x, LONGDESC],
                                           temp,
                                           buildings[x, CITY],
                                           buildings[x, STATE],
                                           buildings[x, ZIP],
                                           buildings[x, COUNTRY],
                                           dLat,
                                           dLong,
                                           buildings[x, IMAGE],
                                           buildings[x, SVCS],
                                           buildings[x, VISIBLE]));
                }
            }
            return bldgs;
        }

        public IEnumerable<BuildingType> BuildingTypes
        {
            get
            {
                List<BuildingType> bldgTypes = new List<BuildingType>();
                bldgTypes.Add(new BuildingType("ADMIN", "Administration"));
                bldgTypes.Add(new BuildingType("CLASS", "Classrooms"));
                bldgTypes.Add(new BuildingType("SPORT", "Sport Facility"));
                return bldgTypes;
            }
        }

    }
}
