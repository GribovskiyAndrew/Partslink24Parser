using System.ComponentModel.DataAnnotations;

namespace Partslink24Parser.Entities
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
