using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partslink24Parser.Entities
{
    public class Part : BaseEntity
    {
        public string? Position { get; set; }

        public string? PartNumber { get; set; }
        
        public string? Description { get; set; }
        
        public string? Remark { get; set; }
        
        public string? Unit { get; set; }

        public string? Model { get; set; }

        public string? Path { get; set; }

        public bool? Unavailable { get; set; }

        public string? ImageName { get; set; }

        public bool Done { get; set; }

        public int MinorCategoryId { get; set; }

        public virtual MinorCategory MinorCategory { get; set; }

        public virtual ICollection<PartInformation> PartInformations { get; set; }
    }
}
