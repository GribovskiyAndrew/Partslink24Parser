
namespace Partslink24Parser.Entities
{
    public class Point : BaseEntity
    {
        public int Left { get; set; }

        public int Top { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string Label { get; set; }

        public int MinorCategoryId { get; set; }

        public virtual MinorCategory MinorCategory { get; set; }

    }
}
