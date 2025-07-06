using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker_Application.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage ="Title is required.")]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        [MaxLength(5)]
        public string Icon { get; set; } = "";

        [Required]
        [MaxLength(10)]
        public string Type { get; set; } = "Expense";

        [NotMapped]
        public string? TitleWithIcon
        {
            get
            {
                return this.Icon + " " + this.Title;
            }
        }
    }
}

