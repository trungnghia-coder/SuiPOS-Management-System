using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(20)]
        public string Name { get; set; } = "Cashier";
    }
}
