using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Рақами телефон ҳатмист")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ном ҳатмист")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ном аз 2 то 50 ҳарф бояд бошад")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Насаб ҳатмист")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Насаб аз 2 то 50 ҳарф бояд бошад")]
    public string LastName { get; set; } = string.Empty;
}
