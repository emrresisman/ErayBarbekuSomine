using System;
using System.ComponentModel.DataAnnotations;

namespace ErayBarbekuSomine.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty; // e.g., "TruvaBarbeku", "DuzCepheliSomine"

        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}
