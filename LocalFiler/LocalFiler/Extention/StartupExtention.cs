using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rugal.LocalFiler.Model;
using Rugal.LocalFiler.Service;

namespace Rugal.LocalFiler.Extention
{
    public static class StartupExtention
    {
        public static IServiceCollection AddLocalFile(this IServiceCollection Services, IConfiguration Configuration, string ConfigurationKey = "LocalFile")
        {
            var Setting = NewSetting(Configuration, ConfigurationKey);
            AddLocalFileSetting(Services, Setting);
            AddLocalFileService(Services);

            return Services;
        }
        public static IServiceCollection AddLocalFile(this IServiceCollection Services, IConfiguration Configuration,
           Action<LocalFileManagerSetting, IServiceProvider> SettingFunc, string ConfigurationKey = "LocalFile")
        {
            Services.AddLocalFile(Configuration, ConfigurationKey, SettingFunc);
            return Services;
        }
        public static IServiceCollection AddLocalFile(this IServiceCollection Services, IConfiguration Configuration,
            string ConfigurationKey, Action<LocalFileManagerSetting, IServiceProvider> SettingFunc)
        {
            var Setting = NewSetting(Configuration, ConfigurationKey);
            AddLocalFileSetting(Services, Setting, SettingFunc);
            AddLocalFileService(Services);
            return Services;
        }

        public static IServiceCollection AddLocalFile(this IServiceCollection Services, string RootPath, string RemoteServer = null)
        {
            AddLocalFileSetting(Services, RootPath, RemoteServer);
            AddLocalFileService(Services);
            return Services;
        }
        public static IServiceCollection AddLocalFile(this IServiceCollection Services,
            string RootPath, string RemoteServer, Action<LocalFileManagerSetting, IServiceProvider> SettingFunc)
        {
            AddLocalFileSetting(Services, RootPath, RemoteServer, SettingFunc);
            AddLocalFileService(Services);
            return Services;
        }

        public static IServiceCollection AddLocalFileSetting(this IServiceCollection Services, string RootPath, string RemoteServer = null)
        {
            var Setting = new LocalFileManagerSetting()
            {
                RootPath = RootPath,
                RemoteServer = RemoteServer,
            };
            Services.AddSingleton(Setting);
            return Services;
        }
        public static IServiceCollection AddLocalFileSetting(this IServiceCollection Services, LocalFileManagerSetting Setting)
        {
            Services.AddSingleton(Setting);
            return Services;
        }
        public static IServiceCollection AddLocalFileSetting(this IServiceCollection Services,
            string RootPath, string RemoteServer, Action<LocalFileManagerSetting, IServiceProvider> SettingFunc)
        {
            Services.AddSingleton((Provider) =>
            {
                var Setting = new LocalFileManagerSetting()
                {
                    RootPath = RootPath,
                    RemoteServer = RemoteServer,
                };
                SettingFunc?.Invoke(Setting, Provider);
                return Setting;
            });
            return Services;
        }
        public static IServiceCollection AddLocalFileSetting(this IServiceCollection Services, LocalFileManagerSetting Setting, Action<LocalFileManagerSetting, IServiceProvider> SettingFunc)
        {
            Services.AddSingleton((Provider) =>
            {
                SettingFunc.Invoke(Setting, Provider);
                return Setting;
            });
            return Services;
        }
        public static IServiceCollection AddLocalFileService(this IServiceCollection Services)
        {
            Services.AddSingleton<LocalFileService>();
            return Services;
        }

        private static LocalFileManagerSetting NewSetting(IConfiguration Configuration, string ConfigurationKey)
        {
            var GetSetting = Configuration.GetSection(ConfigurationKey);
            var Spm = GetSetting.GetValue<string>("Spm");
            var SyncWayString = GetSetting.GetValue<string>("SyncWay");
            if (!Enum.TryParse<SyncWayType>(SyncWayString, true, out var SyncWay))
                SyncWay = SyncWayType.None;
            var Setting = new LocalFileManagerSetting()
            {
                RootPath = GetSetting.GetValue<string>("RootPath"),
                RemoteServer = GetSetting.GetValue<string>("RemoteServer"),
                SyncPerMin = Spm == null ? null : TimeSpan.FromMinutes(int.Parse(Spm)),
                SyncWay = SyncWay,
            };
            return Setting;
        }
    }
}
