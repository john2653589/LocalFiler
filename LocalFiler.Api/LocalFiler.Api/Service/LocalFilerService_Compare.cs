namespace Rugal.LocalFiler.Service
{
    public partial class LocalFilerService
    {
        public virtual SyncDirectoryModel GetFileList()
        {
            var Info = new DirectoryInfo(Setting.RootPath);
            if (!Info.Exists)
                Info.Create();

            var Ret = RCS_GetFileList(Setting.RootPath);
            return Ret;
        }
        public virtual IEnumerable<LocalFileInfoModel> ForEachFiles(SyncDirectoryModel FileList = null)
        {
            FileList ??= GetFileList();
            if (FileList.Files.Any())
            {
                foreach (var File in FileList.Files)
                {
                    yield return File;
                }
            }
            foreach (var Dir in FileList.Directories)
            {
                foreach (var File in ForEachFiles(Dir))
                {
                    yield return File;
                }
            }
        }
        private SyncDirectoryModel RCS_GetFileList(string FindPath)
        {
            FindPath = FindPath?.Replace(@"\", "/");

            var FullPath = FindPath;
            var SetPath = "";
            if (FullPath != Setting.RootPath)
            {
                FullPath = CombinePaths(FindPath);
                SetPath = FindPath;
            }

            var FindDirectory = new DirectoryInfo(FullPath);
            var GetFiles = FindDirectory
                .GetFiles()
                .Select(Item => new LocalFileInfoModel()
                {
                    FileName = Item.Name,
                    Path = SetPath,
                    FullPath = FullPath,
                    Length = Item.Length,
                });

            var GetDirectories = FindDirectory
                .GetDirectories()
                .Select(Item =>
                {
                    var NextPath = Item.Name;
                    if (!string.IsNullOrWhiteSpace(SetPath))
                        NextPath = $"{SetPath}/{NextPath}";

                    var NextModel = RCS_GetFileList(NextPath);
                    return NextModel;
                });

            var Model = new SyncDirectoryModel()
            {
                Path = SetPath,
                FullPath = FullPath,
                Files = GetFiles,
                Directories = GetDirectories,
            };
            return Model;
        }

        public virtual void CompareFileList(SyncDirectoryModel MainModel, SyncDirectoryModel TargetModel, Action<LocalFileInfoModel> NotExistFunc)
              => RCS_CompareFileList(MainModel, TargetModel, NotExistFunc);

        private void RCS_CompareFileList(SyncDirectoryModel MainModel, SyncDirectoryModel TargetModel, Action<LocalFileInfoModel> NotExistFunc)
        {
            foreach (var File in MainModel.Files)
            {
                if (!TargetModel.IsFileExist(File.FileName, File.Path))
                {
                    NotExistFunc?.Invoke(File);
                }
            }

            foreach (var Directory in MainModel.Directories)
                RCS_CompareFileList(Directory, TargetModel, NotExistFunc);
        }

        public virtual bool IsFileExists<TData>(object FileName)
        {
            var IsExists = IsFileExists(typeof(TData), FileName);
            return IsExists;
        }
        public virtual bool IsFileExists(Type DataType, object FileName)
        {
            var IsExists = IsFileExists(DataType.Name, FileName);
            return IsExists;
        }
        public virtual bool IsFileExists(string DirectoryName, object FileName)
        {
            var FullFileName = CombineFullName(FileName, out _, new[] { DirectoryName });
            var IsExists = File.Exists(FullFileName);
            return IsExists;
        }
    }
}
