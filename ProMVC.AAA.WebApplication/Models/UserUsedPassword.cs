namespace ProMVC.AAA.WebApplication.Models
{
    public class UserUsedPassword
    {
        public int Id { get; set; }

        public string HashedPassword { get; set; }

        public virtual AppUser User { get; set; }
        public string UserId { get; set; }
    }
}