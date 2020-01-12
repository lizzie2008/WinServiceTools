using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WSM.TcpNetLib;
using WSM.WinServLib;

namespace WSM.Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public string FileName { get; set; }
        /// <summary>
        /// 监控线程
        /// </summary>
        private Thread monitorThread;
        /// <summary>
        /// Tcp服务器端
        /// </summary>
        private AsyncTcpServer tcpServer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        /// <summary>
        /// 加载处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lblStatus.Content = "监控已停止";
            LoadServiceList(TbFilter.Text.Trim());
        }
        /// <summary>
        /// 加载服务列表
        /// </summary>
        private void LoadServiceList(string filter)
        {
            DgService.ItemsSource = WinServHelper.GetServiceInfos(filter);
        }
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "可执行文件 (*.exe)|*.exe"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                if (WinServHelper.IsServiceIsExisted(Path.GetFileNameWithoutExtension(openFileDialog.FileName)))
                {
                    MessageBox.Show("请不要重复安装！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    WinServHelper.InstallService(openFileDialog.FileName);
                    LoadServiceList(TbFilter.Text.Trim());
                }
            }
        }
        /// <summary>
        /// 开始监控
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartMonitorClicked(object sender, RoutedEventArgs e)
        {
            //初始化Tcp服务器
            if (tcpServer == null)
            {
                tcpServer = new AsyncTcpServer(IPAddress.Any, 6666);
                tcpServer.ClientConnected += tcpServer_ClientConnected;
                tcpServer.ClientDisconnected += tcpServer_ClientDisconnected;
            }
            //开始监控
            tcpServer.Start();

            //开始监控线程，定时向客户端发送服务状态
            if (monitorThread == null)
            {
                monitorThread = new Thread(SendServiceInfos);
                monitorThread.IsBackground = true;
                monitorThread.Start();
            }

            lblStatus.Content = "监控中...";
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopMonitorClicked(object sender, RoutedEventArgs e)
        {
            //终止线程
            if (monitorThread != null)
            {
                monitorThread.Abort();
                monitorThread.Join();
                monitorThread = null;
            }

            //停止服务器
            if (tcpServer != null)
                tcpServer.Stop();

            lblStatus.Content = "监控已停止";
            lbClient.Items.Clear();
        }
        /// <summary>
        /// 清空监控列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearMonitorClicked(object sender, RoutedEventArgs e)
        {
            lbMonitorService.Items.Clear();
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
                var sc = ele.DataContext as ServiceInfo;
                if (sc != null)
                {
                    WinServHelper.StartService(sc.ServiceName);
                    LoadServiceList(TbFilter.Text.Trim());
                }
            }

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
                var sc = ele.DataContext as ServiceInfo;
                if (sc != null)
                {
                    WinServHelper.StopService(sc.ServiceName);
                    LoadServiceList(TbFilter.Text.Trim());
                }
            }
        }
        /// <summary>
        /// 删除服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnInstallMenuClick(object sender, RoutedEventArgs e)
        {
            var ele = e.OriginalSource as MenuItem;
            if (ele != null)
            {
                var sc = ele.DataContext as ServiceInfo;
                if (sc != null)
                {
                    WinServHelper.UnInstallService(WinServHelper.GetServiceFileName(sc.ServiceName));
                    LoadServiceList(TbFilter.Text.Trim());
                }
            }
        }
        /// <summary>
        /// 增加监控
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddMonitorMenuClick(object sender, RoutedEventArgs e)
        {
            var ele = e.OriginalSource as MenuItem;
            if (ele != null)
            {
                var sc = ele.DataContext as ServiceInfo;
                if (sc != null)
                {
                    if (!lbMonitorService.Items.Contains(sc.ServiceName))
                        lbMonitorService.Items.Add(sc.ServiceName);
                }
            }
        }
        /// <summary>
        /// 取消监控
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelMonitorMenuClick(object sender, RoutedEventArgs e)
        {
            lbMonitorService.Items.Remove(lbMonitorService.SelectedItem);
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

        /// <summary>
        /// 发送服务信息线程入口
        /// </summary>
        private void SendServiceInfos()
        {
            while (true)
            {
                var serviceNames = (from object item in lbMonitorService.Items select item.ToString()).ToList();
                var serviceInfos = WinServHelper.GetServiceInfos(serviceNames);
                var sendData = WinServHelper.GetBytesFromServiceInfos(serviceInfos);
                tcpServer.SendToAll(sendData);

                Thread.Sleep(1000);
            }
            // ReSharper disable once FunctionNeverReturns
        }
        /// <summary>
        /// 建立客户端连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcpServer_ClientConnected(object sender, TcpClientConnectedEventArgs e)
        {
            var remoteEndPoint = e.TcpClient.Client.RemoteEndPoint.ToString();
            Dispatcher.BeginInvoke((Action)delegate {
                lbClient.Items.Add(remoteEndPoint);
            });

        }
        /// <summary>
        /// 断开客户端连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tcpServer_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            var remoteEndPoint = e.TcpClient.Client.RemoteEndPoint.ToString();
            Dispatcher.BeginInvoke((Action)delegate {
                lbClient.Items.Remove(remoteEndPoint);
            });
        }
    }
}
