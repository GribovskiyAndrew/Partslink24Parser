using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partslink24Parser.Entities
{
    public class VinNumber : BaseEntity
    {
        public string Vin { get; set; }

        //public int? VehicleDataId { get; set; }

        public bool Done { get; set; }
    }
}
