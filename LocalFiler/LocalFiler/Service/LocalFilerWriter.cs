using Rugal.LocalFiler.Model;
using System.Text.RegularExpressions;

namespace Rugal.LocalFiler.Service
{
    public partial class LocalFilerWriter : IDisposable
    {
        #region Property
        public FileInfo Info { get; set; }
        public FileStream Stream { get; set; }
        public LocalFilerInfo LocalInfo { get; set; }
        public LocalFilerService LocalFileService { get; set; }
        public bool IsExist => Info.Exists;
        public bool IsTemp => Info.Extension.ToLower() == ".tmp";
        public long Length => Info is not null && Info.Exists ? Info.Length : 0;
        #endregion
        public LocalFilerWriter(LocalFilerService _LocalFileService)
        {
            LocalFileService = _LocalFileService;
        }
        public LocalFilerWriter(LocalFilerService _LocalFileService, LocalFilerInfo Model) : this(_LocalFileService)
        {
            WithFile(Model);
        }

        #region With property
        public LocalFilerWriter WithFilerService(LocalFilerService _LocalFileService)
        {
            LocalFileService = _LocalFileService;
            return this;
        }
        public LocalFilerWriter WithFile(LocalFilerInfo Model)
        {
            var FullFileName = LocalFileService.CombineRootFileName(Model);
            LocalInfo = Model;
            Info = new FileInfo(FullFileName);
            ClearStream();
            return this;
        }
        public LocalFilerWriter WithTemp()
        {
            LocalInfo.FileName = $"{LocalInfo.FileName}.tmp";
            var TempPath = LocalFileService.CombineRootFileName(LocalInfo);
            Info = new FileInfo(TempPath);
            ClearStream();
            return this;
        }
        public LocalFilerWriter WithRemoveTemp()
        {
            var ReFileName = Regex.Replace(LocalInfo.FileName, ".tmp$", "");
            ClearStream();
            ReName(ReFileName);
            return this;
        }
        #endregion

        #region Temp check
        public bool IsHasTemp(out long OutLength)
        {
            var TempFileName = $"{LocalInfo.FileName}.tmp";
            var TempPath = LocalFileService.CombineRootFileName(TempFileName, new[] { LocalInfo.Path });
            var TempInfo = new FileInfo(TempPath);
            OutLength = TempInfo.Length;
            return TempInfo.Exists;
        }
        public bool IsHasTemp()
        {
            var HasTemp = IsHasTemp(out _);
            return HasTemp;
        }
        #endregion

        #region Open and seek
        public LocalFilerWriter OpenRead(long SeekLength = 0)
        {
            if (Stream is not null)
                ClearStream();

            if (!IsExist)
                return this;

            Stream = Info.OpenRead();
            if (SeekLength > 0)
                Seek(SeekLength);
            return this;
        }
        public LocalFilerWriter OpenWrite(long SeekLength = 0)
        {
            if (Stream is not null)
                ClearStream();

            if (!Info.Directory.Exists)
                Info.Directory.Create();

            Stream = Info.OpenWrite();
            if (SeekLength > 0)
                Seek(SeekLength);
            return this;
        }
        public LocalFilerWriter OpenReadFromEnd()
        {
            OpenRead();
            SeekFromEnd();
            return this;
        }
        public LocalFilerWriter OpenWriteFromEnd()
        {
            OpenWrite();
            SeekFromEnd();
            return this;
        }
        public LocalFilerWriter Seek(long SeekLength)
        {
            Stream.Seek(SeekLength, SeekOrigin.Begin);
            return this;
        }
        public LocalFilerWriter SeekFromEnd()
        {
            Stream.Seek(Stream.Length, SeekOrigin.Begin);
            return this;
        }
        #endregion

        #region Read and write
        private void BufferLoop(Stream StreamBuffer, long MaxLength, Func<byte[], bool> LoopFunc)
        {
            var Buffer = new byte[MaxLength];
            while (StreamBuffer.Position < StreamBuffer.Length)
            {
                var IsOverLength = StreamBuffer.Position + MaxLength > StreamBuffer.Length;
                if (IsOverLength)
                    Buffer = new byte[StreamBuffer.Length - StreamBuffer.Position];

                var IsNext = LoopFunc.Invoke(Buffer);
                if (!IsNext)
                    break;
            }
        }
        private async Task<bool> BufferLoopAsync(Stream StreamBuffer, long MaxLength, Func<byte[], Task<bool>> LoopFunc)
        {
            var Buffer = new byte[MaxLength];
            while (StreamBuffer.Position < StreamBuffer.Length)
            {
                var IsOverLength = StreamBuffer.Position + MaxLength > StreamBuffer.Length;
                if (IsOverLength)
                    Buffer = new byte[StreamBuffer.Length - StreamBuffer.Position];

                var IsNext = await LoopFunc.Invoke(Buffer);
                if (!IsNext)
                    break;
            }
            return true;
        }
        public LocalFilerWriter WriteBytes(byte[] Source, long MaxWriteLength = 1024)
        {
            if (Stream is null)
                OpenWriteFromEnd();

            using var SourceBuffer = new MemoryStream(Source);
            BufferLoop(SourceBuffer, MaxWriteLength, Buffer =>
            {
                SourceBuffer.Read(Buffer);
                Stream?.Write(Buffer);
                return true;
            });
            return this;
        }
        public LocalFilerWriter ReadBytes(Func<byte[], bool> ReadFunc, long MaxReadLength = 1024)
        {
            if (Stream is null)
                OpenRead();

            BufferLoop(Stream, MaxReadLength, Buffer =>
            {
                Stream?.Read(Buffer);
                var IsNext = ReadFunc.Invoke(Buffer);
                return IsNext;
            });
            return this;
        }
        public async Task<LocalFilerWriter> WriteBytesAsync(byte[] Source, long MaxWriteLength = 1024)
        {
            if (Stream is null)
                OpenWriteFromEnd();

            using var SourceBuffer = new MemoryStream(Source);
            var IsComplete = await BufferLoopAsync(SourceBuffer, MaxWriteLength, async Buffer =>
            {
                _ = await SourceBuffer.ReadAsync(Buffer);
                await Stream?.WriteAsync(Buffer, 0, Buffer.Length);
                return true;
            });
            return this;
        }
        public async Task<LocalFilerWriter> ReadBytesAsync(Func<byte[], Task<bool>> ReadFunc, long MaxReadLength)
        {
            if (Stream is null)
                OpenRead();

            var IsComplete = await BufferLoopAsync(Stream, MaxReadLength, async Buffer =>
            {
                _ = await Stream.ReadAsync(Buffer, 0, Buffer.Length);
                var IsNext = await ReadFunc.Invoke(Buffer);
                return IsNext;
            });
            return this;
        }
        #endregion

        #region Control
        public LocalFilerWriter ReName(string NewName)
        {
            var SourcePath = LocalFileService.CombineRootFileName(LocalInfo);
            LocalInfo.FileName = NewName;
            var TargetPath = LocalFileService.CombineRootFileName(LocalInfo);
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
        #endregion
    }
}