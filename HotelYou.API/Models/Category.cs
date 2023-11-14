namespace FlowInn.API.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Product> Products{get;set;}
    }
}