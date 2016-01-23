namespace MusicWPF.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PlayList")]
    public partial class PlayList
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PlayList()
        {
            ListFiles = new HashSet<ListFiles>();
        }

        [Key]
        public int ListId { get; set; }

        [Required]
        [StringLength(10)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Tag { get; set; }

        public bool Enabled { get; set; }

        [StringLength(500)]
        public string Source { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ListFiles> ListFiles { get; set; }
    }
}
