namespace cellphones_backend.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateOnly birthDay { get; set; }
    }
}
