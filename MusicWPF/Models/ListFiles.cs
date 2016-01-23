namespace MusicWPF.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ListFiles
    {
        public int Id { get; set; }

        public int FileId { get; set; }

        public int ListId { get; set; }

        public bool IsChecked { get; set; }

        public virtual MusicFile MusicFile { get; set; }

        public virtual PlayList PlayList { get; set; }
    }
}
