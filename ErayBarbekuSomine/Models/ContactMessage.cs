using System;
using System.ComponentModel.DataAnnotations;

namespace ErayBarbekuSomine.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "Email en fazla 100 karakter olabilir.")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Konu alanı zorunludur.")]
        [StringLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir.")]
        public string Subject { get; set; }
        
        [Required(ErrorMessage = "Mesaj alanı zorunludur.")]
        [StringLength(2000, ErrorMessage = "Mesaj en fazla 2000 karakter olabilir.")]
        public string Message { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsRead { get; set; } = false;
    }
}
