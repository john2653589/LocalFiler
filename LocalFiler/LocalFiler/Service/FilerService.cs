using Microsoft.AspNetCore.Http;
using Rugal.LocalFiler.Model;
using System.Text.RegularExpressions;

namespace Rugal.LocalFiler.Service
{
    public partial class FilerService
    {
        public readonly FilerSetting Setting;
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
            var Config = new SaveConfig(FileName, Buffer);
            ConfigFunc?.Invoke(Config);
            Config.AddPath(typeof(TData).Name);

            var Result = SaveFile(Config);
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
            var Config = new SaveConfig(FileName, File);
            ConfigFunc?.Invoke(Config);
            Config.AddPath(typeof(TData).Name);
            var Result = SaveFile(Config);
            return Result;
        }
        #endregion

        #region File Read
        private byte[] LocalRead(ReadConfig Config)
        {
            if (Config.FileName is null)
                return Array.Empty<byte>();

            var FullFileName = CombineRootFileName(Config.FileName, Config.Paths);
            if (!File.Exists(FullFileName))
                return Array.Empty<byte>();

            var FileBuffer = File.ReadAllBytes(FullFileName);
            return FileBuffer;
        }
        public virtual byte[] ReadFile<TData>(object FileName, Action<ReadConfig> ConfigFunc = null)
        {
            var Config = new ReadConfig(FileName);
            ConfigFunc?.Invoke(Config);
            Config.AddPath(typeof(TData).Name);
            var FileBuffer = LocalRead(Config);
            return FileBuffer;
        }
        public virtual byte[] ReadFile(object FileName, Action<ReadConfig> ConfigFunc = null)
        {
            var Config = new ReadConfig(FileName);
            ConfigFunc?.Invoke(Config);
            var FileBuffer = LocalRead(Config);
            return FileBuffer;
        }

        private Task<byte[]> LocalReadAsync(ReadConfig Config)
        {
            if (Config.FileName is null)
                return Task.FromResult(Array.Empty<byte>());

            var FullFileName = CombineRootFileName(Config.FileName, Config.Paths);
            if (!File.Exists(FullFileName))
                return Task.FromResult(Array.Empty<byte>());

            var FileBuffer = File.ReadAllBytesAsync(FullFileName);
            return FileBuffer;
        }
        public virtual Task<byte[]> ReadFileAsync<TData>(object FileName, Action<ReadConfig> ConfigFunc = null)
        {
            var Config = new ReadConfig(FileName);
            ConfigFunc?.Invoke(Config);
            Config.AddPath(typeof(TData).Name);

            var FileBuffer = LocalReadAsync(Config);
            return FileBuffer;
        }
        public virtual Task<byte[]> ReadFileAsync(object FileName, Action<ReadConfig> ConfigFunc = null)
        {
            var Config = new ReadConfig(FileName);
            ConfigFunc?.Invoke(Config);
            var FileBuffer = LocalReadAsync(Config);
            return FileBuffer;
        }
        #endregion

        #region File Delete
        public virtual bool DeleteFile(IEnumerable<string> FileNames, Action<ReadConfig> ConfigFunc = null)
        {
            var IsDelete = true;
            foreach (var Item in FileNames)
            {
                var Config = new ReadConfig(Item);
                ConfigFunc?.Invoke(Config);
                IsDelete = IsDelete && DeleteFile(Config);
            }
            return IsDelete;
        }
        public virtual bool DeleteFile<TData>(IEnumerable<string> FileNames, Action<ReadConfig> ConfigFunc = null)
        {
            var IsDelete = true;
            foreach (var Item in FileNames)
            {
                var Config = new ReadConfig(Item);
                ConfigFunc?.Invoke(Config);
                Config.AddPath(typeof(TData).Name);

                IsDelete = IsDelete && DeleteFile(Config);
            }
            return IsDelete;
        }
        public virtual bool DeleteFile(object FileName, Action<ReadConfig> ConfigFunc = null)
        {
            var Config = new ReadConfig(FileName);
            ConfigFunc?.Invoke(Config);
            var IsDelete = DeleteFile(Config);
            return IsDelete;
        }
        public virtual bool DeleteFile<TData>(object FileName, Action<ReadConfig> ConfigFunc = null)
        {
            var Config = new ReadConfig(FileName);
            ConfigFunc?.Invoke(Config);
            Config.AddPath(typeof(TData).Name);

            var IsDelete = DeleteFile(Config);
            return IsDelete;
        }
        public virtual bool DeleteFile<TData, TColumn>(IEnumerable<TData> FileDatas, Func<TData, TColumn> GetColumnFunc, Action<ReadConfig> ConfigFunc = null)
        {
            var IsDelete = true;
            foreach (var Item in FileDatas)
            {
                var GetFileName = GetColumnFunc(Item);
                var Config = new ReadConfig(GetFileName);
                ConfigFunc?.Invoke(Config);
                Config.AddPath(typeof(TData).Name);
                IsDelete = IsDelete && DeleteFile(Config);
            }
            return IsDelete;
        }
        public bool DeleteFile(ReadConfig Config)
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

        #region File Info
        public virtual FilerInfo InfoFile(ReadConfig Config)
        {
            if (Config.FileName is null)
                throw new Exception("file name can not be null");

            var FullFileName = CombineRootFileName(Config);
            var Result = new FilerInfo(this, Config);

            return Result;
        }
        public virtual FilerInfo InfoFile(Action<ReadConfig> ConfigFunc)
        {
            var Config = new ReadConfig();
            ConfigFunc.Invoke(Config);
            var Result = InfoFile(Config);
            return Result;
        }
        public virtual FolderInfo InfoFolder()
        {
            var Config = new PathConfig().AddPath("/");
            var Result = new FolderInfo(this, Config);
            return Result;
        }
        public virtual FolderInfo InfoFolder(PathConfig Config)
        {
            var Result = new FolderInfo(this, Config);
            return Result;
        }
        public virtual FolderInfo InfoFolder(Action<PathConfig> ConfigFunc)
        {
            var Config = new PathConfig();
            ConfigFunc.Invoke(Config);
            var Result = InfoFolder(Config);
            return Result;
        }
        #endregion

        #region File Control
        public virtual FilerInfo ReNameInfo(FilerInfo Info, string NewFileName)
        {
            var NewConfig = Info.Config
                .WithFileName(NewFileName);

            var NewInfo = new FilerInfo(this, NewConfig);
            return NewInfo;
        }
        public virtual FilerInfo ReNameFile(FilerInfo Info, string NewFileName)
        {
            var NewFullFileName = CombineRootFileName(NewFileName, Info.Config.Paths);
            Info.BaseInfo.MoveTo(NewFullFileName, true);

            var NewConfig = Info.Config.WithFileName(NewFileName);
            var NewInfo = new FilerInfo(this, NewConfig);
            return NewInfo;
        }
        public FilerInfo WithTempInfo(FilerInfo Info, string TempExtension = "tmp")
        {
            TempExtension = ConvertExtension(TempExtension);
            var NewFileName = $"{Info.FileName}{TempExtension}";
            var TempInfo = ReNameInfo(Info, NewFileName);
            return TempInfo;
        }
        public FilerInfo RemoveTempInfo(FilerInfo Info, string TempExtension = "tmp")
        {
            TempExtension = ConvertExtension(TempExtension);
            var NewFileName = Regex.Replace(Info.FileName, $"{TempExtension}$", "");
            var NewInfo = ReNameInfo(Info, NewFileName);
            return NewInfo;
        }
        public FilerInfo RemoveTempFile(FilerInfo Info, string TempExtension = "tmp")
        {
            TempExtension = ConvertExtension(TempExtension);
            var NewFileName = Regex.Replace(Info.FileName, $"{TempExtension}$", "");
            var NewInfo = ReNameFile(Info, NewFileName);
            return NewInfo;
        }

        #endregion

        #endregion

        #region Convert File Name And Root File Name
        public virtual string CombineRootFileName(string FileName, out string SetFileName, IEnumerable<string> Paths = null, bool IsVerifyFileName = true)
        {
            SetFileName = ConvertFileName(FileName);

            if (IsVerifyFileName)
                VerifyFileName(SetFileName);

            Paths ??= new List<string> { };
            var PathList = Paths.ToList();
            PathList.Add(SetFileName);

            var FullFileName = CombineRootPaths(PathList);
            return FullFileName;
        }
        public virtual string CombineRootFileName(string FileName, IEnumerable<string> Paths = null, bool IsVerifyFileName = false)
        {
            var FullFileName = CombineRootFileName(FileName, out _, Paths, IsVerifyFileName);
            return FullFileName;
        }
        public virtual string CombineRootFileName(ReadConfig Config, bool IsVerifyFileName = false)
        {
            var FullFileName = CombineRootFileName(Config.FileName, Config.Paths, IsVerifyFileName);
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

        #region Root Path Process
        public virtual string CombineRootPaths(IEnumerable<string> Paths)
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

            if (ConvertPaths is not null)
            {
                foreach (var Item in ConvertPaths)
                    VerifyPath(Item);

                AllPaths.AddRange(ConvertPaths);
            }

            var FullPath = Path.Combine(AllPaths.ToArray()).Replace(@"\", "/");
            return FullPath;
        }
        public virtual string CombineRootPaths(PathConfig Config)
        {
            var Result = CombineRootPaths(Config.Paths);
            return Result;
        }
        #endregion

        #region Private Method
        private string ConvertFileName(string FileName)
        {
            if (FileName is null)
                return null;

            var SetFileName = FileName.Replace("-", "");
            SetFileName = Setting.FileNameCase switch
            {
                FileNameCaseType.None => SetFileName,
                FileNameCaseType.Upper => SetFileName.ToUpper(),
                FileNameCaseType.Lower => SetFileName.ToLower(),
                _ => SetFileName,
            };

            return SetFileName;
        }
        private static string ConvertExtension(string Extension)
        {
            Extension = $".{Extension.Replace(".", "")}";
            return Extension;
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
        private static void BaseWriteFile(string FullFileName, byte[] WriteBuffer)
        {
            var Info = new FileInfo(FullFileName);
            if (!Info.Directory.Exists)
                Info.Directory.Create();

            File.WriteAllBytes(FullFileName, WriteBuffer);
        }
        public static bool IsVerifyFileName(string FileName, out string ErrorMessage)
        {
            ErrorMessage = null;

            var WhiteList = new Regex(@"^[a-zA-Z0-9_.-]+$");
            if (!WhiteList.IsMatch(FileName))
            {
                ErrorMessage = "file name verification failed";
                return false;
            }

            var BlackList = new[] { ".." };
            foreach (var Item in BlackList)
            {
                if (FileName.Contains(Item))
                {
                    ErrorMessage = "file name verification failed";
                    return false;
                }
            }
            return true;
        }
        public static void VerifyFileName(string FileName)
        {
            if (!IsVerifyFileName(FileName, out var ErrorMessage))
                throw new Exception(ErrorMessage);
        }
        public static void VerifyPath(string FilePath)
        {
            if (Path.IsPathRooted(FilePath))
                throw new Exception("not allowed path");
        }
        #endregion
    }
}