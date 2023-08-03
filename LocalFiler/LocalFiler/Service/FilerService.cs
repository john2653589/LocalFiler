using Rugal.LocalFiler.Model;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Rugal.LocalFiler.Service
{
    public partial class FilerService
    {
        public FilerSetting Setting;
        public FilerService(FilerSetting _Setting)
        {
            Setting = _Setting;
        }

        #region Public Method

        #region Transfer
        public virtual FilerService TransferSave<TData>(IEnumerable<TData> Datas, Func<TData, byte[]> ExtractBuffer, Func<TData, object> GetFileName, Action<TData, string> SetFileNameFunc)
        {
            foreach (var Item in Datas)
            {
                var GetBuffer = ExtractBuffer.Invoke(Item);
                var FileName = GetFileName.Invoke(Item).ToString();
                var SetFileName = SaveFile<TData>(FileName, GetBuffer);
                SetFileNameFunc.Invoke(Item, SetFileName);
            }
            return this;
        }
        #endregion

        #region File Save
        public virtual string SaveFile(SaveConfig Config, Action<SaveConfig> ConfigFunc = null)
        {
            ConfigFunc?.Invoke(Config);
            var Result = LocalSave(Config);
            return Result;
        }
        public virtual string SaveFile(Action<SaveConfig> ConfigFunc)
        {
            var Config = new SaveConfig();
            var Result = SaveFile(Config, ConfigFunc);
            return Result;
        }
        public virtual string SaveFile(object FileName, byte[] Buffer, Action<SaveConfig> ConfigFunc = null)
        {
            var Config = new SaveConfig(FileName, Buffer);
            var Result = SaveFile(Config, ConfigFunc);
            return Result;
        }
        public virtual string SaveFile<TData>(object FileName, byte[] Buffer, Action<SaveConfig> ConfigFunc = null)
        {
            var Config = new SaveConfig(FileName, Buffer)
                .AddPath(typeof(TData).Name);

            var Result = SaveFile(Config, ConfigFunc);
            return Result;
        }
        public virtual string SaveFile(object FileName, IFormFile File, Action<SaveConfig> ConfigFunc = null)
        {
            var Config = new SaveConfig(FileName, File);
            var Result = SaveFile(Config, ConfigFunc);
            return Result;
        }
        public virtual string SaveFile<TData>(object FileName, IFormFile File, Action<SaveConfig> ConfigFunc = null)
        {
            var Config = new SaveConfig(FileName, File)
                .AddPath(typeof(TData).Name);
            var Result = SaveFile(Config, ConfigFunc);
            return Result;
        }
        #endregion

        #region File Read
        private byte[] BaseReadFile(PathConfig Config)
        {
            if (Config.FileName is null)
                return Array.Empty<byte>();

            var FullFileName = CombineRootFileName(Config.FileName, Config.Paths);
            if (!File.Exists(FullFileName))
                return Array.Empty<byte>();

            var FileBuffer = File.ReadAllBytes(FullFileName);
            return FileBuffer;
        }
        public virtual byte[] ReadFile<TData>(object FileName, IEnumerable<string> Paths = null)
        {
            var Config = new PathConfig(FileName, Paths)
                .AddPath(typeof(TData).Name);

            var FileBuffer = BaseReadFile(Config);
            return FileBuffer;
        }
        public virtual byte[] ReadFile(object FileName, IEnumerable<string> Paths = null)
        {
            var Config = new PathConfig(FileName, Paths);

            var FileBuffer = BaseReadFile(Config);
            return FileBuffer;
        }

        private Task<byte[]> BaseReadFileAsync(PathConfig Config)
        {
            if (Config.FileName is null)
                return Task.FromResult(Array.Empty<byte>());

            var FullFileName = CombineRootFileName(Config.FileName, Config.Paths);
            if (!File.Exists(FullFileName))
                return Task.FromResult(Array.Empty<byte>());

            var FileBuffer = File.ReadAllBytesAsync(FullFileName);
            return FileBuffer;
        }
        public virtual Task<byte[]> ReadFileAsync<TData>(object FileName, IEnumerable<string> Paths = null)
        {
            var Config = new PathConfig(FileName, Paths)
                .AddPath(typeof(TData).Name);

            var FileBuffer = BaseReadFileAsync(Config);
            return FileBuffer;
        }
        public virtual Task<byte[]> ReadFileAsync(object FileName, IEnumerable<string> Paths = null)
        {
            var Config = new PathConfig(FileName, Paths);
            var FileBuffer = BaseReadFileAsync(Config);
            return FileBuffer;
        }
        #endregion

        #region File Delete
        public virtual bool DeleteFile(IEnumerable<string> FileNames, IEnumerable<string> Paths = null)
        {
            var IsDelete = true;
            foreach (var Item in FileNames)
            {
                var Config = new PathConfig(Item, Paths);
                IsDelete = IsDelete && DeleteFile(Config);
            }
            return IsDelete;
        }
        public virtual bool DeleteFile<TData>(IEnumerable<string> FileNames, IEnumerable<string> Paths = null)
        {
            var IsDelete = true;
            foreach (var Item in FileNames)
            {
                var Config = new PathConfig(Item, Paths)
                    .AddPath(typeof(TData).Name);

                IsDelete = IsDelete && DeleteFile(Config);
            }
            return IsDelete;
        }
        public virtual bool DeleteFile(object FileName, IEnumerable<string> Paths = null)
        {
            var Config = new PathConfig(FileName, Paths);

            var IsDelete = DeleteFile(Config);
            return IsDelete;
        }
        public virtual bool DeleteFile<TData>(object FileName, IEnumerable<string> Paths = null)
        {
            var Config = new PathConfig(FileName, Paths)
                .AddPath(typeof(TData).Name);

            var IsDelete = DeleteFile(Config);
            return IsDelete;
        }
        public virtual bool DeleteFile<TData, TColumn>(IEnumerable<TData> FileDatas, Func<TData, TColumn> GetColumnFunc, IEnumerable<string> Paths = null)
        {
            var IsDelete = true;
            foreach (var Item in FileDatas)
            {
                var GetFileName = GetColumnFunc(Item);
                var Config = new PathConfig(GetFileName, Paths)
                    .AddPath(typeof(TData).Name);
                IsDelete = IsDelete && DeleteFile(Config);
            }
            return IsDelete;
        }
        public bool DeleteFile(PathConfig Config)
        {
            if (Config.FileName is null)
                return false;

            var FullFileName = CombineRootFileName(Config.FileName, Config.Paths);
            if (!File.Exists(FullFileName))
                return false;

            File.Delete(FullFileName);
            var IsDelete = !File.Exists(FullFileName);
            return IsDelete;
        }
        #endregion

        #endregion

        #region Convert file name and root file name
        public virtual string CombineRootFileName(string FileName, out string SetFileName, IEnumerable<string> Paths = null)
        {
            SetFileName = ConvertFileName(FileName);

            VerifyFileName(SetFileName);

            Paths ??= new List<string> { };
            var PathList = Paths.ToList();
            PathList.Add(SetFileName);

            var FullFileName = CombineRootPaths(PathList);
            return FullFileName;
        }
        public virtual string CombineRootFileName(string FileName, IEnumerable<string> Paths = null)
        {
            var FullFileName = CombineRootFileName(FileName, out _, Paths);
            return FullFileName;
        }
        public virtual string CombineRootFileName(FilerInfo Model)
        {
            var FullFileName = CombineRootFileName(Model.FileName, new[] { Model.Path });
            return FullFileName;
        }
        public virtual string CombineExtension(string FileName, string Extension)
        {
            var ClearFileName = FileName.TrimEnd('.');
            if (string.IsNullOrWhiteSpace(Extension))
                return FileName;

            var CombineFileName = $"{ClearFileName}.{Extension.ToLower().TrimStart('.')}";
            return CombineFileName;
        }
        #endregion

        #region Private Method
        private static string ConvertFileName(string FileName)
        {
            if (FileName is null)
                return null;

            var SetFileName = FileName.Replace("-", "").ToUpper();
            return SetFileName;
        }
        private string CombineRootPaths(IEnumerable<string> Paths)
        {
            var AllPaths = new[]
            {
                Setting.RootPath,
            }.ToList();

            var ConvertPaths = Paths?
                .Select(Item => Item?.ToString().TrimStart('/').TrimEnd('/').Split('/'))
                .Where(Item => Item is not null)
                .SelectMany(Item => Item)
                .ToList();

            foreach (var Item in ConvertPaths)
                if (Path.IsPathRooted(Item))
                    throw new Exception("not allowed path");

            AllPaths.AddRange(ConvertPaths);
            var FullPath = Path.Combine(AllPaths.ToArray()).Replace(@"\", "/");

            return FullPath;
        }
        private string ProcessFileNameExtension(SaveConfig Config, out string SetFileName)
        {
            var FileName = Config.FileName;

            if (Setting.DefaultExtensionFromFile && Config.SaveBy == SaveByType.FormFile && !Config.HasExtension)
                Config.UseFileExtension();

            if (Setting.UseExtension && Config.HasExtension)
                FileName = CombineExtension(FileName, Config.Extension);

            var FullFileName = CombineRootFileName(FileName, out SetFileName, Config.Paths);
            return FullFileName;
        }
        private string LocalSave(SaveConfig Config)
        {
            var FullFileName = ProcessFileNameExtension(Config, out var SetFileName);
            if (Config.SaveBy == SaveByType.FormFile)
            {
                using var Ms = new MemoryStream();
                Config.FormFile.CopyTo(Ms);
                Config.Buffer = Ms.ToArray();
            }

            BaseWriteFile(FullFileName, Config.Buffer);
            return SetFileName;
        }
        private static void BaseWriteFile(string FullFileName, byte[] WriteBuffer)
        {
            var Info = new FileInfo(FullFileName);
            if (!Info.Directory.Exists)
                Info.Directory.Create();

            File.WriteAllBytes(FullFileName, WriteBuffer);
        }
        private static void VerifyFileName(string FileName)
        {
            var WhiteList = new Regex(@"^[a-zA-Z0-9_.-]+$");
            if (!WhiteList.IsMatch(FileName))
                throw new Exception("file name verification failed");

            var BlackList = new[] { ".." };
            foreach (var Item in BlackList)
            {
                if (FileName.Contains(Item))
                    throw new Exception("file name verification failed");
            }
        }
        #endregion
    }
}