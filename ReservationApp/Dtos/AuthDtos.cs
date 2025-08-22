using System.ComponentModel.DataAnnotations;

namespace ReservationApp.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]  
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalı.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Şifre en az 1 büyük harf, 1 küçük harf, 1 rakam ve 1 özel karakter içermelidir.")]
        public string Password { get; set; } = null!;
    }
    public record LoginDto(string Email, string Password);
    public record AuthResponse(string Token, string FullName, string Role);
}
