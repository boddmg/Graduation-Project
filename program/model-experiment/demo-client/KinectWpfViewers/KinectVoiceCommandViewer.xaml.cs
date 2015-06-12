//------------------------------------------------------------------------------
// <copyright file="KinectVoiceCommandViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for KinectVoiceCommandViewer.xaml
    /// </summary>
    public partial class KinectVoiceCommandViewer : KinectViewer
    {
        public KinectVoiceCommandViewer()
        {
            InitializeComponent();
        }

        // Event: VoiceCommand is changing the status of kinect, host should know
        public delegate void VoiceCommandActivatedHandler(object sender, EventArgs e);

        public event VoiceCommandActivatedHandler VoiceCommandActivated;

        protected override void OnKinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if ((null != args.OldValue) && (null != args.OldValue.AudioSource))
            {
                // remove old handlers
                kinectSpeechCommander.Stop();
                kinectSpeechCommander.ActiveListeningModeChanged -= this.KinectSpeechCommander_ActiveListeningModeChanged;
                kinectSpeechCommander.SpeechRecognized -= this.KinectSpeechCommander_SpeechRecognized;
            }

            if ((null != args.NewValue) && (null != args.NewValue.AudioSource))
            {
                if (args.NewValue.IsRunning)
                {
                    kinectSpeechCommander.Start(args.NewValue);
                }

                // add new handlers
                kinectSpeechCommander.ActiveListeningModeChanged += this.KinectSpeechCommander_ActiveListeningModeChanged;
                kinectSpeechCommander.SpeechRecognized += this.KinectSpeechCommander_SpeechRecognized;
            }
        }

        protected override void OnKinectRunningStateChanged(object sender, KinectSensorManagerEventArgs<bool> args)
        {
            if ((null != this.KinectSensorManager) && 
                (null != this.KinectSensorManager.KinectSensor))
            {
                if (this.KinectSensorManager.KinectSensor.IsRunning)
                {
                    kinectSpeechCommander.Start(this.KinectSensorManager.KinectSensor);
                }
                else
                {
                    kinectSpeechCommander.Stop();
                }
            }
        }

        protected override void OnAudioWasResetBySkeletonEngine(object sender, EventArgs args)
        {
            // If the SkeletonEngine caused a reset of the Audio system, we need to restart the 
            // KinectSpeechCommander.
            // This is to work around a bug present in 1.0 and maintained in 1.5 for compatibility reasons.
            var kinectSensorManager = this.KinectSensorManager;
            var kinectSensor = (null == kinectSensorManager) ? null : kinectSensorManager.KinectSensor;

            if (null != kinectSensor)
            {
                kinectSpeechCommander.Stop();
                kinectSpeechCommander.Start(kinectSensor);
            }
        }

        private void OnVoiceCommandActivated()
        {
            if (this.VoiceCommandActivated != null)
            {
                EventArgs e = new EventArgs();

                this.VoiceCommandActivated(this, e);
            }
        }

        private void KinectSpeechCommander_ActiveListeningModeChanged(object sender, ActiveListeningModeChangedEventArgs e)
        {
            if (e.ActiveListeningModeEnabled)
            {
                textBlockModeFeedback.Text = "Yes?.....";
                textBlockCommandFeedback.Text = "Say: NEAR MODE or DEFAULT MODE for Range, SEATED MODE or STANDING MODE for Skeleton";
            }
            else
            {
                textBlockModeFeedback.Text = "Say KINECT to activate...";
                textBlockCommandFeedback.Text = string.Empty;
            }
        }

        private void KinectSpeechCommander_SpeechRecognized(object sender, SpeechCommanderEventArgs e)
        {
            switch (e.Command)
            {
                case "IRMODE_NEAR":
                    try
                    {
                        this.KinectSensorManager.DepthRange = DepthRange.Near;
                        textBlockActionFeedback.Text = "Range: Near";
                    }
                    catch (InvalidOperationException)
                    {
                        textBlockActionFeedback.Text = "Near mode is not supported.";
                    }

                    this.OnVoiceCommandActivated();
                    break;
                case "IRMODE_DEFAULT":
                    this.KinectSensorManager.DepthRange = DepthRange.Default;
                    textBlockActionFeedback.Text = "Range: Default";
                    this.OnVoiceCommandActivated();
                    break;
                case "STMODE_SEATED":
                    this.KinectSensorManager.SkeletonTrackingMode = SkeletonTrackingMode.Seated;
                    textBlockActionFeedback.Text = "Skeleton: Seated";
                    this.OnVoiceCommandActivated();
                    break;
                case "STMODE_DEFAULT":
                    this.KinectSensorManager.SkeletonTrackingMode = SkeletonTrackingMode.Default;
                    textBlockActionFeedback.Text = "Skeleton: Default";
                    this.OnVoiceCommandActivated();
                    break;
                default:
                    break;
            }
        }
    }
}
