using System;
using System.ComponentModel.DataAnnotations;

namespace NotizApi.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}