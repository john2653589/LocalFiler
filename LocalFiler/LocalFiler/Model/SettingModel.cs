namespace Rugal.LocalFiler.Model
{
    public partial class LocalFileManagerSetting
    {
        internal string FormatRootPath { get; private set; }
        public string RootPath
        {
            get => GetRootPath();
            set => FormatRootPath = value;
        }
        public string RemoteServer { get; set; }
        public TimeSpan? SyncPerMin { get; set; }
        public long SyncToPerByte { get; set; } = 1024 * 1024;
        public SyncWayType SyncWay { get; set; } = SyncWayType.None;
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

    public enum SyncWayType
    {
        None,
        ToServer,
        FromServer,
        Trade,
    }

}
