using Hos.ScheduleMaster.Core;
using Hos.ScheduleMaster.Core.Common;
using Hos.ScheduleMaster.Core.Log;
using Hos.ScheduleMaster.Core.Models;
using Hos.ScheduleMaster.QuartzHost.HosSchedule;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Hos.ScheduleMaster.QuartzHost.Common
{
    public class HosScheduleFactory
    {
        public static async Task<IHosSchedule> GetHosSchedule(ScheduleContext context)
        {
            IHosSchedule result;
            switch ((ScheduleMetaType) context.Schedule.MetaType)
            {
                case ScheduleMetaType.Assembly:
                {
                    result = new AssemblySchedule();
                    await LoadPluginFile(context.Schedule);
                    break;
                }
                case ScheduleMetaType.Http:
                {
                    result = new HttpSchedule();
                    break;
                }
                default: throw new InvalidOperationException("unknown schedule type.");
            }

            result.Main = context.Schedule;
            result.CustomParams = ConvertParamsJson(context.Schedule.CustomParamsJson);
            result.Keepers = context.Keepers;
            result.Children = context.Children;
            result.CancellationTokenSource = new System.Threading.CancellationTokenSource();
            result.CreateRunnableInstance(context);
            result.RunnableInstance.TaskId = context.Schedule.Id;
            result.RunnableInstance.CancellationToken = result.CancellationTokenSource.Token;
            result.RunnableInstance.Initialize();
            return result;
        }

        private static async Task LoadPluginFile(ScheduleEntity model)
        {
            bool pull = true;
            var pluginPath = $"{ConfigurationCache.PluginPathPrefix}\\{model.FileId}".ToPhysicalPath();
            //看一下拉取策略
            string policy = ConfigurationCache.GetField<string>("Assembly_ImagePullPolicy");
            if (policy == "IfNotPresent" && System.IO.Directory.Exists(pluginPath))
            {
                pull = false;
            }

            if (pull)
            {
                using var scope = new ScopeDbContext();
                var file = await scope.GetDbContext().ScheduleFiles.FindAsync(model.FileId);
                var zipPath =
                    $"{ConfigurationCache.PluginPathPrefix}\\{model.FileId:n}.zip".ToPhysicalPath();

                var array = file.Content;
                await using var fs = new System.IO.FileStream(zipPath, System.IO.FileMode.Create);
                fs.Write(array, 0, array.Length);
                fs.Close();
                //将指定 zip 存档中的所有文件都解压缩到各自对应的目录下
                ZipFile.ExtractToDirectory(zipPath, pluginPath, true);
                if(File.Exists(Path.Combine(pluginPath, "Hos.ScheduleMaster.Base.dll")))
                    File.Delete(Path.Combine(pluginPath, "Hos.ScheduleMaster.Base.dll"));
                File.Delete(zipPath);
            }
        }

        public static Dictionary<string, object> ConvertParamsJson(string source)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                List<ScheduleParam> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ScheduleParam>>(source);
                foreach (var item in list)
                {
                    result[item.ParamKey] = item.ParamValue;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

            return result;
        }
    }
}