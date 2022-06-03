using System.ComponentModel.DataAnnotations;

namespace WebAdvert.Web.Models.Accounts
{
    public class ConfirmModel
    {
        [Required(ErrorMessage = "Email is Required")]
        [Display(Name ="Email")]
        [EmailAddress]
        public string  Email { get; set; }

        [Required(ErrorMessage ="Code is Required")]
        public string Code { get; set; }
    }
}
