using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace WSM.WinServLib
{
    /// <summary>
    /// windows服务辅助类
    /// </summary>
    public static class WinServHelper
    {
        /// <summary>
        /// 安装服务
        /// </summary>
        public static bool InstallService(string serviceFileName)
        {
            try
            {
                var installer = new AssemblyInstaller();
                installer.UseNewContext = true;
                installer.Path = serviceFileName;
                installer.Install(null);
                installer.Commit(null);
                installer.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 卸载服务
        /// </summary>
        /// <param name="serviceFileName"></param>
        /// <returns></returns>
        public static bool UnInstallService(string serviceFileName)
        {
            try
            {
                var installer = new AssemblyInstaller();
                installer.UseNewContext = true;
                installer.Path = serviceFileName;
                installer.Uninstall(null);
                installer.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool StartService(string serviceName)
        {
            if (IsServiceIsExisted(serviceName))
            {
                ServiceController service = new ServiceController(serviceName);
                if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.StartPending)
                {
                    service.Start();
                    for (int i = 0; i < 60; i++)
                    {
                        service.Refresh();
                        Thread.Sleep(1000);
                        if (service.Status == ServiceControllerStatus.Running)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool StopService(string serviceName)
        {
            if (IsServiceIsExisted(serviceName))
            {
                var service = new ServiceController(serviceName);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    for (int i = 0; i < 60; i++)
                    {
                        service.Refresh();
                        Thread.Sleep(1000);
                        if (service.Status == ServiceControllerStatus.Stopped)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 检查服务是否存在
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool IsServiceIsExisted(string serviceName)
        {
            var services = ServiceController.GetServices();
            foreach (var s in services)
            {
                if (string.Equals(s.ServiceName, serviceName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断服务是否启动
        /// </summary>
        /// <returns></returns>
        public static bool IsServiceStart(string serviceName)
        {
            var psc = new ServiceController(serviceName);
            if (!IsServiceIsExisted(serviceName))
            {
                throw new Exception(string.Format("{0}未找到！", serviceName));
            }

            return !psc.Status.Equals(ServiceControllerStatus.Stopped);
        }
        /// <summary>  
        /// 修改服务的启动项 2为自动,3为手动  
        /// </summary>  
        /// <param name="startType"></param>  
        /// <param name="serviceName"></param>  
        /// <returns></returns>  
        public static bool ChangeServiceStartType(int startType, string serviceName)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Services\" + serviceName);
                if (key != null)
                {
                    key.SetValue("Start", startType);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 根据过滤条件返回所有服务信息
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IEnumerable<ServiceInfo> GetServiceInfos(string filter)
        {
            var services = ServiceController.GetServices()
                .Where(s => s.ServiceType == ServiceType.Win32OwnProcess)
                .OrderBy(s => s.ServiceName).ToList();

            if (!string.IsNullOrEmpty(filter))
            {
                services = services.Where(s => s.ServiceName.ToLower().Contains(filter.ToLower())).ToList();
            }

            return services.Select(s => new ServiceInfo
            {
                ServiceName = s.ServiceName,
                DisplayName = s.DisplayName,
                Status = s.Status
            });
        }
        /// <summary>
        /// 根据过滤条件返回所有服务信息
        /// </summary>
        /// <param name="serviceNames"></param>
        /// <returns></returns>
        public static IList<ServiceInfo> GetServiceInfos(IList<string> serviceNames)
        {
            var services = ServiceController.GetServices()
                .Where(s => s.ServiceType == ServiceType.Win32OwnProcess)
                .OrderBy(s => s.ServiceName).ToList();

            if (serviceNames != null)
            {
                services = services.Join(serviceNames, c => c.ServiceName, s => s, (c, s) => c).ToList();
            }

            return services.Select(s => new ServiceInfo
            {
                ServiceName = s.ServiceName,
                DisplayName = s.DisplayName,
                Status = s.Status
            }).ToList();
        }
        /// <summary>
        /// 获得服务信息的bytes流
        /// </summary>
        /// <param name="serviceInfos"></param>
        /// <returns></returns>
        public static byte[] GetBytesFromServiceInfos(IList<ServiceInfo> serviceInfos)
        {
            var buffer = BitConverter.GetBytes(serviceInfos.Count());
            foreach (var serviceInfo in serviceInfos)
            {
                var bytesServiceName = Encoding.Default.GetBytes(serviceInfo.ServiceName); //服务名字节数组
                var bytesServiceNameLen = BitConverter.GetBytes(bytesServiceName.Count());

                var bytesDisplayName = Encoding.Default.GetBytes(serviceInfo.DisplayName); //显示名称字节数组
                var bytesDisplayNameLen = BitConverter.GetBytes(bytesDisplayName.Count());

                var bytesStatus = BitConverter.GetBytes((int)serviceInfo.Status);

                buffer=buffer.Concat(bytesServiceNameLen)
                    .Concat(bytesServiceName)
                    .Concat(bytesDisplayNameLen)
                    .Concat(bytesDisplayName)
                    .Concat(bytesStatus).ToArray();
            }
            return buffer;
        }
        public static IList<ServiceInfo> GetServiceInfosFromBytes(byte[] buffer)
        {
            var list = new List<ServiceInfo>();
            var index = 0;
            var count = BitConverter.ToInt32(buffer, 0);
            index += 4;
            for (int i = 0; i < count; i++)
            {
                var serviceInfo = new ServiceInfo();

                var bytesServiceNameLen = BitConverter.ToInt32(buffer, index);
                index += 4;
                serviceInfo.ServiceName = Encoding.Default.GetString(buffer, index, bytesServiceNameLen);
                index += bytesServiceNameLen;
                var bytesDisplayNameLen = BitConverter.ToInt32(buffer, index);
                index += 4;
                serviceInfo.DisplayName = Encoding.Default.GetString(buffer, index, bytesDisplayNameLen);

                index += bytesDisplayNameLen;
                serviceInfo.Status = (ServiceControllerStatus)(BitConverter.ToInt32(buffer, index));

                list.Add(serviceInfo);
            }
            return list;
        }
        /// <summary>
        /// 根据服务名称获取服务文件路径
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static string GetServiceFileName(string serviceName)
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Services\" + serviceName);
            if (key != null)
            {
                var path = key.GetValue("ImagePath");
                if (path != null) return path.ToString().Trim('"');
            }
            return string.Empty;
        }
    }
}
