//------------------------------------------------------------------------------
// <copyright file="KinectViewer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// A base class for a KinectSensor-based UserControl.
    /// The properties exposed are slanted towards image/visual-based output, but this is not required.
    /// </summary>
    public abstract class KinectViewer : KinectControl
    {
        public static readonly DependencyProperty FlipHorizontallyProperty =
            DependencyProperty.Register(
                "FlipHorizontally", 
                typeof(bool), 
                typeof(KinectViewer),
                new UIPropertyMetadata(false, FlipHorizontallyChanged));

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(
                "Stretch", 
                typeof(Stretch), 
                typeof(KinectViewer), 
                new UIPropertyMetadata(Stretch.Uniform));

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey HorizontalScaleTransformPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HorizontalScaleTransform",
                typeof(Transform),
                typeof(KinectViewer),
                new PropertyMetadata(Transform.Identity));

        public static readonly DependencyProperty HorizontalScaleTransformProperty = HorizontalScaleTransformPropertyKey.DependencyProperty;

        public static readonly DependencyProperty CollectFrameRateProperty =
            DependencyProperty.Register(
                "CollectFrameRate", 
                typeof(bool), 
                typeof(KinectViewer), 
                new PropertyMetadata(false));

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey FrameRatePropertyKey =
            DependencyProperty.RegisterReadOnly(
                "FrameRate",
                typeof(int),
                typeof(KinectViewer),
                new PropertyMetadata(0));

        public static readonly DependencyProperty FrameRateProperty = FrameRatePropertyKey.DependencyProperty;

        public static readonly DependencyProperty RetainImageOnSensorChangeProperty =
            DependencyProperty.Register(
                "RetainImageOnSensorChange",
                typeof(bool),
                typeof(KinectViewer),
                new PropertyMetadata(false));

        private static readonly ScaleTransform FlipXTransform = CreateFlipXTransform();

        private DateTime lastTime = DateTime.MinValue;

        public bool FlipHorizontally
        {
            get { return (bool)GetValue(FlipHorizontallyProperty); }
            set { SetValue(FlipHorizontallyProperty, value); }
        }
        
        public Transform HorizontalScaleTransform
        {
            get { return (Transform)GetValue(HorizontalScaleTransformProperty); }
            private set { SetValue(HorizontalScaleTransformPropertyKey, value); }
        }

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public bool CollectFrameRate
        {
            get { return (bool)GetValue(CollectFrameRateProperty); }
            set { SetValue(CollectFrameRateProperty, value); }
        }

        public int FrameRate
        {
            get { return (int)GetValue(FrameRateProperty); }
            private set { SetValue(FrameRatePropertyKey, value); }
        }
        
        public bool RetainImageOnSensorChange
        {
            get { return (bool)GetValue(RetainImageOnSensorChangeProperty); }
            set { SetValue(RetainImageOnSensorChangeProperty, value); }
        }

        protected int TotalFrames { get; set; }

        protected int LastFrames { get; set; }

        protected void ResetFrameRateCounters()
        {
            if (this.CollectFrameRate)
            {
                this.lastTime = DateTime.MinValue;
                this.TotalFrames = 0;
                this.LastFrames = 0;
            }
        }

        protected void UpdateFrameRate()
        {
            if (this.CollectFrameRate)
            {
                ++this.TotalFrames;

                DateTime cur = DateTime.Now;
                var span = cur.Subtract(this.lastTime);

                if (span >= TimeSpan.FromSeconds(1))
                {
                    // A straight cast will truncate the value, leading to chronic under-reporting of framerate.
                    // rounding yields a more balanced result
                    this.FrameRate = (int)Math.Round((this.TotalFrames - this.LastFrames) / span.TotalSeconds);
                    this.LastFrames = this.TotalFrames;
                    this.lastTime = cur;
                }
            }
        }

        private static ScaleTransform CreateFlipXTransform()
        {
            var flipXTransform = new ScaleTransform(-1, 1);
            flipXTransform.Freeze();
            return flipXTransform;
        }

        private static void FlipHorizontallyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            KinectViewer kinectViewer = sender as KinectViewer;

            if (null != kinectViewer)
            {
                kinectViewer.HorizontalScaleTransform = (bool)args.NewValue ? FlipXTransform : Transform.Identity;
            }
        }
    }
}
