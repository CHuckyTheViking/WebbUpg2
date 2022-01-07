using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebbUpg2.Models
{
    public class DataFile
    {

        public Guid Id { get; set; }
        public string UnTrustedName { get; set; }
        public DateTime TimeStamp { get; set; }
        public long Size { get; set; }
        public byte[] Content { get; set; }
    }
}
