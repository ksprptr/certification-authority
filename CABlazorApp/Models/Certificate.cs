using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CABlazorApp.Models
{
    public class Certificate
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Users")]
        public string User { get; set; }

        public byte[] publicKey { get; set; }

        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string path { get; set; }
    }
}
