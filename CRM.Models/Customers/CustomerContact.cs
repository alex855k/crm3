namespace CRM.Models
{
    public class CustomerContact
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}