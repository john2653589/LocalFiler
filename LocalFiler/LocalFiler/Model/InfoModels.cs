using Rugal.LocalFiler.Service;

namespace Rugal.LocalFiler.Model
{
    public class FilerInfo
    {
        private readonly Lazy<FolderInfo> _Folder;
        public readonly FilerService Filer;
        public ReadConfig Config { get; set; }
        public FileInfo BaseInfo { get; set; }
        public FolderInfo Folder => _Folder.Value;
        public string FileName => BaseInfo.Name;
        public bool IsExist => BaseInfo.Exists;
        public long Length => IsExist ? BaseInfo.Length : -1;
        public FilerInfo(FilerService _Filer, ReadConfig _Config, bool IsVerifyFileName)
        {
            Config = _Config;
            Filer = _Filer;

            var FullFileName = Filer.CombineRootFileName(Config, IsVerifyFileName);
            BaseInfo = new FileInfo(FullFileName);
            _Folder = new Lazy<FolderInfo>(() => GetFolder());
        }
        public FilerInfo(FilerService _Filer, ReadConfig _Config) : this(_Filer, _Config, false) { }
        

        public FilerInfo NextFile(PositionByType NextBy)
        {
            var Result = NextBy switch
            {
                PositionByType.Length => NextFileByLength(),
                PositionByType.Name => NextFileByName(),
                _ => null
            };
            return Result;
        }
        public FilerInfo PreviousFile(PositionByType PreviousBy)
        {
            var Result = PreviousBy switch
            {
                PositionByType.Length => PreviousByLength(),
                PositionByType.Name => PreviousByName(),
                _ => null
            };
            return Result;
        }
        public FilerInfo Clone()
        {
            var NewInfo = new FilerInfo(Filer, Config.Clone());
            return NewInfo;
        }
        public FilerWriter ToWriter()
        {
            var Writer = new FilerWriter(this);
            return Writer;
        }

        #region Public Process
        public int IndexOfBy(IEnumerable<FilerInfo> Files, PositionByType PositionBy = PositionByType.Name)
        {
            var FindFile = Files
                .Select((Item, Index) => new
                {
                    Info = Item,
                    Index
                })
                .FirstOrDefault(Item =>
                {
                    var IsFind = PositionBy switch
                    {
                        PositionByType.Name => Item.Info.FileName == FileName,
                        PositionByType.Length => Item.Info.Length == Length,
                        _ => false
                    };
                    return IsFind;
                });

            if (FindFile is null)
                return -1;

            return FindFile.Index;
        }
        #endregion

        #region Private Process
        private FolderInfo GetFolder()
        {
            var Result = new FolderInfo(Filer, Config);
            return Result;
        }
        private FilerInfo NextFileByLength()
        {
            var Result = Folder.Files
                .Where(Item => Item.Length > Length)
                .OrderBy(Item => Item.Length)
                .FirstOrDefault();
            return Result;
        }
        private FilerInfo NextFileByName()
        {
            var Files = Folder.Files
                .OrderBy(Item => Item.FileName)
                .ToArray();

            var Index = IndexOfBy(Files);
            Index++;

            if (Index >= Files.Length)
                return null;

            var Result = Files[Index];
            return Result;
        }
        private FilerInfo PreviousByLength()
        {
            var Result = Folder.Files
                .Where(Item => Item.Length < Length)
                .OrderBy(Item => Item.Length)
                .LastOrDefault();
            return Result;
        }
        private FilerInfo PreviousByName()
        {
            var Files = Folder.Files
                .OrderBy(Item => Item.FileName)
                .ToArray();

            var Index = IndexOfBy(Files);
            Index--;

            if (Index < 0)
                return null;

            var Result = Files[Index];
            return Result;
        }
        #endregion
    }
    public class FolderInfo
    {
        #region Lazy Property
        private readonly Lazy<IEnumerable<FilerInfo>> _Files;
        private readonly Lazy<IEnumerable<FolderInfo>> _Folders;
        private readonly Lazy<FolderInfo> _ParentFolder;
        private readonly Lazy<long> _TotalLength;
        #endregion
        public readonly FilerService Filer;
        public readonly PathConfig Config;
        public readonly DirectoryInfo Info;
        public FolderInfo ParentFolder => _ParentFolder.Value;
        public string FolderName => Info.Name;
        public IEnumerable<FilerInfo> Files => _Files.Value;
        public IEnumerable<FolderInfo> Folders => _Folders.Value;
        public long TotalLength => _TotalLength.Value;
        public FolderModeType FolderMode { get; set; } = FolderModeType.Dynamic;
        public FolderInfo(FilerService _Filer, PathConfig _Config)
        {
            Config = _Config;
            Filer = _Filer;

            var FullPath = Filer.CombineRootPaths(Config);
            Info = new DirectoryInfo(FullPath);

            _Files = new Lazy<IEnumerable<FilerInfo>>(() => GetFiles());
            _Folders = new Lazy<IEnumerable<FolderInfo>>(() => GetFolders());
            _ParentFolder = new Lazy<FolderInfo>(() => GetParentFolder());
            _TotalLength = new Lazy<long>(() => GetTotalLength());
        }
        public FolderInfo NextFolder(PositionByType NextBy)
        {
            var Result = NextBy switch
            {
                PositionByType.Length => NextFolderByLength(),
                PositionByType.Name => NextFolderByName(),
                _ => null
            };
            return Result;
        }
        public FolderInfo PreviousFolder(PositionByType PreviousBy)
        {
            var Result = PreviousBy switch
            {
                PositionByType.Length => PreviousByLength(),
                PositionByType.Name => PreviousByName(),
                _ => null
            };
            return Result;
        }
        public FolderInfo WithMode(FolderModeType _FolderMode)
        {
            FolderMode = _FolderMode;
            return this;
        }
        public FilerInfo InfoFile(string FileName)
        {
            var FileConfig = new ReadConfig()
                .WithConfig(Config)
                .WithFileName(FileName);

            var NewInfo = new FilerInfo(Filer, FileConfig);
            return NewInfo;
        }
        private IEnumerable<FilerInfo> GetFiles()
        {
            var Files = Info.GetFiles()
                .Select(FileInfo =>
                {
                    var FileConfig = new ReadConfig()
                        .WithConfig(Config)
                        .WithFileName(FileInfo.Name);

                    var GetInfo = new FilerInfo(Filer, FileConfig);
                    return GetInfo;
                });

            if (FolderMode == FolderModeType.Static)
            {
                Files = Files.ToArray();
                return Files;
            }

            return Files;
        }
        private IEnumerable<FolderInfo> GetFolders()
        {
            var Folders = Info.EnumerateDirectories()
                .Select(FolderInfo =>
                {
                    var FolderConfig = new PathConfig()
                        .AddPath(Config.Paths)
                        .AddPath(FolderInfo.Name);

                    var GetInfo = new FolderInfo(Filer, FolderConfig);
                    return GetInfo;
                });

            if (FolderMode == FolderModeType.Static)
            {
                Folders = Folders.ToArray();
                return Folders;
            }

            return Folders;
        }
        private long GetTotalLength()
        {
            var FilesSum = Files.Sum(Item => Item.Length);
            var FoldersSum = Folders.Sum(Item => Item.TotalLength);
            var Result = FilesSum + FoldersSum;
            return Result;
        }
        private FolderInfo GetParentFolder()
        {
            var FolderConfig = new PathConfig()
                .AddPath(Config.Paths)
                .SkipLast();

            var Result = new FolderInfo(Filer, FolderConfig);
            return Result;
        }
        private FolderInfo NextFolderByLength()
        {
            var Result = ParentFolder.Folders
                .Where(Item => Item.TotalLength > TotalLength)
                .OrderBy(Item => Item.TotalLength)
                .FirstOrDefault();
            return Result;
        }
        private FolderInfo NextFolderByName()
        {
            var Folders = ParentFolder.Folders
                .OrderBy(Item => Item.FolderName);

            var Index = IndexOfBy(Folders);
            Index++;

            if (Index >= Folders.Count())
                return null;

            var Result = Folders
                .Skip(Index)
                .First();

            return Result;
        }
        private FolderInfo PreviousByLength()
        {
            var Result = ParentFolder.Folders
                .Where(Item => Item.TotalLength < TotalLength)
                .OrderBy(Item => Item.TotalLength)
                .LastOrDefault();
            return Result;
        }
        private FolderInfo PreviousByName()
        {
            var Folders = ParentFolder.Folders
                .OrderBy(Item => Item.FolderName);

            var Index = IndexOfBy(Folders);
            Index--;

            if (Index < 0)
                return null;

            var Result = Folders
                .Skip(Index)
                .First();
            return Result;
        }
        public int IndexOfBy(IEnumerable<FolderInfo> Folders, PositionByType PositionBy = PositionByType.Name)
        {
            var FindFolder = Folders
                .Select((Item, Index) => new
                {
                    Info = Item,
                    Index
                })
                .FirstOrDefault(Item =>
                {
                    var IsFind = PositionBy switch
                    {
                        PositionByType.Name => Item.Info.FolderName == FolderName,
                        PositionByType.Length => Item.Info.TotalLength == TotalLength,
                        _ => false
                    };
                    return IsFind;
                });

            if (FindFolder is null)
                return -1;

            return FindFolder.Index;
        }
    }
    public enum PositionByType
    {
        Name,
        Length,
    }
    public enum FolderModeType
    {
        Static,
        Dynamic,
    }
}