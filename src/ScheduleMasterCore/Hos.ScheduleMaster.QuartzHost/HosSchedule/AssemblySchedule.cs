using System;
using System.Collections.Generic;
using System.Threading;
using Hos.ScheduleMaster.Base;
using Hos.ScheduleMaster.Core;
using Hos.ScheduleMaster.Core.Models;
using Hos.ScheduleMaster.QuartzHost.Common;

namespace Hos.ScheduleMaster.QuartzHost.HosSchedule
{
    public class AssemblySchedule : IHosSchedule
    {
        public ScheduleEntity Main { get; set; }
        public Dictionary<string, object> CustomParams { get; set; }
        public List<KeyValuePair<string, string>> Keepers { get; set; }
        public Dictionary<Guid, string> Children { get; set; }
        public TaskBase RunnableInstance { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        private PluginLoadContext _loadContext;


        public void CreateRunnableInstance(ScheduleContext context)
        {
            _loadContext = AssemblyHelper.LoadAssemblyContext(context.Schedule.FileId, context.Schedule.AssemblyName);
            RunnableInstance = AssemblyHelper.CreateTaskInstance(
                _loadContext,
                context.Schedule.FileId,
                context.Schedule.AssemblyName,
                context.Schedule.ClassName
            );
            if (RunnableInstance == null)
            {
                throw new InvalidCastException($"任务实例创建失败，请确认目标任务是否派生自TaskBase类型。程序集：{context.Schedule.AssemblyName}，类型：{context.Schedule.ClassName}");
            }
        }

        public Type GetQuartzJobType()
        {
            return typeof(RunnableJob.AssemblyJob);
        }

        public void Dispose()
        {
            AssemblyHelper.UnLoadAssemblyLoadContext(_loadContext);
            RunnableInstance = null;
        }
    }
}
