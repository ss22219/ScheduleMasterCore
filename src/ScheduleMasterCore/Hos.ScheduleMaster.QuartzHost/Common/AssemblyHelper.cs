using Hos.ScheduleMaster.Base;
using Hos.ScheduleMaster.Core;
using Hos.ScheduleMaster.Core.Common;
using Hos.ScheduleMaster.Core.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Web;

namespace Hos.ScheduleMaster.QuartzHost.Common
{
    public static class AssemblyHelper
    {
        public static Type GetClassType(string assemblyPath, string className)
        {
            var assembly = Assembly.Load(File.ReadAllBytes(assemblyPath));
            var type = assembly.GetType(className, true, true);
            return type;
        }

        public static T CreateInstance<T>(Type type) where T : class
        {
            return Activator.CreateInstance(type) as T;
        }

        public static TaskBase CreateTaskInstance(PluginLoadContext context, Guid fileId, string assemblyName,
            string className)
        {
            var pluginLocation = GetTaskAssemblyPath(fileId, assemblyName);
            var assembly =
                context.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
            var type = assembly.GetType(className, true, true);
            if (type == null) throw new Exception($"class {className} not found in {assemblyName}");
            return Activator.CreateInstance(type) as TaskBase;
        }

        private static string GetTaskAssemblyPath(Guid fileId, string assemblyName)
        {
            return $"{ConfigurationCache.PluginPathPrefix}\\{fileId}\\{assemblyName}.dll".ToPhysicalPath();
        }

        /// <summary>
        /// 加载应用程序域
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static PluginLoadContext LoadAssemblyContext(Guid fileId, string assemblyName)
        {
            var pluginLocation = GetTaskAssemblyPath(fileId, assemblyName);
            var loadContext = new PluginLoadContext(pluginLocation);
            return loadContext;
        }

        /// <summary>
        /// 卸载应用程序域
        /// </summary>
        /// <param name="context"></param>
        public static void UnLoadAssemblyLoadContext(PluginLoadContext context)
        {
            if (context == null) return;
            context.Unload();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}