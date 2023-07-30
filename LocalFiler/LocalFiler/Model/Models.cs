namespace Rugal.LocalFiler.Model
{
    public class LocalFileInfoModel
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public long Length { get; set; }
    }
    public class SyncDirectoryModel
    {
        public string Path { get; set; }
        public string FullPath { get; set; }
        public IEnumerable<LocalFileInfoModel> Files { get; set; }
        public IEnumerable<SyncDirectoryModel> Directories { get; set; }
        public bool TryGetDirectory(string FindPath, out SyncDirectoryModel OutDirectory)
        {
            OutDirectory = null;

            if (string.IsNullOrWhiteSpace(FindPath))
            {
                OutDirectory = this;
                return true;
            }

            FindPath = FindPath.ToLower();

            var GetDirectory = Directories
                .FirstOrDefault(Item => Item.Path.ToLower() == FindPath);

            if (GetDirectory is not null)
            {
                OutDirectory = GetDirectory;
                return true;
            }

            foreach (var Item in Directories)
            {
                if (Item.TryGetDirectory(FindPath, out var GetModel))
                {
                    OutDirectory = GetModel;
                    return true;
                }
            }

            return false;
        }
        public bool IsFileExist(string FileName, string FindPath)
        {
            if (!TryGetDirectory(FindPath, out var OutDirectory))
                return false;

            var IsExist = OutDirectory.Files.Any(Item => Item.FileName == FileName);
            return IsExist;
        }
    }
}