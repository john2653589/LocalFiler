//namespace Rugal.LocalFiler.Service
//{
//    public partial class LocalFilerService 
//    {
//        public virtual int DeleteFile_DataAlignForDb(DbContext Db, Func<IEnumerable<object>, string, bool> IsCompare)
//        {
//            var RootModel = GetFileList();
//            var DeleteCount = 0;
//            foreach (var Directory in RootModel.Directories)
//            {
//                var GetTable = Db.Table(Directory.Path);
//                var AllData = GetTable.AsEnumerable();
//                foreach (var File in Directory.Files)
//                {
//                    if (!IsCompare(AllData, File.FileName))
//                    {
//                        DeleteFile(Directory.Path, File.FileName);
//                        DeleteCount++;
//                    }
//                }
//            }

//            return DeleteCount;
//        }
//    }
//}