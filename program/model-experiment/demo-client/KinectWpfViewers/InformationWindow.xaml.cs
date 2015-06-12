using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using System.Runtime.Remoting.Messaging;

namespace Microsoft.Samples.Kinect.WpfViewers
{
    /// <summary>
    /// information.xaml 的交互逻辑
    /// </summary>
    public partial class InformationWindow : Window
    {
        private SkeletonSender skeletonSender;
        private static InformationWindow instance = null;
        public delegate void skeletonSender_ResultReceiveEventHandler(object sender, ResultReceiveEventArgs e);
        private String DebugText{
            set { debugOutput.Text = value; }
            get { return debugOutput.Text;}
        }
        public InformationWindow()
        {
            InitializeComponent();
            string address = System.Configuration.ConfigurationManager.AppSettings["address"];
            string pushDataPort = System.Configuration.ConfigurationManager.AppSettings["pushDataPort"];
            string pullResultPort = System.Configuration.ConfigurationManager.AppSettings["pullResultPort"];
            skeletonSender = new SkeletonSender(address, pushDataPort, pullResultPort);
            skeletonSender.ResultReceiveEvent += (object sender, ResultReceiveEventArgs e) =>
            {
                debugOutput.Dispatcher.Invoke(new Action(() => DebugText =  e.Message ));
            };
            DebugText = address;
        }

        void skeletonSender_ResultReceiveEvent(object sender, ResultReceiveEventArgs e)
        {
            DebugText = e.Message;
        }
        public void setSkeleton(Skeleton skeleton)
        {
            if(skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                skeletonSender.SendSkeletonFrame(skeleton) ;
            }
        }
        public static InformationWindow getInstance()
        {
            if(instance == null)
            {
                instance = new InformationWindow();
            }
            return instance;
        }
    }
}
