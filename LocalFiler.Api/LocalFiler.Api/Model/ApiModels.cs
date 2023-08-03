
namespace Rugal.LocalFiler.Api.Model
{
    public class GetFileModel
    {
        public byte[] Buffer { get; set; }
    }

    public class ApiSetting
    {
        public string RemoteServer { get; set; }
        public TimeSpan? SyncPerMin { get; set; }
        public long SyncToPerByte { get; set; } = 1024 * 1024;
        public SyncWayType SyncWay { get; set; } = SyncWayType.None;
    }

    public enum SyncWayType
    {
        None,
        ToServer,
        FromServer,
        Trade,
    }



}
