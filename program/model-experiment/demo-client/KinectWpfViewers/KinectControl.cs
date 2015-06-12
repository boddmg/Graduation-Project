// -----------------------------------------------------------------------
// <copyright file="KinectControl.cs" company="Microsoft IT">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Kinect;

    /// <summary>
    /// A base class for a KinectSensor-based UserControl.
    /// The properties exposed are slanted towards image/visual-based output, but this is not required.
    /// </summary>
    public abstract class KinectControl : UserControl
    {
        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(KinectControl),
                new UIPropertyMetadata(null, KinectSensorManagerChanged));

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }

        /// <summary>
        /// Virtual method which can be used to react to changes of the underlying KinectSensor.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain the old and new values</param>
        protected virtual void OnKinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
        }

        /// <summary>
        /// Virtual method which can be used to react to status changes of the underlying KinectSensor.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain the old and new values</param>
        protected virtual void OnKinectStatusChanged(object sender, KinectSensorManagerEventArgs<KinectStatus> args)
        {
        }

        /// <summary>
        /// Virtual method which can be used to react to running state changes of the underlying KinectSensor.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain the old and new values</param>
        protected virtual void OnKinectRunningStateChanged(object sender, KinectSensorManagerEventArgs<bool> args)
        {
        }

        /// <summary>
        /// Virtual method which can be used to react to app conflict state changes of the underlying KinectSensor.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain the old and new values</param>
        protected virtual void OnKinectAppConflictChanged(object sender, KinectSensorManagerEventArgs<bool> args)
        {
        }

        /// <summary>
        /// Virtual method which can be used to react to changes of the SkeletonEngine which would 
        /// necessitate reseting AudioStream-dependent state.
        /// Workaround for Microsoft.Kinect.dll 1.X bug where enabling/diabling the SkeletonEngine resets audio.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain no useful information</param>
        protected virtual void OnAudioWasResetBySkeletonEngine(object sender, EventArgs args)
        {
        }

        /// <summary>
        /// This callback helps us call the virtual On*Changed events.  These can occur because of changes
        /// in the KinectSensor - for which we register notifiers - or because the KinectSensorManager itself
        /// has changed.
        /// </summary>
        /// <param name="sender">The DependencyObject on which the property changed.</param>
        /// <param name="args">The args describing the change.</param>
        private static void KinectSensorManagerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var kinectControl = sender as KinectControl;

            if (null == kinectControl)
            {
                return;
            }

            var oldWrapper = args.OldValue as KinectSensorManager;
            var newWrapper = args.NewValue as KinectSensorManager;

            KinectSensor oldSensor = null;
            KinectSensor newSensor = null;

            if (null != oldWrapper)
            {
                oldWrapper.KinectSensorChanged -= kinectControl.OnKinectSensorChanged;
                oldWrapper.KinectStatusChanged -= kinectControl.OnKinectStatusChanged;
                oldWrapper.KinectRunningStateChanged -= kinectControl.OnKinectRunningStateChanged;
                oldWrapper.KinectAppConflictChanged -= kinectControl.OnKinectAppConflictChanged;
                oldWrapper.AudioWasResetBySkeletonEngine -= kinectControl.OnAudioWasResetBySkeletonEngine;

                oldSensor = oldWrapper.KinectSensor;
            }

            if (null != newWrapper)
            {
                newWrapper.KinectSensorChanged += kinectControl.OnKinectSensorChanged;
                newWrapper.KinectStatusChanged += kinectControl.OnKinectStatusChanged;
                newWrapper.KinectRunningStateChanged += kinectControl.OnKinectRunningStateChanged;
                newWrapper.KinectAppConflictChanged += kinectControl.OnKinectAppConflictChanged;
                newWrapper.AudioWasResetBySkeletonEngine += kinectControl.OnAudioWasResetBySkeletonEngine;

                newSensor = newWrapper.KinectSensor;
            }

            var oldStatus = KinectStatus.Undefined;
            var newStatus = KinectStatus.Undefined;

            bool oldRunningState = false;
            bool newRunningState = false;

            bool oldAppConflict = false;
            bool newAppConflict = false;

            if (null != oldSensor)
            {
                oldStatus = oldSensor.Status;
                oldRunningState = oldSensor.IsRunning;
                oldAppConflict = oldWrapper.KinectSensorAppConflict;
            }

            if (null != newSensor)
            {
                newStatus = newSensor.Status;
                newRunningState = newSensor.IsRunning;
                newAppConflict = newWrapper.KinectSensorAppConflict;
            }

            if (!object.ReferenceEquals(oldSensor, newSensor))
            {
                kinectControl.OnKinectSensorChanged(newWrapper, new KinectSensorManagerEventArgs<KinectSensor>(newWrapper, oldSensor, newSensor));
            }

            if (oldStatus != newStatus)
            {
                kinectControl.OnKinectStatusChanged(newWrapper, new KinectSensorManagerEventArgs<KinectStatus>(newWrapper, oldStatus, newStatus));
            }

            if (oldRunningState != newRunningState)
            {
                kinectControl.OnKinectRunningStateChanged(newWrapper, new KinectSensorManagerEventArgs<bool>(newWrapper, oldRunningState, newRunningState));
            }

            if (oldAppConflict != newAppConflict)
            {
                kinectControl.OnKinectAppConflictChanged(newWrapper, new KinectSensorManagerEventArgs<bool>(newWrapper, oldAppConflict, newAppConflict));
            }
        }
    }
}