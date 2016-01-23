using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace MusicWPF.Models
{
    [Table("DirectoryList")]
    public partial class DirectoryList
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(500)]
        public string Name { get; set; }
        [Required]
        [StringLength(500)]
        public string FullName { get; set; }
        [Required]
        [StringLength(500)]
        public string ClassName { get; set; }

        private DirectoryInfo di;
        [NotMapped]
        public DirectoryInfo DirectoryInfo
        {
            get
            {
                if (di == null)
                    di = new DirectoryInfo(FullName);
                return di;
            }
        }
    }
}
