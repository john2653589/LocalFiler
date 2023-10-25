using Rugal.LocalFiler.Model;

namespace Rugal.LocalFiler.Service
{
    public class FilerWriter
    {
        public readonly FilerInfo Info;
        public FilerWriter(FilerInfo _Info)
        {
            Info = _Info;
        }
        public FilerWriter OpenRead(Func<byte[], bool> ReadFunc, long ReadFromLength = 0, long KbPerRead = 1024)
        {
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

                var IsNext = ReadFunc.Invoke(ReadBuffer);
                if (!IsNext)
                    break;
            }
            return this;
        }
        public FilerWriter OpenWrite(Action<FileStream> WriterFunc, long WriteFromLength = 0)
        {
            using var FileBuffer = Info.BaseInfo.OpenWrite();
            FileBuffer.Seek(WriteFromLength, SeekOrigin.Begin);
            WriterFunc.Invoke(FileBuffer);
            return this;
        }
    }
}