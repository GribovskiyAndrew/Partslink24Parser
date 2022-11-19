using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partslink24Parser.Entities
{
    public class MajorCategory : BaseEntity
    {
        public string Type { get; set; }

        public bool Done { get; set; }

        public int VariantId { get; set; }

        public virtual VehicleData VehicleData { get; set; }

        public virtual ICollection<MinorCategory> MinorCategories { get; set; }
    }
}
