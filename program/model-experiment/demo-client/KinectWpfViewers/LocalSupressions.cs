// -----------------------------------------------------------------------
// <copyright file="LocalSupressions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// This property must be read/write as it's being used to pass information from the owning class
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Microsoft.Samples.Kinect.WpfViewers.KinectSkeleton.#JointMappings")]
