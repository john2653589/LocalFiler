﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rugal.LocalFiler.Model;
using Rugal.LocalFiler.Service;

namespace Rugal.LocalFiler.Extention
{
    public static class StartupExtention
    {
        public static IServiceCollection AddLocalFiler(this IServiceCollection Services, IConfiguration Configuration)
        {
            var Setting = NewSetting(Configuration);
            AddLocalFiler_Setting(Services, Setting);
            AddLocalFiler_Service(Services);
            return Services;
        }
        public static IServiceCollection AddLocalFiler(this IServiceCollection Services, IConfiguration Configuration,
           Action<LocalFilerSetting, IServiceProvider> SettingFunc)
        {
            var Setting = NewSetting(Configuration);
            AddLocalFiler_Setting(Services, Setting, SettingFunc);
            AddLocalFiler_Service(Services);
            return Services;
        }
        public static IServiceCollection AddLocalFiler_Setting(this IServiceCollection Services, LocalFilerSetting Setting)
        {
            Services.AddSingleton(Setting);
            return Services;
        }
        public static IServiceCollection AddLocalFiler_Setting(this IServiceCollection Services, LocalFilerSetting Setting, Action<LocalFilerSetting, IServiceProvider> SettingFunc)
        {
            Services.AddSingleton(Provider =>
            {
                SettingFunc.Invoke(Setting, Provider);
                return Setting;
            });
            return Services;
        }
        public static IServiceCollection AddLocalFiler_Service(this IServiceCollection Services)
        {
            Services.AddSingleton<LocalFilerService>();
            return Services;
        }
        private static LocalFilerSetting NewSetting(IConfiguration Configuration)
        {
            var GetSetting = Configuration.GetSection("LocalFiler");
            _ = bool.TryParse(GetSetting["DefaultExtensionFromFile"], out var DefaultExtensionFromFile);
            _ = bool.TryParse(GetSetting["UseExtension"], out var UseExtension);

            var Setting = new LocalFilerSetting()
            {
                RootPath = GetSetting["RootPath"],
                DefaultExtensionFromFile = DefaultExtensionFromFile,
                UseExtension = UseExtension,
            };
            return Setting;
        }
    }
}