using System.ComponentModel.DataAnnotations;

namespace LoginMVCProject.Models
{
    
    public class PasswordLogHistory
    {
        [Key]
        public ulong Id { get; set; }

        [Required]
        [Display(Name = "UserId")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "PasswordHash")]
        public string PasswordHash { get; set; }

        [Display(Name = "Expire Date")]
        [DataType(DataType.Date)]
        public DateTime ExpireDate { get; set; }


        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created AT")]
        [DataType(DataType.Date)]
        public DateTime CreatedAT { get; set; }
    }
}
