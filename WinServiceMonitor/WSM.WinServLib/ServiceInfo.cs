using System;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace WSM.WinServLib
{
    /// <summary>
    /// 服务相关信息
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public ServiceControllerStatus Status { get; set; }
        /// <summary>
        /// 实体转换成byte流
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            var bytesServiceName = Encoding.Default.GetBytes(ServiceName); //服务名字节数组
            var bytesServiceNameLen = BitConverter.GetBytes(bytesServiceName.Count());

            var bytesDisplayName = Encoding.Default.GetBytes(DisplayName); //显示名称字节数组
            var bytesDisplayNameLen = BitConverter.GetBytes(bytesDisplayName.Count());

            var bytesStatus = BitConverter.GetBytes((int)Status);

            return bytesServiceNameLen
                   .Concat(bytesServiceName)
                   .Concat(bytesDisplayNameLen)
                   .Concat(bytesDisplayName)
                   .Concat(bytesStatus)
                   .ToArray();
        }
    }
}
