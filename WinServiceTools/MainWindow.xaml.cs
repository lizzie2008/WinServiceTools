using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;


namespace WinServiceTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string cmdExe = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe ";

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadServiceList(TbFilter.Text.Trim());
        }
        /// <summary>
        /// 加载服务列表
        /// </summary>
        private void LoadServiceList(string filter)
        {
            var services = ServiceController.GetServices()
                .Where(s => s.ServiceType == ServiceType.Win32OwnProcess)
                .OrderBy(s => s.ServiceName).ToList();

            if (!string.IsNullOrEmpty(filter))
            {
                services = services.Where(s => s.ServiceName.ToLower().Contains(filter.ToLower())).ToList();
            }

            DgService.ItemsSource = services;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegServiceClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "可执行文件 (*.exe)|*.exe"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                InstallService(openFileDialog.FileName);
                LoadServiceList(TbFilter.Text.Trim());
            }
        }

        /// <summary>
        /// 运行cmd命令
        /// 会显示命令窗口
        /// </summary>
        /// <param name="cmdExe">指定应用程序的完整路径</param>
        /// <param name="cmdStr">执行命令行参数</param>
        static bool RunCmd(string cmdExe, string cmdStr)
        {
            bool result = false;
            try
            {
                using (Process myPro = new Process())
                {
                    //指定启动进程是调用的应用程序和命令行参数
                    ProcessStartInfo psi = new ProcessStartInfo(cmdExe, cmdStr);
                    myPro.StartInfo = psi;
                    myPro.Start();
                    myPro.WaitForExit();
                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartMenuClick(object sender, RoutedEventArgs e)
        {
            var ele = e.OriginalSource as MenuItem;
            if (ele != null)
            {
                var sc = ele.DataContext as ServiceController;
                if (sc != null) StartService(sc.ServiceName);
            }

            LoadServiceList(TbFilter.Text.Trim());
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopMenuClick(object sender, RoutedEventArgs e)
        {
            var ele = e.OriginalSource as MenuItem;
            if (ele != null)
            {
                var sc = ele.DataContext as ServiceController;
                if (sc != null) StopService(sc.ServiceName);
            }

            LoadServiceList(TbFilter.Text.Trim());
        }
        /// <summary>
        /// 删除服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteMenuClick(object sender, RoutedEventArgs e)
        {
            var ele = e.OriginalSource as MenuItem;
            if (ele != null)
            {
                var sc = ele.DataContext as ServiceController;
                if (sc != null) UninstallService(sc.ServiceName);
            }

            LoadServiceList(TbFilter.Text.Trim());
        }

        /// <summary>
        /// 过滤文本事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbFilterChanged(object sender, TextChangedEventArgs e)
        {
            LoadServiceList(TbFilter.Text.Trim());
        }

        #region Windows服务控制区

        #region 安装服务
        /// <summary>
        /// 安装服务
        /// </summary>
        private bool InstallService(string serviceFileName)
        {
            bool flag = true;
            try
            {

                InstallmyService(null, serviceFileName);
            }
            catch
            {
                flag = false;
            }

            return flag;
        }
        #endregion

        #region 卸载服务
        /// <summary>
        /// 卸载服务
        /// </summary>
        private bool UninstallService(string NameService)
        {
            bool flag = true;
            if (IsServiceIsExisted(NameService))
            {
                try
                {
                    UnInstallmyService(NameService);
                }
                catch
                {
                    flag = false;
                }
            }
            return flag;
        }
        #endregion

        #region 检查服务存在的存在性
        /// <summary>
        /// 检查服务存在的存在性
        /// </summary>
        /// <param name=" NameService ">服务名</param>
        /// <returns>存在返回 true,否则返回 false;</returns>
        public static bool IsServiceIsExisted(string NameService)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName.ToLower() == NameService.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 安装Windows服务
        /// <summary>
        /// 安装Windows服务
        /// </summary>
        /// <param name="stateSaver">集合</param>
        /// <param name="filepath">程序文件路径</param>
        public static void InstallmyService(IDictionary stateSaver, string filepath)
        {
            AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
            AssemblyInstaller1.UseNewContext = true;
            AssemblyInstaller1.Path = filepath;
            AssemblyInstaller1.Install(stateSaver);
            AssemblyInstaller1.Commit(stateSaver);
            AssemblyInstaller1.Dispose();
        }
        #endregion

        #region 卸载Windows服务
        /// <summary>
        /// 卸载Windows服务
        /// </summary>
        /// <param name="filepath">程序文件路径</param>
        public static void UnInstallmyService(string filepath)
        {
            //AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
            //AssemblyInstaller1.UseNewContext = true;
            //AssemblyInstaller1.Path = filepath;
            //AssemblyInstaller1.Uninstall(null);
            //AssemblyInstaller1.Dispose();

            RunCmd("sc", " delete " + filepath);
        }
        #endregion

        #region 判断window服务是否启动
        /// <summary>
        /// 判断某个Windows服务是否启动
        /// </summary>
        /// <returns></returns>
        public static bool IsServiceStart(string serviceName)
        {
            ServiceController psc = new ServiceController(serviceName);
            bool bStartStatus = false;
            try
            {
                if (!psc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    bStartStatus = true;
                }

                return bStartStatus;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region  修改服务的启动项
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
                RegistryKey regist = Registry.LocalMachine;
                RegistryKey sysReg = regist.OpenSubKey("SYSTEM");
                RegistryKey currentControlSet = sysReg.OpenSubKey("CurrentControlSet");
                RegistryKey services = currentControlSet.OpenSubKey("Services");
                RegistryKey servicesName = services.OpenSubKey(serviceName, true);
                servicesName.SetValue("Start", startType);
            }
            catch (Exception ex)
            {

                return false;
            }
            return true;


        }
        #endregion

        #region 启动服务
        private bool StartService(string serviceName)
        {
            bool flag = true;
            if (IsServiceIsExisted(serviceName))
            {
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running && service.Status != System.ServiceProcess.ServiceControllerStatus.StartPending)
                {
                    service.Start();
                    for (int i = 0; i < 60; i++)
                    {
                        service.Refresh();
                        System.Threading.Thread.Sleep(1000);
                        if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                        {
                            break;
                        }
                        if (i == 59)
                        {
                            flag = false;
                        }
                    }
                }
            }
            return flag;
        }
        #endregion

        #region 停止服务
        private bool StopService(string serviceName)
        {
            bool flag = true;
            if (IsServiceIsExisted(serviceName))
            {
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    service.Stop();
                    for (int i = 0; i < 60; i++)
                    {
                        service.Refresh();
                        System.Threading.Thread.Sleep(1000);
                        if (service.Status == System.ServiceProcess.ServiceControllerStatus.Stopped)
                        {
                            break;
                        }
                        if (i == 59)
                        {
                            flag = false;
                        }
                    }
                }
            }
            return flag;
        }
        #endregion

        #endregion


    }
}
