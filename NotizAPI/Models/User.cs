using System.ComponentModel.DataAnnotations;

namespace NotizApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = null!;

        [Required]
        public byte[] PasswordHash { get; set; } = null!;

        [Required]
        public byte[] Salt { get; set; } = null!;

        public string? Email { get; set; }
    }
}