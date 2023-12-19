namespace UserRegistry.Client.Models
{
    public class User(int id, string name, string guid, string address, string phoneNumber)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;

        public string Guid { get; set; } = guid;

        public string Address { get; set; } = address;

        public string PhoneNumber { get; set; } = phoneNumber;
    }
}