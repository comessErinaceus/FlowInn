using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FlowInn.API.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Sku { get; set; } = string.Empty;
        
        public decimal value { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [JsonIgnore]//Make the category category not result in inf. loop during json serialization.
        public virtual Category? Category { get; set; }

    }
}