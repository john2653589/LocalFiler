using Rugal.LocalFiler.Model;
using Microsoft.AspNetCore.Http;

namespace Rugal.LocalFiler.Service
{
    public partial class LocalFileService
    {
        public LocalFileManagerSetting Setting;
        public LocalFileService(LocalFileManagerSetting _Setting)
        {
            Setting = _Setting;
        }

        #region Public Method

        #region File Save
        public virtual LocalFileService TransferSave<TData>(IEnumerable<TData> Datas, Func<TData, byte[]> ExtractBuffer, Func<TData, object> GetFileName, Action<TData, string> SetFileNameFunc)
        {
            foreach (var Item in Datas)
            {
                var GetBuffer = ExtractBuffer.Invoke(Item);
                var FileName = GetFileName.Invoke(Item).ToString();
                var SetFileName = LocalSave<TData>(FileName, GetBuffer);
                SetFileNameFunc.Invoke(Item, SetFileName);
            }
            return this;
        }
        public virtual string SaveFile<TData>(object FileName, byte[] SaveBuffer, params object[] Paths)
           => LocalSave<TData>(FileName.ToString(), SaveBuffer, Paths);
        public virtual string SaveFile(object FileName, byte[] SaveBuffer, params object[] Paths)
            => LocalSave(FileName.ToString(), SaveBuffer, Paths);
        public virtual string SaveFile(object FileName, IFormFile File, params object[] Paths)
            => LocalSave(FileName.ToString(), File, Paths);
        public virtual string SaveFile<TData>(object FileName, string Extension, byte[] SaveBuffer, params object[] Paths)
            => LocalSave<TData>(CombineExtension(FileName, Extension), SaveBuffer, Paths);
        public virtual string SaveFile(object FileName, string Extension, byte[] SaveBuffer, params object[] Paths)
            => LocalSave(CombineExtension(FileName, Extension), SaveBuffer, Paths);
        public virtual string SaveFile(object FileName, string Extension, IFormFile File, params object[] Paths)
             => LocalSave(CombineExtension(FileName, Extension), File, Paths);
        #endregion

        #region File Read
        public virtual byte[] ReadFile<TData>(object FileName, params object[] Paths)
        {
            var FindPaths = Paths.ToList();
            FindPaths.Add(typeof(TData).Name);
            var FileBuffer = BaseReadFile(FileName, FindPaths);
            return FileBuffer;
        }
        public virtual byte[] ReadFile(Type DataType, object FileName, params object[] Paths)
        {
            var FindPaths = Paths.ToList();
            FindPaths.Add(DataType.Name);
            var FileBuffer = BaseReadFile(FileName, FindPaths);
            return FileBuffer;
        }
        public virtual byte[] ReadFile(object FileName, params object[] Paths)
        {
            var FileBuffer = BaseReadFile(FileName, Paths);
            return FileBuffer;
        }
        private byte[] BaseReadFile(object FileName, IEnumerable<object> Paths)
        {
            if (FileName is null)
                return Array.Empty<byte>();

            var FullFileName = CombineFullName(FileName, out _, Paths);
            if (!File.Exists(FullFileName))
                return Array.Empty<byte>();

            var FileBuffer = File.ReadAllBytes(FullFileName);
            return FileBuffer;
        }

        public virtual Task<byte[]> ReadFileAsync<TData>(object FileName, params object[] Paths)
        {
            var FindPaths = Paths.ToList();
            FindPaths.Add(typeof(TData).Name);
            var FileBuffer = BaseReadFileAsync(FileName, FindPaths);
            return FileBuffer;
        }
        public virtual Task<byte[]> ReadFileAsync(Type DataType, object FileName, params object[] Paths)
        {
            var FindPaths = Paths.ToList();
            FindPaths.Add(DataType.Name);
            var FileBuffer = BaseReadFileAsync(FileName, FindPaths);
            return FileBuffer;
        }
        public virtual Task<byte[]> ReadFileAsync(object FileName, params object[] Paths)
        {
            var FileBuffer = BaseReadFileAsync(FileName, Paths);
            return FileBuffer;
        }
        private Task<byte[]> BaseReadFileAsync(object FileName, IEnumerable<object> Paths)
        {
            if (FileName is null)
                return Task.FromResult(Array.Empty<byte>());

            var FullFileName = CombineFullName(FileName, out _, Paths);
            if (!File.Exists(FullFileName))
                return Task.FromResult(Array.Empty<byte>());

            var FileBuffer = File.ReadAllBytesAsync(FullFileName);
            return FileBuffer;
        }
        #endregion

        #region File Compare
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
        #endregion

        #region File Delete
        public virtual bool DeleteFile<TData>(IEnumerable<object> FileNames)
        {
            var IsDelete = true;
            foreach (var Item in FileNames)
                IsDelete = IsDelete && DeleteFile(typeof(TData), Item);
            return IsDelete;
        }
        public virtual bool DeleteFile<TData, TColumn>(IEnumerable<TData> FileDatas, Func<TData, TColumn> GetColumnFunc)
        {
            var IsDelete = true;
            foreach (var Item in FileDatas)
                IsDelete = IsDelete && DeleteFile(typeof(TData), GetColumnFunc(Item));
            return IsDelete;
        }
        public virtual bool DeleteFile<TData>(object FileName)
        {
            var IsDelete = DeleteFile(typeof(TData), FileName);
            return IsDelete;
        }
        public virtual bool DeleteFile(Type DataType, object FileName)
        {
            var IsDelete = DeleteFile(DataType.Name, FileName);
            return IsDelete;
        }
        public virtual bool DeleteFile(string DirectoryName, object FileName)
        {
            if (FileName is null)
                return false;

            var FullFileName = CombineFullName(FileName, out _, new[] { DirectoryName });
            if (!File.Exists(FullFileName))
                return false;

            File.Delete(FullFileName);
            var IsDelete = !File.Exists(FullFileName);
            return IsDelete;
        }
        #endregion

        #region Combine Path
        public virtual string CombinePaths(params string[] Paths)
        {
            var FullFileName = CombineFullPath(Paths);
            return FullFileName;
        }
        public virtual string CombinePaths(LocalFileInfoModel Model)
        {
            var FullFileName = CombineFullPath(Model.Path, Model.FileName);
            return FullFileName;
        }
        #endregion

        #endregion

        #region Internal Function
        internal virtual string ConvertFullName(object FileName, IEnumerable<object> Paths)
        {
            var FullFileName = CombineFullName(FileName, out _, Paths);
            return FullFileName;
        }
        internal string CombineFullName(object FileName, out string SetFileName, IEnumerable<object> Paths)
        {
            SetFileName = ConvertFileName(FileName);

            var ClearPaths = new[] { Setting.RootPath }.ToList();

            var ConvertPaths = Paths?
                .Select(Item => Item?.ToString().TrimStart('/').TrimEnd('/').Split('/'))
                .Where(Item => Item is not null)
                .SelectMany(Item => Item)
                .ToList();

            ClearPaths.AddRange(ConvertPaths);
            ClearPaths.Add(SetFileName);

            var FullFileName = Path.Combine(ClearPaths.ToArray()).Replace(@"\", "/");
            return FullFileName;
        }
        internal string CombineFullPath(params string[] Paths)
        {
            var AllPaths = new[]
            {
                Setting.RootPath,
            }.ToList();

            AllPaths.AddRange(Paths);
            var FullPath = Path.Combine(AllPaths.ToArray()).Replace(@"\", "/");
            return FullPath;
        }
        internal virtual string ConvertFileName(object FileName)
        {
            if (FileName is null)
                return "";

            var SetFileName = FileName.ToString().Replace("-", "").ToUpper();
            return SetFileName;
        }
        internal virtual string CombineExtension(object FileName, string Extension)
        {
            var CombineFileName = $"{FileName.ToString().TrimEnd('.')}.{Extension.ToLower().TrimStart('.')}";
            return CombineFileName;
        }
        #endregion

        #region Private Method
        private string LocalSave<TData>(string FileName, byte[] SaveBuffer, params object[] Paths)
        {
            var FindPaths = Paths.ToList();
            FindPaths.Add(typeof(TData).Name);
            var FullFileName = CombineFullName(FileName, out var SetFileName, FindPaths);
            BaseWriteFile(FullFileName, SaveBuffer);
            return SetFileName;
        }
        private string LocalSave(string FileName, byte[] SaveBuffer, params object[] Paths)
        {
            var FullFileName = CombineFullName(FileName, out var SetFileName, Paths);
            BaseWriteFile(FullFileName, SaveBuffer);
            return SetFileName;
        }
        private string LocalSave(string FileName, IFormFile File, params object[] Paths)
        {
            var FullFileName = CombineFullName(FileName, out var SetFileName, Paths);
            using var Ms = new MemoryStream();
            File.CopyTo(Ms);
            BaseWriteFile(FullFileName, Ms.ToArray());
            Ms?.Dispose();
            return SetFileName;
        }
        private static void BaseWriteFile(string FullFileName, byte[] WriteBuffer)
        {
            var Info = new FileInfo(FullFileName);
            if (!Info.Directory.Exists)
                Info.Directory.Create();

            File.WriteAllBytes(FullFileName, WriteBuffer);
        }
        #endregion
    }
}