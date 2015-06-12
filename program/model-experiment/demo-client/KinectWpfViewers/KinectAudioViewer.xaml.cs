//------------------------------------------------------------------------------
// <copyright file="KinectAudioViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows;
    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for KinectAudioViewer.xaml
    /// </summary>
    public partial class KinectAudioViewer : KinectViewer
    {
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey BeamAnglePropertyKey =
            DependencyProperty.RegisterReadOnly(
                "BeamAngle",
                typeof(double),
                typeof(KinectAudioViewer),
                new PropertyMetadata((double)0));

        public static readonly DependencyProperty BeamAngleProperty = BeamAnglePropertyKey.DependencyProperty;

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey SourceAnglePropertyKey =
            DependencyProperty.RegisterReadOnly(
                "SourceAngle",
                typeof(double),
                typeof(KinectAudioViewer),
                new PropertyMetadata((double)0));

        public static readonly DependencyProperty SourceAngleProperty = SourceAnglePropertyKey.DependencyProperty;

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey ConfidenceLevelPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "ConfidenceLevel",
                typeof(int),
                typeof(KinectAudioViewer),
                new PropertyMetadata(0));

        public static readonly DependencyProperty ConfidenceLevelProperty = ConfidenceLevelPropertyKey.DependencyProperty;
        
        private Stream audioStream;

        public KinectAudioViewer()
        {
            InitializeComponent();
            this.ViewModelRoot.DataContext = this;
        }

        public double BeamAngle
        {
            get { return (double)GetValue(BeamAngleProperty); }
            private set { SetValue(BeamAnglePropertyKey, value); }
        }
        
        public double SourceAngle
        {
            get { return (double)GetValue(SourceAngleProperty); }
            private set { SetValue(SourceAnglePropertyKey, value); }
        }
        
        public int Confidence
        {
            get { return (int)GetValue(ConfidenceLevelProperty); }
            private set { SetValue(ConfidenceLevelPropertyKey, value); }
        }

        protected override void OnKinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null == args)
            {
                throw new ArgumentNullException("args");
            }

            if ((null != args.OldValue) && (null != args.OldValue.AudioSource))
            {
                // remove old handlers
                args.OldValue.AudioSource.BeamAngleChanged -= this.AudioSourceBeamChanged;
                args.OldValue.AudioSource.SoundSourceAngleChanged -= this.AudioSourceSoundSourceAngleChanged;
                
                if (null != this.audioStream)
                {
                    this.audioStream.Close();
                }

                args.OldValue.AudioSource.Stop();
            }

            if ((null != args.NewValue) && (null != args.NewValue.AudioSource))
            {
                // add new handlers
                args.NewValue.AudioSource.BeamAngleChanged += this.AudioSourceBeamChanged;
                args.NewValue.AudioSource.SoundSourceAngleChanged += this.AudioSourceSoundSourceAngleChanged;

                this.EnsureAudio(args.NewValue, args.NewValue.Status);
            }
        }

        protected override void OnKinectStatusChanged(object sender, KinectSensorManagerEventArgs<KinectStatus> args)
        {
            if ((null != this.KinectSensorManager) && (null != this.KinectSensorManager.KinectSensor) && (null != this.KinectSensorManager.KinectSensor.AudioSource))
            {
                this.EnsureAudio(this.KinectSensorManager.KinectSensor, this.KinectSensorManager.KinectSensor.Status);
            }            
        }

        protected override void OnAudioWasResetBySkeletonEngine(object sender, EventArgs args)
        {
            if ((null != this.KinectSensorManager) && (null != this.KinectSensorManager.KinectSensor) && (null != this.KinectSensorManager.KinectSensor.AudioSource))
            {
                this.EnsureAudio(this.KinectSensorManager.KinectSensor, this.KinectSensorManager.KinectSensor.Status);
            }
        }

        private void EnsureAudio(KinectSensor sensor, KinectStatus status)
        {
            if ((null != sensor) && (KinectStatus.Connected == status) && sensor.IsRunning)
            {
                this.audioStream = sensor.AudioSource.Start();
            }
        }

        private void AudioSourceSoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            // Set width of mark based on confidence
            this.Confidence = (int)Math.Round(e.ConfidenceLevel * 100);

            // Move indicator
            this.SourceAngle = e.Angle;
        }

        private void AudioSourceBeamChanged(object sender, BeamAngleChangedEventArgs e)
        {
            // Move our indicator
            this.BeamAngle = e.Angle;
        }
    }
}
