using System.ComponentModel.DataAnnotations.Schema;

namespace Partslink24Parser.Entities
{
    public class MinorCategory : BaseEntity
    {
        public string SubGroup { get; set; }

        public string Illustration { get; set; }

        public string Description { get; set; }
        
        public string? Remark { get; set; }
        
        public string? Model { get; set; }

        public bool Done { get; set; }

        public int MajorCategoryId { get; set; }

        public virtual MajorCategory MajorCategory { get; set; }

        public virtual ICollection<Part> Parts { get; set; }

        [NotMapped]
        public string Path { get; set; }
    }
}
