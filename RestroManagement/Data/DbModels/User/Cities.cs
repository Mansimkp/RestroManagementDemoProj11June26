using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestroManagement.DbModels.User
{
    [Table("Cities")]
    public class City
    {
        [Key]
        public int UniqueId { get; set; }
        public int StateId { get; set; }
        public string Name { get; set; } = string.Empty;
        [ForeignKey("StateId")]
        public State? State { get; set; }
    }
}
