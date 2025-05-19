using System.ComponentModel.DataAnnotations;

namespace NotizApi.Models.Dtos
{
    public class CreateNoteDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        public string? Content { get; set; }
    }
}