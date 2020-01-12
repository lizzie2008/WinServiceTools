using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using WSM.TcpNetLib;
using WSM.WinServLib;

namespace WSM.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Tcp客户端
        /// </summary>
        private AsyncTcpClient tcpClient;

        public MainWindow()
        {
            InitializeComponent();

            lblStatus.Content = "未连接";
        }

        private void StartMenuClick(object sender, RoutedEventArgs e)
        {

        }

        private void StopMenuClick(object sender, RoutedEventArgs e)
        {

        }

        private void UnInstallMenuClick(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 连接服务器端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectClicked(object sender, RoutedEventArgs e)
        {
            var ip = tbIP.Text.Trim();
            var port = tbPort.Text.Trim();

            if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(port))
            {
                if (tcpClient == null)
                {
                    try
                    {
                        tcpClient = new AsyncTcpClient(IPAddress.Parse(ip), int.Parse(port));
                        tcpClient.Connect();
                        tcpClient.DatagramReceived += client_DatagramReceived;
                        lblStatus.Content = "已连接";
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("创建客户端连接失败：{0}", ex.Message));
                    }
                }
            }
        }
        /// <summary>
        /// 接收数据处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                DgService.ItemsSource = WinServHelper.GetServiceInfosFromBytes(e.Datagram);
            });
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisconnectClicked(object sender, RoutedEventArgs e)
        {
            if (tcpClient != null)
            {
                tcpClient.DatagramReceived -= client_DatagramReceived;
                tcpClient.Close();
            }
        }
        /// <summary>
        /// 过滤条件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbFilterChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
