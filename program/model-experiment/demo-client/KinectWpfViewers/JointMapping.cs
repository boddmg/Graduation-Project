//------------------------------------------------------------------------------
// <copyright file="JointMapping.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System.Windows;
    using Microsoft.Kinect;

    /// <summary>
    /// This class is used to map points between skeleton and color/depth
    /// </summary>
    public class JointMapping
    {
        /// <summary>
        /// Gets or sets the joint at which we we are looking
        /// </summary>
        public Joint Joint { get; set; }

        /// <summary>
        /// Gets or sets the point mapped into the target display
        /// </summary>
        public Point MappedPoint { get; set; }
    }
}
