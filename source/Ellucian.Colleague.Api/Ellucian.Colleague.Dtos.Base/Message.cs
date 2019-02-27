using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    class Message
    {
        public Message() : base()
        {
        }
        //returns unique id of a message
        public String Id { get; set; }
        //returns title of message
        public String Category { get; set; }
        //returns String description of notification task
        public String Description { get; set; }
        //returns 0,1,2 for read/unread/deleted
        public String ProcessLink { get; set; }

    }
}
