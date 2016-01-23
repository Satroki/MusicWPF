namespace MusicWPF.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.ComponentModel;

    [Table("MusicFile")]
    public partial class MusicFile : SatrokiLibrary.MVVM.ViewModelBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MusicFile()
        {
            ListFiles = new HashSet<ListFiles>();
        }

        [Key]
        public int FileId { get; set; }

        [Required]
        [StringLength(500)]
        public string Name { get; set; }

        private string title;
        [StringLength(500)]
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private string artist;
        [StringLength(500)]
        public string Artist
        {
            get { return artist; }
            set { SetProperty(ref artist, value); }
        }

        private string album;
        [StringLength(500)]
        public string Album
        {
            get { return album; }
            set { SetProperty(ref album, value); }
        }

        [Required]
        [StringLength(500)]
        public string FullName { get; set; }

        public string Lyric { get; set; }

        public double? FileSize_MB { get; set; }

        public double? TotalSeconds { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ListFiles> ListFiles { get; set; }

        [NotMapped]
        public string DisplayName => string.IsNullOrWhiteSpace(Title) ? Name : Title;

        private LyricInfo li;
        [NotMapped]
        public LyricInfo LyricInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Lyric))
                    return null;
                if (li == null)
                    li = new LyricInfo(Lyric);
                return li;
            }
        }

        [NotMapped]
        public TimeSpan Duration
        {
            get { return TimeSpan.FromSeconds(TotalSeconds ?? 0); }
            set { TotalSeconds = value.TotalSeconds; OnPropertyChanged(nameof(Duration)); }
        }

        [NotMapped]
        public string HasLrc => string.IsNullOrWhiteSpace(Lyric) ? string.Empty : "[L]";

        public void ReloadLrc()
        {
            li = new LyricInfo(Lyric);
            OnPropertyChanged(nameof(HasLrc));
        }
    }
}
