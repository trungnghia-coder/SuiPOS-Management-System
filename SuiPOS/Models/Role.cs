using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }
        [Required, StringLength(20)]
        public string Name { get; set; }
    }
}
