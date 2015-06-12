//------------------------------------------------------------------------------
// <copyright file="KinectSensorItemCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.KinectExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.Kinect;

    /// <summary>
    /// An ObservableCollection of KinectSensorItems, used to track collection changes.
    /// </summary>
    public class KinectSensorItemCollection : ObservableCollection<KinectSensorItem>
    {
        private readonly Dictionary<KinectSensor, KinectSensorItem> sensorLookup = new Dictionary<KinectSensor, KinectSensorItem>();

        public Dictionary<KinectSensor, KinectSensorItem> SensorLookup
        {
            get
            {
                return this.sensorLookup;
            }
        }

        protected override void InsertItem(int index, KinectSensorItem item)
        {
            if (item == null)
            {
                throw new ArgumentException("Inserted item can't be null.", "item");
            }

            this.SensorLookup.Add(item.Sensor, item);
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this.SensorLookup.Remove(this[index].Sensor);
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            this.SensorLookup.Clear();
            base.ClearItems();
        }
    }
}
