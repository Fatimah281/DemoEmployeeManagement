
using System.ComponentModel.DataAnnotations;
namespace BaseLibrary.DTOs
{
    public class Register: AccountBase
    {
        [Required]
        [MaxLength(100)]
        [MinLength(5)]
        public string? FullName { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Required]
        public string? ConfirmPassword { get; set; }
    }
}
