//------------------------------------------------------------------------------
// <copyright file="KinectSensorItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.KinectExplorer
{
    using System;
    using System.ComponentModel;
    using Microsoft.Kinect;

    /// <summary>
    /// A KinectSensorItem maintains a bit of state about a KinectSensor and manages showing/closing
    /// a KinectWindow associated with the KinectSensor.
    /// </summary>
    public class KinectSensorItem : INotifyPropertyChanged
    {
        /// <summary>
        /// The last set status.
        /// </summary>
        private KinectStatus status;

        public KinectSensorItem(KinectSensor sensor, string id)
        {
            this.Sensor = sensor;
            this.Id = id;
        }

        /// <summary>
        /// Part of INotifyPropertyChanged, this event fires whenver a property changes value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public KinectSensor Sensor { get; private set; }

        public string Id { get; private set; }

        public KinectStatus Status
        {
            get
            {
                return this.status;
            }

            set
            {
                if (this.status != value)
                {
                    this.status = value;

                    if (null != this.Window)
                    {
                        this.Window.StatusChanged(value);
                    }

                    this.NotifyPropertyChanged("Status");
                }
            }
        }

        private KinectWindow Window { get; set; }

        /// <summary>
        /// Ensure a KinectWindow is associated with this KinectSensorItem, and Show it and Activate it.
        /// This can be safely called for a fully operation and visible Window.
        /// </summary>
        public void ShowWindow()
        {
            if (null == this.Window)
            {
                var kinectWindow = new KinectWindow();
                kinectWindow.Closed += this.KinectWindowOnClosed;
                this.Window = kinectWindow;
            }

            this.Window.KinectSensor = this.Sensor;
            this.Window.Show();
            this.Window.Activate();
        }

        /// <summary>
        /// Activate a Window for this sensor if such a Window already exists.
        /// </summary>
        public void ActivateWindow()
        {
            if (null != this.Window)
            {
                this.Window.Activate();
            }
        }


        /// <summary>
        /// Close the KinectWindow associated with this KinectSensorItem, if present.
        /// </summary>
        public void CloseWindow()
        {
            if (null != this.Window)
            {
                this.Window.Close();
                this.Window = null;
            }
        }

        private void KinectWindowOnClosed(object sender, EventArgs e)
        {
            var sensor = this.Window.KinectSensor;
            this.Window.Closed -= this.KinectWindowOnClosed;
            this.Window.KinectSensor = null;
            this.Window = null;

            if ((null != sensor) && sensor.IsRunning)
            {
                sensor.Stop();
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
