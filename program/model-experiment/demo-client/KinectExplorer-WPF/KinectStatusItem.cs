// -----------------------------------------------------------------------
// <copyright file="KinectStatusItem.cs" company="Microsoft IT">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.KinectExplorer
{
    using System;
    using Microsoft.Kinect;

    /// <summary>
    /// A data class that describes a Kinect Sensor status at a point in time.
    /// </summary>
    public class KinectStatusItem
    {
        public string Id { get; set; }

        public KinectStatus Status { get; set; }

        public DateTime DateTime { get; set; }
    }
}
