using System.ComponentModel.DataAnnotations;

namespace RestroManagement.ViewModels
{
    public class profile1
    {
        [Key]
        public int Id { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImage { get; set; }

    }
}
