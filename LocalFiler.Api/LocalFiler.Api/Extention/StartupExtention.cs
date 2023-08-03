using Rugal.LocalFiler.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rugal.LocalFiler.Api.LocalFiler.Api.Extention
{
    public static class StartupExtention
    {
        public static void Test()
        {
            var Spm = GetSetting.GetValue<string>("Spm");
            var SyncWayString = GetSetting.GetValue<string>("SyncWay");
            if (!Enum.TryParse<SyncWayType>(SyncWayString, true, out var SyncWay))
                SyncWay = SyncWayType.None;
        }
    }
}
