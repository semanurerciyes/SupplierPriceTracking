using System.ComponentModel.DataAnnotations; // Validasyon kuralları için bu kütüphane şart

namespace SupplierPriceTracking.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tedarikçi Adı alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Tedarikçi adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ülke alanı zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ülke adı 2 ile 50 karakter arasında olmalıdır.")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "İletişim Bilgisi alanı zorunludur.")]
        [StringLength(200, ErrorMessage = "İletişim bilgisi en fazla 200 karakter olabilir.")]
        public string ContactInfo { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // İlişki (Navigation Property)
        public ICollection<PriceQuote> PriceQuotes { get; set; } = new List<PriceQuote>();
    }
}