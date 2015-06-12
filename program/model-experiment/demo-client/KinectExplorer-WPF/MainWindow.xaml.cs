//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.KinectExplorer
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KinectSensorItemCollection sensorItems;
        private readonly ObservableCollection<KinectStatusItem> statusItems;

        /// <summary>
        /// Initializes a new instance of the MainWindow class, which displays a list of sensors and their status.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.sensorItems = new KinectSensorItemCollection();
            this.statusItems = new ObservableCollection<KinectStatusItem>();
            this.kinectSensors.ItemsSource = this.sensorItems;
            this.kinectStatus.ItemsSource = this.statusItems;
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            // listen to any status change for Kinects.
            KinectSensor.KinectSensors.StatusChanged += this.KinectsStatusChanged;

            // show status for each sensor that is found now.
            foreach (KinectSensor kinect in KinectSensor.KinectSensors)
            {
                this.ShowStatus(kinect, kinect.Status);
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            foreach (KinectSensorItem sensorItem in this.sensorItems)
            {
                sensorItem.CloseWindow();
            }

            this.sensorItems.Clear();
        }

        private void ShowStatus(KinectSensor kinectSensor, KinectStatus kinectStatus)
        {
            this.statusItems.Add(new KinectStatusItem 
            { 
                Id = (null == kinectSensor) ? null : kinectSensor.DeviceConnectionId,
                Status = kinectStatus,
                DateTime = DateTime.Now
            });

            KinectSensorItem sensorItem;
            this.sensorItems.SensorLookup.TryGetValue(kinectSensor, out sensorItem);

            if (KinectStatus.Disconnected == kinectStatus)
            {
                if (sensorItem != null)
                {
                    this.sensorItems.Remove(sensorItem);
                    sensorItem.CloseWindow();
                }
            }
            else
            {
                if (sensorItem == null)
                {
                    sensorItem = new KinectSensorItem(kinectSensor, kinectSensor.DeviceConnectionId);
                    sensorItem.Status = kinectStatus;

                    this.sensorItems.Add(sensorItem);
                }
                else
                {
                    sensorItem.Status = kinectStatus;
                }

                if (KinectStatus.Connected == kinectStatus)
                {
                    // show a window
                    sensorItem.ShowWindow();
                }
                else
                {
                    sensorItem.CloseWindow();
                }
            }
        }

        private void KinectsStatusChanged(object sender, StatusChangedEventArgs e)
        {
            this.ShowStatus(e.Sensor, e.Status);
        }

        private void ShowMoreInfo(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = e.OriginalSource as Hyperlink;
            if (hyperlink != null)
            {
                try
                {
                    // Careful - ensure that this NavigateUri comes from a trusted source, as in this sample, before launching a process using it.
                    Process.Start(new ProcessStartInfo(hyperlink.NavigateUri.ToString()));
                }
                catch (Win32Exception)
                {
                    // No default browser was set to handle the http request or unable to launch the browser
                    MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "Unable to launch the default web browser.  Trying to navigate to: {0}", hyperlink.NavigateUri));
                }
            }

            e.Handled = true;
        }

        private void Sensor_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;

            if (null == element)
            {
                return;
            }

            var sensorItem = element.DataContext as KinectSensorItem;

            if (null == sensorItem)
            {
                return;
            }

            sensorItem.ActivateWindow();
        }
    }
}
