namespace ConsoleApplication1
{
    public class DriveInformation
    {
        public string Name { get; set; }
        public bool IsReady { get; set; }
        public string DriveType { get; set; }
        public string FileSystem { get; set; }
        public long AvailableFreeSpace { get; set; }
        public long TotalSize { get; set; }
    }
}