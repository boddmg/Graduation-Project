// -----------------------------------------------------------------------
// <copyright file="KinectSensorManagerEventArgs.cs" company="Microsoft IT">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;

    /// <summary>
    /// A template class for EventArgs for KinectSensorManager On*Changed events.
    /// </summary>
    /// <typeparam name="T">The type of the property that has changed.</typeparam>
    public sealed class KinectSensorManagerEventArgs<T> : EventArgs
    {
        public KinectSensorManagerEventArgs(KinectSensorManager sensorManager, T oldValue, T newValue)
        {
            this.KinectSensorManager = sensorManager;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public KinectSensorManager KinectSensorManager { get; private set; }

        public T OldValue { get; private set; }

        public T NewValue { get; private set; }
    }
}
