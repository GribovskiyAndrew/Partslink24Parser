﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partslink24Parser.Entities
{
    public class MajorCategory : BaseEntity
    {
        public string Type { get; set; }

        public bool Done { get; set; }

        public int VehicleDataId { get; set; }

        public virtual VehicleData VehicleData { get; set; }

        public virtual ICollection<MinorCategory> MinorCategories { get; set; }

        [NotMapped]
        public string Path { get; set; }
    }
}
