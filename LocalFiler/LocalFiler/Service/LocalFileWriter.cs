using Rugal.LocalFiler.Model;
using System.Text.RegularExpressions;

namespace Rugal.LocalFiler.Service
{
    public class LocalFileWriter : IDisposable
    {
        public FileInfo Info { get; set; }
        public FileStream Stream { get; set; }
        public LocalFileInfoModel LocalInfo { get; set; }
        public LocalFileService LocalFileService { get; set; }
        public bool IsExist => Info.Exists;
        public bool IsTemp => Info.Extension.ToLower() == ".tmp";
        public long Length => Info is not null && Info.Exists ? Info.Length : 0;
        public LocalFileWriter(LocalFileService _LocalFileService)
        {
            LocalFileService = _LocalFileService;
        }
        public LocalFileWriter(LocalFileService _LocalFileService, LocalFileInfoModel Model) : this(_LocalFileService)
        {
            WithFile(Model);
        }

        public LocalFileWriter WithFileService(LocalFileService _LocalFileService)
        {
            LocalFileService = _LocalFileService;
            return this;
        }
        public LocalFileWriter WithFile(LocalFileInfoModel Model)
        {
            var FullFileName = LocalFileService.CombinePaths(Model);
            LocalInfo = Model;
            Info = new FileInfo(FullFileName);
            ClearStream();
            return this;
        }
        public LocalFileWriter WithTemp()
        {
            LocalInfo.FileName = $"{LocalInfo.FileName}.tmp";
            var TempPath = LocalFileService.CombinePaths(LocalInfo);
            Info = new FileInfo(TempPath);
            ClearStream();
            return this;
        }
        public LocalFileWriter WithRemoveTemp()
        {
            var ReFileName = Regex.Replace(LocalInfo.FileName, ".tmp$", "");
            ClearStream();
            ReName(ReFileName);
            return this;
        }
        public bool IsHasTemp(out long OutLength)
        {
            var TempFileName = $"{LocalInfo.FileName}.tmp";
            var TempPath = LocalFileService.CombinePaths(LocalInfo.Path, TempFileName);
            var TempInfo = new FileInfo(TempPath);
            OutLength = TempInfo.Length;
            return TempInfo.Exists;
        }
        public bool IsHasTemp()
        {
            var TempFileName = $"{LocalInfo.FileName}.tmp";
            var TempPath = LocalFileService.CombinePaths(LocalInfo.Path, TempFileName);
            return File.Exists(TempPath);
        }

        public LocalFileWriter OpenRead(long SeekLength = 0)
        {
            if (Stream != null)
                ClearStream();

            Stream = Info.OpenRead();
            if (SeekLength > 0)
                Seek(SeekLength);
            return this;
        }
        public LocalFileWriter OpenReadFromEnd()
        {
            OpenRead();
            SeekFromEnd();
            return this;
        }
        public LocalFileWriter OpenWrite(long SeekLength = 0)
        {
            if (Stream != null)
                ClearStream();

            if (!Info.Directory.Exists)
                Info.Directory.Create();

            Stream = Info.OpenWrite();
            if (SeekLength > 0)
                Seek(SeekLength);
            return this;
        }
        public LocalFileWriter OpenWriteFromEnd()
        {
            OpenWrite();
            SeekFromEnd();
            return this;
        }
        public LocalFileWriter Seek(long SeekLength)
        {
            Stream.Seek(SeekLength, SeekOrigin.Begin);
            return this;
        }
        public LocalFileWriter SeekFromEnd()
        {
            Stream.Seek(Stream.Length, SeekOrigin.Begin);
            return this;
        }

        public LocalFileWriter WriteBytes(byte[] Source, long MaxWriteLength = 1024)
        {
            if (Stream is null)
                OpenWriteFromEnd();

            using var SourceBuffer = new MemoryStream(Source);

            var Buffer = new byte[MaxWriteLength];
            while (SourceBuffer.Position < SourceBuffer.Length)
            {
                if (SourceBuffer.Position + MaxWriteLength > SourceBuffer.Length)
                    Buffer = new byte[SourceBuffer.Length - SourceBuffer.Position];

                SourceBuffer.Read(Buffer);
                Stream?.Write(Buffer);
            }

            return this;
        }
        public LocalFileWriter ReadBytes(Func<byte[], bool> ReadFunc, long MaxReadLength)
        {
            if (Stream is null)
                OpenRead();

            var Buffer = new byte[MaxReadLength];
            while (Stream.Position < Stream.Length)
            {
                if (Stream.Position + MaxReadLength > Stream.Length)
                    Buffer = new byte[Stream.Length - Stream.Position];

                Stream?.Read(Buffer);
                var IsCanNext = ReadFunc.Invoke(Buffer);
                if (!IsCanNext)
                    break;
            }
            return this;
        }
        public async Task<LocalFileWriter> ReadBytesAsync(Func<byte[], Task<bool>> ReadFunc, long MaxReadLength)
        {
            if (Stream is null)
                OpenRead();

            var Buffer = new byte[MaxReadLength];
            while (Stream.Position < Stream.Length)
            {
                if (Stream.Position + MaxReadLength > Stream.Length)
                    Buffer = new byte[Stream.Length - Stream.Position];

                Stream?.Read(Buffer);
                var IsCanNext = await ReadFunc.Invoke(Buffer);
                if (!IsCanNext)
                    break;
            }

            return this;
        }
        public LocalFileWriter ReName(string NewName)
        {
            var SourcePath = LocalFileService.CombinePaths(LocalInfo);
            LocalInfo.FileName = NewName;
            var TargetPath = LocalFileService.CombinePaths(LocalInfo);
            File.Move(SourcePath, TargetPath, true);
            return this;
        }

        public byte[] ReadAllByte()
        {
            var Buffer = File.ReadAllBytes(Info.FullName);
            return Buffer;
        }
        public void ClearStream()
        {
            Stream?.Flush();
            Stream?.Close();
            Stream?.Dispose();
            Stream = null;
        }
        public void Dispose()
        {
            ClearStream();
            GC.SuppressFinalize(this);
        }
    }
}