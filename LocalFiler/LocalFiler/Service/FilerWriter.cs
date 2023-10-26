using Rugal.LocalFiler.Model;
using System;

namespace Rugal.LocalFiler.Service
{
    public class FilerWriter
    {
        public readonly FilerInfo Info;
        private FilerService Filer => Info.Filer;
        public FilerWriter(FilerInfo _Info)
        {
            Info = _Info;
        }
        public FilerWriter OpenRead(Func<byte[], bool> ReadFunc, long ReadFromLength = 0, long KbPerRead = 0)
        {
            if (KbPerRead == 0)
                KbPerRead = Filer.Setting.ReadPerKb;

            _ = OpenReadAsync(Buffer =>
            {
                var IsNext = ReadFunc(Buffer);
                return Task.FromResult(IsNext);
            }, ReadFromLength, KbPerRead).Result;
            return this;
        }
        public async Task<FilerWriter> OpenReadAsync(Func<byte[], Task<bool>> ReadFunc, long ReadFromLength = 0, long KbPerRead = 0)
        {
            if (KbPerRead == 0)
                KbPerRead = Filer.Setting.ReadPerKb;

            if (!Info.BaseInfo.Exists)
                return this;

            using var FileBuffer = Info.BaseInfo.OpenRead();
            FileBuffer.Seek(ReadFromLength, SeekOrigin.Begin);

            var ReadByteLength = KbPerRead * 1024;
            while (FileBuffer.Position < FileBuffer.Length)
            {
                if (FileBuffer.Position + ReadByteLength > FileBuffer.Length)
                    ReadByteLength = FileBuffer.Length - FileBuffer.Position;

                var ReadBuffer = new byte[ReadByteLength];
                var ReadCount = FileBuffer.Read(ReadBuffer);

                if (ReadCount == 0)
                    break;

                var IsNext = await ReadFunc.Invoke(ReadBuffer);
                if (!IsNext)
                    break;
            }
            return this;
        }
        public FilerWriter OpenWrite(Func<FileStream, long> WriterFunc, long WriteFromLength = 0)
        {
            var BaseInfo = Info.BaseInfo;
            var Directory = BaseInfo.Directory;
            if (!Directory.Exists)
                Directory.Create();

            using var FileBuffer = Info.BaseInfo.OpenWrite();
            FileBuffer.Seek(WriteFromLength, SeekOrigin.Begin);
            var WriteLength = WriterFunc(FileBuffer);
            return this;
        }
        public async Task<FilerWriter> OpenWriteAsync(Func<FileStream, Task<long>> WriterFunc, long WriteFromLength = 0)
        {
            var BaseInfo = Info.BaseInfo;
            var Directory = BaseInfo.Directory;
            if (!Directory.Exists)
                Directory.Create();

            var IsExist = BaseInfo.Exists;
            using var FileBuffer = Info.BaseInfo.OpenWrite();

            if (IsExist && WriteFromLength > 0)
                FileBuffer.Seek(WriteFromLength, SeekOrigin.Begin);
            var WriteLength = await WriterFunc(FileBuffer);
            return this;
        }
    }
}