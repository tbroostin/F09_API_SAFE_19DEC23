using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Base.Tests
{

    public class TestImportantNumberRepository : IImportantNumberRepository {
        private const int ID = 0;
        private const int NAME = 1;
        private const int CITY = 2;
        private const int STATE = 3;
        private const int ZIP = 4;
        private const int COUNTRY = 5;
        private const int PHONE = 6;
        private const int EXT = 7;
        private const int CATEGORY = 8;
        private const int EMAIL = 9;
        private const int ADDRESS = 10;
        private const int BLDG = 11;
        private const int EXPORT = 12;
        private const int LATITUDE = 13;
        private const int LONGITUDE = 14;
        private const int LOCATION = 15;

        private string[,] impNums = { // ID,  Name       City      State    Zip   Country     Phone      Ext    Category  Email             Address          Bldg   Export  Latitude     Longitude      Location
                                        {"1", "Police",  "Fairfax", "VA", "22033", "USA", "703-111-1111", "444", "EMER",  "help@police.com", "123 Main St",    "AND", "Y",   "45.123456", "-114.321312", "MC"},
                                        {"2", "Fire",    "Fairfax", "VA", "22033", "USA", "703-111-2222", "",    "EMER",  "help@fire.com",   "125 Main St",    "",    "Y",   "",          "",            ""},
                                        {"3", "Joe's",   "Vienna",  "VA", "22180", "USA", "703-222-3333", "",    "FOOD",  "pizza@joes.com",  "234 Maple Ave",  "",    "Y",   "",          "",            ""},
                                        {"4", "RH&B",    "Fairfax", "VA", "22033", "USA", "703-333-4444", "",    "FOOD",  "",                "345 Main St",    "",    "Y",   "77.123456", "-132.234234", "MC"},
                                        {"5", "Admin",   "",        "",   "",      "",    "703-999-9999", "123", "ADMIN", "admin@some.edu",  "545 2nd Street", "EIN", "Y",   "",          "",            ""},
                                        {"6", "Bursar",  "",        "",   "",      "",    "703-888-8888", "777", "ADMIN", "bursar@some.edu", "547 2nd Street", "DIN", "N",   "",          "",            ""},
                                        {"7", "Minimal", "",        "",   "",      "",    "703-555-5555", "",    "ADMIN", "",                "",               "",    "Y",   "",          "",            ""},
                                    };

        public IEnumerable<ImportantNumber> Get() {
            var impn = new List<ImportantNumber>();
            var items = impNums.Length / (LOCATION + 1);
            for (int x = 0; x < items; x++) {
                List<string> lAddr = new List<string>(); lAddr.Add(impNums[x,ADDRESS]);
                decimal dLat;  Decimal.TryParse(impNums[x, LATITUDE], out dLat);
                decimal dLong; Decimal.TryParse(impNums[x, LONGITUDE], out dLong);
                if (!string.IsNullOrEmpty(impNums[x, ADDRESS])) {
                    impn.Add(new ImportantNumber(impNums[x, ID],
                                                 impNums[x, NAME],
                                                 impNums[x, PHONE],
                                                 impNums[x, EXT],
                                                 impNums[x, CATEGORY],
                                                 impNums[x, EMAIL],
                                                 impNums[x, BLDG],
                                                 impNums[x, EXPORT]));
                } else {
                    impn.Add(new ImportantNumber(impNums[x, ID],
                                                 impNums[x, NAME],
                                                 impNums[x, CITY],
                                                 impNums[x, STATE],
                                                 impNums[x, ZIP],
                                                 impNums[x, COUNTRY],
                                                 impNums[x, PHONE],
                                                 impNums[x, EXT],
                                                 impNums[x, CATEGORY],
                                                 impNums[x, EMAIL],
                                                 lAddr,
                                                 impNums[x, EXPORT],
                                                 dLat,
                                                 dLong,
                                                 impNums[x, LOCATION]));
                }
            }
            return impn;
        }

        public IEnumerable<ImportantNumberCategory> ImportantNumberCategories
        {
            get {
                List<ImportantNumberCategory> impCats = new List<ImportantNumberCategory>();
                impCats.Add(new ImportantNumberCategory("ADMIN", "Administrative"));
                impCats.Add(new ImportantNumberCategory("FOOD",  "Diners & Dives"));
                impCats.Add(new ImportantNumberCategory("EMER",  "Emergency"));
                return impCats;
            }
        }

    }
}
