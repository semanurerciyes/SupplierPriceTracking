namespace SupplierPriceTracking.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // İlişki (Navigation Property)
        public ICollection<PriceQuote> PriceQuotes { get; set; } = new List<PriceQuote>();
    }
}