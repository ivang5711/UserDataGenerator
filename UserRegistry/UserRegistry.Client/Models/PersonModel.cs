namespace UserRegistry.Client.Models
{
    public record PersonModel
    {
        public int Number {  get; set; }
        public int Id { get; set; }
        public string Gender { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        public CreditRating Rating { get; set; }
    }
     
    
    
}
