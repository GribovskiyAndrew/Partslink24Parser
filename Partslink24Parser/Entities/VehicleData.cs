
namespace Partslink24Parser.Entities
{
    public class VehicleData : BaseEntity
    {
        public string VinNumber { get; set; }

        public string Model { get; set; }

        public string DateOfProduction { get; set; }

        public int Year { get; set; }

        public string SalesType { get; set; }

        public string EngineCode { get; set; }

        public string TransmissionCode { get; set; }

        public string AxleDrive { get; set; }

        public string Equipment { get; set; }

        public string RoofColor { get; set; }

        public string ExteriorColorAndPaintCode { get; set; }

        public bool Done { get; set; }

        public virtual ICollection<MajorCategory> MajorCategories { get; set; }

    }
}
