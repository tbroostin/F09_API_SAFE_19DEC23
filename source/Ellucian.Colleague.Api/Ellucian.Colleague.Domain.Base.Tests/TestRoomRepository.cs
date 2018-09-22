using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestRoomRepository
    {
        private string[,] roomArray = {
            {"JCL*6",   "JCL","6"},
            {"WENG*17", "WENG","17"},
            {"DOLI*100",    "DOLI","100"},
            {"LEE*700","LEE","700"},
            {"CARV*310","CARV","310"},
            {"ALV*03","ALV","03"},
            {"LEE*401","LEE","401"},
            {"HHS*231","HHS","231"},
            {"BANN*130","BANN","130"},
            {"BANL*120","BANL","120"},
            {"BRWN*110","BRWN","110"},
            {"SCI*100A","SCI","100A"},
            {"CARV*TBOX","CARV","TBOX"},
            {"MOON*103","MOON","103"},
            {"320*GMR","320","GMR"},
            {"WAS2*130","WAS2","130"},
            {"JCL*7","JCL","7"},
            {"LEE*701","LEE","701"},
            {"DOLI*400","DOLI","400"},
            {"HHS*232","HHS","232"},
            {"GTT*A","GTT","A"},
            {"WENG*100","WENG","100"},
            {"WENG*2001","WENG","2001"},
            {"DEAN*A555","DEAN","A555"},
            {"MOON*104","MOON","104"},
            {"SUN*101","SUN","101"},
            {"ALV*04","ALV","04"},
            {"PFLD*WRKSH","PFLD","WRKSHP"},
            {"BC*100","BC","100"},
            {"ALV1*01","ALV1","01"},
            {"CS*0128","CS","0128"},
            {"HHS*233","HHS","233"},
            {"ALV*05","ALV","05"},
            {"GTT*B","GTT","B"},
            {"AJG*200","AJG","200"},
            {"BARK*1","BARK","1"},
            {"LEE*702","LEE","702"},
            {"CARV*01","CARV","01"},
            {"RMAS*200","RMAS","200"},
            {"AND*201","AND","201"},
            {"LEE*610","LEE","610"},
            {"GREN*200","GREN","200"},
            {"SCI*TEST","SCI","TEST"},
            {"MOON*105","MOON","105"},
            {"PAS*2010","PAS","2010"},
            {"SUN*102","SUN","102"},
            {"SZDO*100","SZDO","100"},
            {"HHS*234","HHS","234"},
            {"JACK*100","JACK","100"},
            {"GTT*C","GTT","C"},
            {"ALV*06","ALV","06"},
            {"CARV*KAT8","CARV","KAT8"},
            {"WAS2*110","WAS2","110"},
            {"BANL*100","BANL","100"},
            {"CARV*BMA","CARV","BMA"},
            {"LEE*680","LEE","680"},
            {"SCI*JYP","SCI","JYP"},
            {"CARV*02","CARV","02"},
            {"ALV1*02","ALV1","02"},
            {"MOON*106","MOON","106"},
            {"SUN*103","SUN","103"},
            {"LEE*SRV","LEE","SRV"},
            {"GREN*SMG","GREN","SMG"},
            {"HHS*235","HHS","235"},
            {"CARV*03","CARV","03"},
            {"ALV*07","ALV","07"},
            {"GREN*110","GREN","110"},
            {"BRKH*105","BRKH","105"},
            {"PAS*1000","PAS","1000"},
            {"CARV*CONF","CARV","CONF"},
            {"SUN*104","SUN","104"},
            {"WEBB*100","WEBB","100"},
            {"THOM*300","THOM","300"},
            {"WENG*6030","WENG","6030"},
            {"CARV*200","CARV","200"},
            {"AUS*100","AUS","100"},
            {"SCI*100","SCI","100"},
            {"NRAD*200","NRAD","200"},
            {"WENG*1200","WENG","1200"},
            {"GTT*123456","GTT","12345678"},
            {"MC*229A","MC","229A"},
            {"CDC*101","CDC","101"},
            {"COE*0100","COE","0100"},
            {"SMGD*300","SMGD","300"},
            {"HAM*202","HAM","202"},
            {"DARN*101","DARN","101"},
            {"THOM*600","THOM","600"},
            {"CARV*500","CARV","500"},
            {"WASS*140","WASS","140"},
            {"LEE*660","LEE","660"},
            {"GOSH*202","GOSH","202"},
            {"CARV*MAIN","CARV","MAIN"},
            {"CDC*102","CDC","102"},
            {"BRKH*107","BRKH","107"},
            {"COE*0101","COE","0101"},
            {"MOON*201","MOON","201"},
            {"BC*105","BC","105"},
            {"GOSH*GOLLY","GOSH","GOLLY"},
            {"CARV*CLAB","CARV","CLAB"},
            {"HHS*100","HHS","100"}};

        public IEnumerable<Room> Get()
        {
            var rooms = new List<Room>();

            // There are 3 fields for each room in the array
            var items = roomArray.Length / 3;

            for (int x = 0; x < items; x++)
            {
                var guid = Guid.NewGuid().ToString();
                rooms.Add(new Room(guid, roomArray[x, 0], roomArray[x, 1]));
            }
            return rooms;
        }

    }
}
