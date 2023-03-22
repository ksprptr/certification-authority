using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CABlazorApp.Models
{
    public class Users
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }
    }
}
