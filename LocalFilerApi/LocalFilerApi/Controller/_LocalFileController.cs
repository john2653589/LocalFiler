using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rugal.LocalFiler.Model;
using Rugal.LocalFiler.Service;
using System.Net.Http.Json;

namespace Rugal.Net.LocalFileManager.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public partial class _LocalFileController : ControllerBase
    {
        private readonly LocalFileService LocalFileService;
        public _LocalFileController(LocalFileService _LocalFileService)
        {
            LocalFileService = _LocalFileService;
        }

        [HttpGet]
        public dynamic GetFileList()
        {
            try
            {
                var FileList = LocalFileService.GetFileList();
                return FileList;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        public async Task<dynamic> SyncToRemote()
        {
            using var Client = new HttpClient();
            var Setting = LocalFileService.Setting;

            var RemoteServer = Setting.RemoteServer.Trim('/');
            var BaseUrl = $"{RemoteServer}/api/_LocalFile";
            var GetFileListUrl = $"{BaseUrl}/GetFileList";

            var RemoteRoot = await Client.GetFromJsonAsync<SyncDirectoryModel>(GetFileListUrl);
            var LocalRoot = LocalFileService.GetFileList();

            LocalFileService.CompareFileList(LocalRoot, RemoteRoot, async File =>
            {
                var UploadFileUrl = $"{BaseUrl}/UploadFile?Path={File.Path}&FileName={File.FileName}";
                var GetBuffer = LocalFileService.ReadFile(File.FileName, File.Path);

                using var Ms = new MemoryStream(GetBuffer);
                using var BufferContent = new StreamContent(Ms);
                using var FormContent = new MultipartFormDataContent()
                {
                    { BufferContent, "File",  File.FileName },
                };

                var ApiRet = await Client.PostAsync(UploadFileUrl, FormContent);
            });
            return true;
        }

        [HttpPost]
        public async Task<dynamic> SyncToLocal()
        {
            var Setting = LocalFileService.Setting;

            var RemoteServer = Setting.RemoteServer.Trim('/');
            var BaseUrl = $"{RemoteServer}/api/_LocalFile";
            var GetFileListUrl = $"{BaseUrl}/GetFileList";

            using var GetModelClient = new HttpClient();
            var RemoteRoot = await GetModelClient.GetFromJsonAsync<SyncDirectoryModel>(GetFileListUrl);
            var LocalRoot = LocalFileService.GetFileList();

            LocalFileService.CompareFileList(RemoteRoot, LocalRoot, File =>
            {
                using var GetFileClient = new HttpClient();
                var GetFileUrl = $"{BaseUrl}/GetFile?Path={File.Path}&FileName={File.FileName}";
                var ApiRet = GetFileClient.GetAsync(GetFileUrl).Result;
                var ApiBuffer = ApiRet.Content.ReadFromJsonAsync<GetFileModel>().Result;
                var Buffer = ApiBuffer.Buffer;
                LocalFileService.SaveFile(File.FileName, Buffer, File.Path);
            });

            return true;
        }

        [HttpPost]
        public async Task<dynamic> SyncTrade()
        {
            await SyncToRemote();
            await SyncToLocal();
            return true;
        }

        [HttpGet]
        public dynamic GetFile(string Path, string FileName)
        {
            var FileBuffer = LocalFileService.ReadFile(FileName, Path);
            var Ret = new GetFileModel()
            {
                Buffer = FileBuffer,
            };
            return Ret;
        }

        [HttpPost]
        public dynamic UploadFile(string Path, string FileName, IFormFile File)
        {
            if (File is null)
                return null;

            using var Ms = new MemoryStream();
            File.CopyTo(Ms);
            var SaveBuffer = Ms.ToArray();
            var GetFileName = LocalFileService.SaveFile(FileName, SaveBuffer, Path);
            return GetFileName;
        }

        [HttpGet]
        public dynamic GetDirectoryModel(string Path)
        {
            var AllFiles = LocalFileService.GetFileList();
            AllFiles.TryGetDirectory(Path, out var Model);
            return Model;
        }

        [HttpGet]
        public dynamic ForEachFileList()
        {
            var Files = LocalFileService
                .ForEachFiles()
                .ToArray();
            return Files;
        }

    }
}