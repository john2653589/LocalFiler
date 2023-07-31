using Rugal.LocalFiler.Model;
using Microsoft.AspNetCore.Http;

namespace Rugal.LocalFiler.Service
{
    public partial class LocalFilerService
    {
        public LocalFileManagerSetting Setting;
        public LocalFilerService(LocalFileManagerSetting _Setting)
        {
            Setting = _Setting;
        }

        #region Public Method

        #region Transfer
        public virtual LocalFilerService TransferSave<TData>(IEnumerable<TData> Datas, Func<TData, byte[]> ExtractBuffer, Func<TData, object> GetFileName, Action<TData, string> SetFileNameFunc)
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
        #endregion

        #region File Save
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