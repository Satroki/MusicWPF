namespace MusicWPF.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MDModel : DbContext
    {
        public MDModel()
            : base("name=MDModel")
        {
        }

        public virtual DbSet<ListFiles> ListFiles { get; set; }
        public virtual DbSet<MusicFile> MusicFile { get; set; }
        public virtual DbSet<PlayList> PlayList { get; set; }
        public virtual DbSet<DirectoryList> DirectoryList { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MusicFile>()
                .HasMany(e => e.ListFiles)
                .WithRequired(e => e.MusicFile)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PlayList>()
                .HasMany(e => e.ListFiles)
                .WithRequired(e => e.PlayList)
                .WillCascadeOnDelete(false);
        }

        private static MDModel model = new MDModel();
        [NotMapped]
        public static MDModel Model => model;
    }
}
