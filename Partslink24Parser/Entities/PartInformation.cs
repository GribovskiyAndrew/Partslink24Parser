using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partslink24Parser.Entities
{
    public class PartInformation : BaseEntity
    {
        public string PartNumber { get; set; }

        public string Description { get; set; }

        public string Price { get; set; }

        public string Type { get; set; }

        public int PartId { get; set; }

        public virtual Part Part { get; set; }
    }
}
