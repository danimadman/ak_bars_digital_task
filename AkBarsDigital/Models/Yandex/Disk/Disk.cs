namespace AkBarsDigital.Models.Disk
{
    public class Disk
    {
        public bool unlimited_autoupload_enabled { get; set; }
        public long max_file_size { get; set; }
        public long total_space { get; set; }
        public long trash_size { get; set; }
        public bool is_paid { get; set; }
        public bool used_space { get; set; }
        public SystemFolders system_folders { get; set; }
        public User user { get; set; }
        public long revision { get; set; }
    }
}