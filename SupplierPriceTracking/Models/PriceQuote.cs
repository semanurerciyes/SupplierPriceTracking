namespace SupplierPriceTracking.Models
{
    public class PriceQuote
    {
        public int Id { get; set; }

        // Foreign Keyler
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        // Fiyat ve Geçerlilik Detayları
        public decimal Price { get; set; }
        public string Currency { get; set; } = "TRY";
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}