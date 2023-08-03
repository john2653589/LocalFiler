using Microsoft.AspNetCore.Http;

namespace Rugal.LocalFiler.Model
{
    public class FilerSetting
    {
        public bool DefaultExtensionFromFile { get; set; }
        public bool UseExtension { get; set; }
        public string FormatRootPath { get; private set; }
        public string RootPath
        {
            get => GetRootPath();
            set => FormatRootPath = value;
        }
        public Dictionary<string, object> Paths { get; set; }
        public void AddPath(string Key, object Path)
        {
            Paths ??= new Dictionary<string, object>();
            Key = Key.ToLower();
            if (Paths.ContainsKey(Key))
                Paths[Key] = Path;
            else
                Paths.TryAdd(Key, Path);
        }
        private string GetRootPath()
        {
            var PathArray = FormatRootPath
                .Split('/')
                .Select(Item =>
                {
                    if (!Item.Contains('{') && !Item.Contains('}'))
                        return Item;

                    if (Paths is null)
                        return "null";

                    var PathKey = Item
                        .TrimStart('{')
                        .TrimEnd('}')
                        .ToLower();

                    if (!Paths.TryGetValue(PathKey, out var Path))
                        return "null";

                    var PathString = Path.ToString();
                    return PathString;
                });

            var GetRootPath = string.Join('/', PathArray);
            return GetRootPath;
        }
    }
    public class FilerInfo
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public long Length { get; set; }
    }
    public class SaveConfig
    {
        public string FileName { get; set; }
        public byte[] Buffer { get; set; }
        public IFormFile FormFile { get; set; }
        public IEnumerable<string> Paths { get; set; }
        public string Extension { get; set; }
        public bool HasExtension => !string.IsNullOrWhiteSpace(Extension);
        public SaveByType SaveBy => GetSaveBy();
        public SaveConfig() { }
        public SaveConfig(object _FileName, byte[] _Buffer)
        {
            FileName = _FileName.ToString();
            Buffer = _Buffer;
        }
        public SaveConfig(object _FileName, IFormFile _FormFile)
        {
            FileName = _FileName.ToString();
            FormFile = _FormFile;
        }

        public SaveConfig AddPath(string Path)
        {
            Paths ??= new List<string> { };
            var PathList = Paths is IList<string> IListPaths ? IListPaths : Paths.ToList();
            PathList.Add(Path);
            Paths = PathList;
            return this;
        }
        public SaveConfig UseFileExtension()
        {
            if (FormFile is null)
                return this;

            Extension = FormFile.Name.Split('.').Last();
            return this;
        }
        public SaveConfig WithFile(IFormFile File, bool UseExtension = true)
        {
            FormFile = File;
            if (UseExtension)
                UseFileExtension();
            return this;
        }
        public SaveConfig WithFile(IFormFile File)
        {
            FormFile = File;
            return this;
        }
        private SaveByType GetSaveBy()
        {
            if (Buffer is not null)
                return SaveByType.Buffer;

            if (FormFile is not null)
                return SaveByType.FormFile;

            return SaveByType.None;
        }
    }
    public class PathConfig
    {
        public string FileName { get; set; }
        public IEnumerable<string> Paths { get; set; }
        public PathConfig() { }
        public PathConfig(object _FileName)
        {
            FileName = _FileName.ToString();
        }
        public PathConfig(object _FileName, IEnumerable<string> _Paths = null) : this(_FileName)
        {
            AddPath(_Paths);
        }
        public PathConfig AddPath(string Path)
        {
            if (Path is null)
                return this;

            Paths ??= new List<string>();
            var PathList = Paths is IList<string> IListPaths ? IListPaths : Paths.ToList();
            PathList.Add(Path);
            Paths = PathList;
            return this;
        }
        public PathConfig AddPath(IEnumerable<string> Paths)
        {
            if (Paths is null)
                return this;

            foreach (var Item in Paths)
                AddPath(Item);
            return this;
        }
    }
    public enum SaveByType
    {
        None,
        Buffer,
        FormFile,
    }
}