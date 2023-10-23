﻿using Rugal.LocalFiler.Model;

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

            using var FileBuffer = Info.BaseInfo.OpenWrite();
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



    //public class FilerWriter2 : IDisposable
    //{
    //    #region Property
    //    public FilerService Filer { get; set; }
    //    #endregion

    //    #region Property
    //    public FileInfo Info { get; set; }
    //    public FileStream Stream { get; set; }
    //    //public FilerInfo LocalInfo { get; set; }
    //    public bool IsExist => Info.Exists;
    //    public bool IsTemp => Info.Extension.ToLower() == ".tmp";
    //    public long Length => Info is not null && Info.Exists ? Info.Length : 0;
    //    #endregion

    //    #region With File
    //    //public FilerWriter WithFile(FilerInfo Info)
    //    //{
    //    //    Filer.InfoFile
    //    //}
    //    #endregion

    //    #region With Property
    //    //public FilerWriter WithFile(FilerInfo Model)
    //    //{
    //    //    var FullFileName = LocalFileService.CombineRootFileName(Model);
    //    //    LocalInfo = Model;
    //    //    Info = new FileInfo(FullFileName);
    //    //    ClearStream();
    //    //    return this;
    //    //}
    //    public FilerWriter WithTemp()
    //    {
    //        LocalInfo.FileName = $"{LocalInfo.FileName}.tmp";
    //        var TempPath = LocalFileService.CombineRootFileName(LocalInfo);
    //        Info = new FileInfo(TempPath);
    //        ClearStream();
    //        return this;
    //    }
    //    public FilerWriter WithRemoveTemp()
    //    {
    //        var ReFileName = Regex.Replace(LocalInfo.FileName, ".tmp$", "");
    //        ClearStream();
    //        ReName(ReFileName);
    //        return this;
    //    }
    //    #endregion

    //    #region Temp check
    //    public bool IsHasTemp(out long OutLength)
    //    {
    //        var TempFileName = $"{LocalInfo.FileName}.tmp";
    //        var TempPath = Filer.CombineRootFileName(TempFileName, new[] { LocalInfo.Path });
    //        var TempInfo = new FileInfo(TempPath);
    //        OutLength = TempInfo.Length;
    //        return TempInfo.Exists;
    //    }
    //    public bool IsHasTemp()
    //    {
    //        var HasTemp = IsHasTemp(out _);
    //        return HasTemp;
    //    }
    //    #endregion

    //    #region Open And Seek
    //    public FilerWriter OpenRead(long SeekLength = 0)
    //    {
    //        if (Stream is not null)
    //            ClearStream();

    //        if (!IsExist)
    //            return this;

    //        Stream = Info.OpenRead();
    //        if (SeekLength > 0)
    //            Seek(SeekLength);
    //        return this;
    //    }
    //    public FilerWriter OpenWrite(long SeekLength = 0)
    //    {
    //        if (Stream is not null)
    //            ClearStream();

    //        if (!Info.Directory.Exists)
    //            Info.Directory.Create();

    //        Stream = Info.OpenWrite();
    //        if (SeekLength > 0)
    //            Seek(SeekLength);
    //        return this;
    //    }
    //    public FilerWriter OpenReadFromEnd()
    //    {
    //        OpenRead();
    //        SeekFromEnd();
    //        return this;
    //    }
    //    public FilerWriter OpenWriteFromEnd()
    //    {
    //        OpenWrite();
    //        SeekFromEnd();
    //        return this;
    //    }
    //    public FilerWriter Seek(long SeekLength)
    //    {
    //        Stream.Seek(SeekLength, SeekOrigin.Begin);
    //        return this;
    //    }
    //    public FilerWriter SeekFromEnd()
    //    {
    //        Stream.Seek(Stream.Length, SeekOrigin.Begin);
    //        return this;
    //    }
    //    #endregion

    //    #region Read And Write
    //    private void BufferLoop(Stream StreamBuffer, long MaxLength, Func<byte[], bool> LoopFunc)
    //    {
    //        var Buffer = new byte[MaxLength];
    //        while (StreamBuffer.Position < StreamBuffer.Length)
    //        {
    //            var IsOverLength = StreamBuffer.Position + MaxLength > StreamBuffer.Length;
    //            if (IsOverLength)
    //                Buffer = new byte[StreamBuffer.Length - StreamBuffer.Position];

    //            var IsNext = LoopFunc.Invoke(Buffer);
    //            if (!IsNext)
    //                break;
    //        }
    //    }
    //    private async Task<bool> BufferLoopAsync(Stream StreamBuffer, long MaxLength, Func<byte[], Task<bool>> LoopFunc)
    //    {
    //        var Buffer = new byte[MaxLength];
    //        while (StreamBuffer.Position < StreamBuffer.Length)
    //        {
    //            var IsOverLength = StreamBuffer.Position + MaxLength > StreamBuffer.Length;
    //            if (IsOverLength)
    //                Buffer = new byte[StreamBuffer.Length - StreamBuffer.Position];

    //            var IsNext = await LoopFunc.Invoke(Buffer);
    //            if (!IsNext)
    //                break;
    //        }
    //        return true;
    //    }
    //    public FilerWriter WriteBytes(byte[] Source, long MaxWriteLength = 1024)
    //    {
    //        if (Stream is null)
    //            OpenWriteFromEnd();

    //        using var SourceBuffer = new MemoryStream(Source);
    //        BufferLoop(SourceBuffer, MaxWriteLength, Buffer =>
    //        {
    //            SourceBuffer.Read(Buffer);
    //            Stream?.Write(Buffer);
    //            return true;
    //        });
    //        return this;
    //    }
    //    public FilerWriter ReadBytes(Func<byte[], bool> ReadFunc, long MaxReadLength = 1024)
    //    {
    //        if (Stream is null)
    //            OpenRead();

    //        BufferLoop(Stream, MaxReadLength, Buffer =>
    //        {
    //            Stream?.Read(Buffer);
    //            var IsNext = ReadFunc.Invoke(Buffer);
    //            return IsNext;
    //        });
    //        return this;
    //    }
    //    public async Task<FilerWriter> WriteBytesAsync(byte[] Source, long MaxWriteLength = 1024)
    //    {
    //        if (Stream is null)
    //            OpenWriteFromEnd();

    //        using var SourceBuffer = new MemoryStream(Source);
    //        var IsComplete = await BufferLoopAsync(SourceBuffer, MaxWriteLength, async Buffer =>
    //        {
    //            _ = await SourceBuffer.ReadAsync(Buffer);
    //            await Stream?.WriteAsync(Buffer, 0, Buffer.Length);
    //            return true;
    //        });
    //        return this;
    //    }
    //    public async Task<FilerWriter> ReadBytesAsync(Func<byte[], Task<bool>> ReadFunc, long MaxReadLength)
    //    {
    //        if (Stream is null)
    //            OpenRead();

    //        var IsComplete = await BufferLoopAsync(Stream, MaxReadLength, async Buffer =>
    //        {
    //            _ = await Stream.ReadAsync(Buffer, 0, Buffer.Length);
    //            var IsNext = await ReadFunc.Invoke(Buffer);
    //            return IsNext;
    //        });
    //        return this;
    //    }
    //    #endregion

    //    #region File Control
    //    public FilerWriter ReName(string NewName)
    //    {
    //        var SourcePath = LocalFileService.CombineRootFileName(LocalInfo);
    //        LocalInfo.FileName = NewName;
    //        var TargetPath = LocalFileService.CombineRootFileName(LocalInfo);
    //        File.Move(SourcePath, TargetPath, true);
    //        return this;
    //    }
    //    #endregion

    //    #region Class Life
    //    public void ClearStream()
    //    {
    //        Stream?.Flush();
    //        Stream?.Close();
    //        Stream?.Dispose();
    //        Stream = null;
    //    }
    //    public void Dispose()
    //    {
    //        ClearStream();
    //        GC.SuppressFinalize(this);
    //    }
    //    #endregion
    //}
}