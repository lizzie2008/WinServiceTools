using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using WSM.WinServLib;

namespace WSM.Controller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private XmlDocument doc = new XmlDocument();

        public MainWindow()
        {
            InitializeComponent();

            //this.Loaded += (Sender, e) =>
            //{
            //    doc.Load("ServiceNames.xml");
            //    var selectSingleNode = doc.SelectSingleNode(@"ServiceNames");
            //    if (selectSingleNode != null)
            //    {
            //        for (int i = 0; i < selectSingleNode.ChildNodes.Count; i++)
            //        {
            //            lbServiceNames.Items.Add(selectSingleNode.ChildNodes[i].InnerText);
            //        }
            //    }
            //};
        }
        /// <summary>
        /// 识别目录服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindServiceNamesClicked(object sender, RoutedEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(@"..\");//初始化制定路径
            DirectoryInfo[] dirs = di.GetDirectories();//取得路径数组

            var serviceName = new List<string>();
            foreach (var dir in dirs)
            {
                if (dir.Name.StartsWith("Erp", true, CultureInfo.CurrentCulture))
                {
                    serviceName.Add(dir.Name);
                }
            }
            lvService.ItemsSource = WinServHelper.GetServiceInfos(serviceName);

        }
        /// <summary>
        /// 停止所有服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServiceStopClicked(object sender, RoutedEventArgs e)
        {
            foreach (ServiceInfo item in lvService.Items)
            {
                WinServHelper.StopService(item.ServiceName);
            }
            RefreshService();
        }
        /// <summary>
        /// 暂停所有服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServiceStartClicked(object sender, RoutedEventArgs e)
        {
            foreach (ServiceInfo item in lvService.Items)
            {
                WinServHelper.StartService(item.ServiceName);
            }
            RefreshService();
        }
        /// <summary>
        /// 刷新服务状态
        /// </summary>
        private void RefreshService()
        {
            var serviceName = new List<string>();

            foreach (ServiceInfo item in lvService.Items)
            {
                serviceName.Add(item.ServiceName);
            }
            lvService.ItemsSource = WinServHelper.GetServiceInfos(serviceName);
        }
    }
}
