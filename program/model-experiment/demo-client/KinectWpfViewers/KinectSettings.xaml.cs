//------------------------------------------------------------------------------
// <copyright file="KinectSettings.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for KinectSettings.xaml
    /// </summary>
    public partial class KinectSettings : KinectControl
    {
        public static readonly DependencyProperty DepthTreatmentProperty =
            DependencyProperty.Register(
                "DepthTreatment",
                typeof(KinectDepthTreatment),
                typeof(KinectSettings),
                new PropertyMetadata(KinectDepthTreatment.ClampUnreliableDepths));

        private readonly KinectSettingsViewModel viewModel = new KinectSettingsViewModel();

        public KinectSettings()
        {
            // We bind the ViewModel's KinectSensorManager to this class's property so changes
            // will be propagated.
            var kinectSensorBinding = new Binding("KinectSensorManager");
            kinectSensorBinding.Source = this;
            BindingOperations.SetBinding(this.viewModel, KinectSettingsViewModel.KinectSensorManagerProperty, kinectSensorBinding);

            // We bind the ViewModel's DepthTreatment to this class's property so changes
            // will be propagated.
            var depthTreatmentBinding = new Binding("DepthTreatment");
            depthTreatmentBinding.Source = this;
            depthTreatmentBinding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(this.viewModel, KinectSettingsViewModel.SelectedDepthTreatmentProperty, depthTreatmentBinding);

            this.InitializeComponent();

            this.ViewModelRoot.DataContext = this.viewModel;
        }

        public KinectDepthTreatment DepthTreatment
        {
            get { return (KinectDepthTreatment)GetValue(DepthTreatmentProperty); }
            set { SetValue(DepthTreatmentProperty, value); }
        }

        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var fe = sender as FrameworkElement;

            if (null != fe)
            {
                if (fe.CaptureMouse())
                {
                    e.Handled = true;
                }
            }
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var fe = sender as FrameworkElement;

            if (null != fe)
            {
                if (fe.IsMouseCaptured)
                {
                    fe.ReleaseMouseCapture();
                    e.Handled = true;
                }
            }
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            var fe = sender as FrameworkElement;

            if (null != fe)
            {
                if (fe.IsMouseCaptured && (null != this.viewModel.KinectSensorManager) && (null != this.viewModel.KinectSensorManager.KinectSensor))
                {
                    var position = Mouse.GetPosition(this.SliderTrack);
                    int newAngle = -27 + (int)Math.Round(54.0 * (this.SliderTrack.ActualHeight - position.Y) / this.SliderTrack.ActualHeight);
                    
                    if (newAngle < -27)
                    {
                        newAngle = -27;
                    }
                    else if (newAngle > 27)
                    {
                        newAngle = 27;
                    }

                    this.viewModel.KinectSensorManager.ElevationAngle = newAngle;
                }
            }
        }

        private void ResetExposure(object sender, EventArgs e)
        {
            var settings = this.viewModel.KinectSensorManager.KinectSensor.ColorStream.CameraSettings;

            // Save non-exposure settings
            var autoWhiteBalance = settings.AutoWhiteBalance;
            var whiteBalance = settings.WhiteBalance;
            var contrast = settings.Contrast;
            var hue = settings.Hue;
            var saturation = settings.Saturation;
            var gamma = settings.Gamma;
            var sharpness = settings.Sharpness;

            // Reset all settings
            settings.ResetToDefault();

            // Restore previous settings
            settings.AutoWhiteBalance = autoWhiteBalance;
            settings.WhiteBalance = whiteBalance;
            settings.Contrast = contrast;
            settings.Hue = hue;
            settings.Saturation = saturation;
            settings.Gamma = gamma;
            settings.Sharpness = sharpness;
        }

        private void ResetColor(object sender, EventArgs e)
        {
            var settings = this.viewModel.KinectSensorManager.KinectSensor.ColorStream.CameraSettings;

            // Save exposure settings
            var autoExposure = settings.AutoExposure;
            var brightness = settings.Brightness;
            var frameInterval = settings.FrameInterval;
            var exposureTime = settings.ExposureTime;
            var gain = settings.Gain;
            var powerLineFrequency = settings.PowerLineFrequency;
            var backlightCompensationMode = settings.BacklightCompensationMode;

            // Reset all settings
            settings.ResetToDefault();

            // Restore previous settings
            settings.AutoExposure = autoExposure;
            settings.Brightness = brightness;
            settings.FrameInterval = frameInterval;
            settings.ExposureTime = exposureTime;
            settings.Gain = gain;
            settings.PowerLineFrequency = powerLineFrequency;
            settings.BacklightCompensationMode = backlightCompensationMode;
        }
    }
}
