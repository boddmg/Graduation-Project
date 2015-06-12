//------------------------------------------------------------------------------
// <copyright file="KinectSettingsViewModel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Data;
    using Microsoft.Kinect;

    /// <summary>
    /// The ViewModel for the KinectSettings class.
    /// It consists of a KinectSensorManager and a set of valid values for various properties.
    /// </summary>
    internal class KinectSettingsViewModel : DependencyObject
    {
        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentSensorColorEnabledProperty =
            DependencyProperty.Register(
                "CurrentSensorColorEnabled",
                typeof(bool),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(false, CurrentSensorColorEnabledChanged));

        public static readonly DependencyProperty CurrentSensorColorFormatProperty =
            DependencyProperty.Register(
                "CurrentSensorColorFormat",
                typeof(ColorImageFormat),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(ColorImageFormat.RgbResolution640x480Fps30, CurrentSensorColorFormatChanged));

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey ColorModesPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "ColorModes",
                typeof(ReadOnlyObservableCollection<ColorMode>),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ColorModesProperty = ColorModesPropertyKey.DependencyProperty;
        
        public static readonly DependencyProperty SelectedColorModeProperty =
            DependencyProperty.Register(
                "SelectedColorMode",
                typeof(ColorMode),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(ColorMode.Off, ColorModeChanged));

        public static readonly DependencyProperty CurrentSensorDepthEnabledProperty =
            DependencyProperty.Register(
                "CurrentSensorDepthEnabled",
                typeof(bool),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(false, CurrentSensorDepthEnabledChanged));

        public static readonly DependencyProperty CurrentSensorDepthFormatProperty =
            DependencyProperty.Register(
                "CurrentSensorDepthFormat",
                typeof(DepthImageFormat),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(DepthImageFormat.Resolution320x240Fps30, CurrentSensorDepthFormatChanged));

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey DepthModesPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "DepthModes",
                typeof(ReadOnlyObservableCollection<DepthMode>),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DepthModesProperty = DepthModesPropertyKey.DependencyProperty;
        public static readonly DependencyProperty SelectedDepthModeProperty =
            DependencyProperty.Register(
                "SelectedDepthMode",
                typeof(DepthMode),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(DepthMode.Off, DepthModeChanged));
        
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey DepthRangesPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "DepthRanges",
                typeof(ReadOnlyObservableCollection<DepthRange>),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DepthRangesProperty = DepthRangesPropertyKey.DependencyProperty;

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey PowerLineFrequencyStatesPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "PowerLineFrequencyStates",
                typeof(ReadOnlyObservableCollection<PowerLineFrequency>),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PowerLineFrequencyStatesProperty = PowerLineFrequencyStatesPropertyKey.DependencyProperty;

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey BacklightCompensationModeStatesPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "BacklightCompensationModeStates",
                typeof(ReadOnlyObservableCollection<BacklightCompensationMode>),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty BacklightCompensationModeStatesProperty = BacklightCompensationModeStatesPropertyKey.DependencyProperty;

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey ToggleStatesPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "ToggleStates",
                typeof(ReadOnlyObservableCollection<ToggleState>),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ToggleStatesProperty = ToggleStatesPropertyKey.DependencyProperty;

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey DepthTreatmentsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "DepthTreatments",
                typeof(ReadOnlyObservableCollection<KinectDepthTreatment>),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DepthTreatmentsProperty = DepthTreatmentsPropertyKey.DependencyProperty;
        public static readonly DependencyProperty SelectedDepthTreatmentProperty =
            DependencyProperty.Register(
                "SelectedDepthTreatment",
                typeof(KinectDepthTreatment),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(KinectDepthTreatment.ClampUnreliableDepths));

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey SkeletonModesPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "SkeletonModes",
                typeof(ReadOnlyObservableCollection<SkeletonMode>),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SkeletonModesProperty = SkeletonModesPropertyKey.DependencyProperty;

        public static readonly DependencyProperty CurrentSensorSkeletonEnabledProperty =
            DependencyProperty.Register(
                "CurrentSensorSkeletonEnabled",
                typeof(bool),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(false, CurrentSensorSkeletonEnabledChanged));

        public static readonly DependencyProperty CurrentSensorSkeletonTrackingModeProperty =
            DependencyProperty.Register(
                "CurrentSensorSkeletonTrackingMode",
                typeof(SkeletonTrackingMode),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(SkeletonTrackingMode.Default, CurrentSensorSkeletonTrackingModeChanged));

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "ReadOnlyDependencyProperty requires private static field to be initialized prior to the public static field")]
        private static readonly DependencyPropertyKey SkeletonChooserModesPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "SkeletonChooserModes",
                typeof(ReadOnlyObservableCollection<SkeletonChooserMode>),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SkeletonChooserModesProperty = SkeletonChooserModesPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SelectedSkeletonModeProperty =
            DependencyProperty.Register(
                "SelectedSkeletonMode",
                typeof(SkeletonMode),
                typeof(KinectSettingsViewModel),
                new PropertyMetadata(SkeletonMode.Off, SkeletonModeChanged));

        public KinectSettingsViewModel()
        {
            // Time to build the value collections
            var colorModes = new ObservableCollection<ColorMode>();

            foreach (ColorMode colorMode in Enum.GetValues(typeof(ColorMode)))
            {
                if (ColorMode.Off != colorMode)
                {
                    colorModes.Add(colorMode);
                }
            }

            colorModes.Add(ColorMode.Off);

            this.ColorModes = new ReadOnlyObservableCollection<ColorMode>(colorModes);

            var depthModes = new ObservableCollection<DepthMode>();

            foreach (DepthMode depthMode in Enum.GetValues(typeof(DepthMode)))
            {
                if (DepthMode.Off != depthMode)
                {
                    depthModes.Add(depthMode);
                }
            }

            depthModes.Add(DepthMode.Off);

            this.DepthModes = new ReadOnlyObservableCollection<DepthMode>(depthModes);

            var depthRanges = new ObservableCollection<DepthRange>();

            foreach (DepthRange depthRange in Enum.GetValues(typeof(DepthRange)))
            {
                depthRanges.Add(depthRange);
            }

            this.DepthRanges = new ReadOnlyObservableCollection<DepthRange>(depthRanges);

            var freqs = new ObservableCollection<PowerLineFrequency>();
            foreach (PowerLineFrequency freq in Enum.GetValues(typeof(PowerLineFrequency)))
            {
                freqs.Add(freq);
            }

            this.PowerLineFrequencyStates = new ReadOnlyObservableCollection<PowerLineFrequency>(freqs);

            var bcms = new ObservableCollection<BacklightCompensationMode>();
            foreach (BacklightCompensationMode bcm in Enum.GetValues(typeof(BacklightCompensationMode)))
            {
                bcms.Add(bcm);
            }

            this.BacklightCompensationModeStates = new ReadOnlyObservableCollection<BacklightCompensationMode>(bcms);

            var toggleStates = new ObservableCollection<ToggleState>();
            foreach (ToggleState state in Enum.GetValues(typeof(ToggleState)))
            {
                toggleStates.Add(state);
            }

            this.ToggleStates = new ReadOnlyObservableCollection<ToggleState>(toggleStates);

            var depthTreatments = new ObservableCollection<KinectDepthTreatment>();

            foreach (KinectDepthTreatment depthTreatment in Enum.GetValues(typeof(KinectDepthTreatment)))
            {
                depthTreatments.Add(depthTreatment);
            }

            this.DepthTreatments = new ReadOnlyObservableCollection<KinectDepthTreatment>(depthTreatments);

            var skeletonModes = new ObservableCollection<SkeletonMode>();

            foreach (SkeletonMode skeletonMode in Enum.GetValues(typeof(SkeletonMode)))
            {
                if (SkeletonMode.Off != skeletonMode)
                {
                    skeletonModes.Add(skeletonMode);
                }
            }

            skeletonModes.Add(SkeletonMode.Off);

            this.SkeletonModes = new ReadOnlyObservableCollection<SkeletonMode>(skeletonModes);

            var skeletonChooserModes = new ObservableCollection<SkeletonChooserMode>();

            foreach (SkeletonChooserMode skeletonChooserMode in Enum.GetValues(typeof(SkeletonChooserMode)))
            {
                skeletonChooserModes.Add(skeletonChooserMode);
            }

            this.SkeletonChooserModes = new ReadOnlyObservableCollection<SkeletonChooserMode>(skeletonChooserModes);

            var currentColorFormatBinding = new Binding("KinectSensorManager.ColorFormat");
            currentColorFormatBinding.Source = this;
            BindingOperations.SetBinding(this, KinectSettingsViewModel.CurrentSensorColorFormatProperty, currentColorFormatBinding);

            var currentColorEnabledBinding = new Binding("KinectSensorManager.ColorStreamEnabled");
            currentColorEnabledBinding.Source = this;
            BindingOperations.SetBinding(this, KinectSettingsViewModel.CurrentSensorColorEnabledProperty, currentColorEnabledBinding);

            var currentDepthBinding = new Binding("KinectSensorManager.DepthFormat");
            currentDepthBinding.Source = this;
            BindingOperations.SetBinding(this, KinectSettingsViewModel.CurrentSensorDepthFormatProperty, currentDepthBinding);

            var currentDepthEnabledBinding = new Binding("KinectSensorManager.DepthStreamEnabled");
            currentDepthEnabledBinding.Source = this;
            BindingOperations.SetBinding(this, KinectSettingsViewModel.CurrentSensorDepthEnabledProperty, currentDepthEnabledBinding);

            var currentSkeletonTrackingBinding = new Binding("KinectSensorManager.SkeletonTrackingMode");
            currentSkeletonTrackingBinding.Source = this;
            BindingOperations.SetBinding(this, KinectSettingsViewModel.CurrentSensorSkeletonTrackingModeProperty, currentSkeletonTrackingBinding);

            var currentSkeletonEnabledBinding = new Binding("KinectSensorManager.SkeletonStreamEnabled");
            currentSkeletonEnabledBinding.Source = this;
            BindingOperations.SetBinding(this, KinectSettingsViewModel.CurrentSensorSkeletonEnabledProperty, currentSkeletonEnabledBinding);
        }

        public enum ColorMode
        {
            /// <summary>
            /// Disable the Color Stream
            /// </summary>
            Off = ColorImageFormat.Undefined,

            /// <summary>
            /// Enable the Color Stream in RgbResolution640x480Fps30 mode
            /// </summary>
            RgbResolution640x480Fps30 = ColorImageFormat.RgbResolution640x480Fps30,

            /// <summary>
            /// Enable the Color Stream in RgbResolution1280x960Fps12 mode
            /// </summary>            
            RgbResolution1280x960Fps12 = ColorImageFormat.RgbResolution1280x960Fps12,

            /// <summary>
            /// Enable the Color Stream in YuvResolution640x480Fps15 mode
            /// </summary>
            YuvResolution640x480Fps15 = ColorImageFormat.YuvResolution640x480Fps15,

            /// <summary>
            /// Enable the Color Stream in InfraredResolution640x480Fps30 mode
            /// </summary>
            InfraredResolution640x480Fps30 = ColorImageFormat.InfraredResolution640x480Fps30,

            /// <summary>
            /// Enable the Color Stream in RawBayerResolution640x480Fps30 mode
            /// </summary>
            RawBayerResolution640x480Fps30 = ColorImageFormat.RawBayerResolution640x480Fps30,

            /// <summary>
            /// Enable the Color Stream in RawBayerResolution1280x960Fps12 mode
            /// </summary>            
            RawBayerResolution1280x960Fps12 = ColorImageFormat.RawBayerResolution1280x960Fps12
        }

        public enum DepthMode
        {
            /// <summary>
            /// Disable the Depth Stream
            /// </summary>
            Off = DepthImageFormat.Undefined,

            /// <summary>
            /// Enable the Depth Stream in Resolution640x480Fps30 mode
            /// </summary>
            Resolution640x480Fps30 = DepthImageFormat.Resolution640x480Fps30,

            /// <summary>
            /// Enable the Depth Stream in Resolution320x240Fps30 mode
            /// </summary>
            Resolution320x240Fps30 = DepthImageFormat.Resolution320x240Fps30,
            
            /// <summary>
            /// Enable the Depth Stream in Resolution80x60Fps30 mode
            /// </summary>
            Resolution80x60Fps30 = DepthImageFormat.Resolution80x60Fps30
        }

        public enum SkeletonMode
        {
            /// <summary>
            /// Enable the Skeleton Stream in Default mode
            /// </summary>
            Default = SkeletonTrackingMode.Default,

            /// <summary>
            /// Enable the Skeleton Stream in Seated mode
            /// </summary>            
            Seated = SkeletonTrackingMode.Seated,

            /// <summary>
            /// Disable the Skeleton Stream
            /// </summary>
            Off
        }

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)this.GetValue(KinectSensorManagerProperty); }
            set { this.SetValue(KinectSensorManagerProperty, value); }
        }

        // Color properties
        // Targets for bindings to reflect sensor status
        public bool CurrentSensorColorEnabled
        {
            get { return (bool)GetValue(CurrentSensorColorEnabledProperty); }
            set { SetValue(CurrentSensorColorEnabledProperty, value); }
        }

        public ColorImageFormat CurrentSensorColorFormat
        {
            get { return (ColorImageFormat)GetValue(CurrentSensorColorFormatProperty); }
            set { SetValue(CurrentSensorColorFormatProperty, value); }
        }

        // Color Modes
        public ReadOnlyObservableCollection<ColorMode> ColorModes
        {
            get { return (ReadOnlyObservableCollection<ColorMode>)this.GetValue(ColorModesProperty); }
            private set { this.SetValue(ColorModesPropertyKey, value); }
        }

        public ColorMode SelectedColorMode
        {
            get { return (ColorMode)GetValue(SelectedColorModeProperty); }
            set { SetValue(SelectedColorModeProperty, value); }
        }
        
        // Depth properties
        // Targets for bindings to reflect sensor status
        public bool CurrentSensorDepthEnabled
        {
            get { return (bool)GetValue(CurrentSensorDepthEnabledProperty); }
            set { SetValue(CurrentSensorDepthEnabledProperty, value); }
        }

        public DepthImageFormat CurrentSensorDepthFormat
        {
            get { return (DepthImageFormat)GetValue(CurrentSensorDepthFormatProperty); }
            set { SetValue(CurrentSensorDepthFormatProperty, value); }
        }

        // Depth Modes and Ranges
        public ReadOnlyObservableCollection<DepthMode> DepthModes
        {
            get { return (ReadOnlyObservableCollection<DepthMode>)this.GetValue(DepthModesProperty); }
            private set { this.SetValue(DepthModesPropertyKey, value); }
        }

        public DepthMode SelectedDepthMode
        {
            get { return (DepthMode)GetValue(SelectedDepthModeProperty); }
            set { SetValue(SelectedDepthModeProperty, value); }
        }

        public ReadOnlyObservableCollection<DepthRange> DepthRanges
        {
            get { return (ReadOnlyObservableCollection<DepthRange>)this.GetValue(DepthRangesProperty); }
            private set { this.SetValue(DepthRangesPropertyKey, value); }
        }

        public ReadOnlyObservableCollection<PowerLineFrequency> PowerLineFrequencyStates
        {
            get { return (ReadOnlyObservableCollection<PowerLineFrequency>)this.GetValue(PowerLineFrequencyStatesProperty); }
            private set { this.SetValue(PowerLineFrequencyStatesPropertyKey, value); }
        }

        public ReadOnlyObservableCollection<BacklightCompensationMode> BacklightCompensationModeStates
        {
            get { return (ReadOnlyObservableCollection<BacklightCompensationMode>)this.GetValue(BacklightCompensationModeStatesProperty); }
            private set { this.SetValue(BacklightCompensationModeStatesPropertyKey, value); }
        }

        public ReadOnlyObservableCollection<ToggleState> ToggleStates
        {
            get { return (ReadOnlyObservableCollection<ToggleState>)this.GetValue(ToggleStatesProperty); }
            private set { this.SetValue(ToggleStatesPropertyKey, value); }
        }

        public ReadOnlyObservableCollection<KinectDepthTreatment> DepthTreatments
        {
            get { return (ReadOnlyObservableCollection<KinectDepthTreatment>)this.GetValue(DepthTreatmentsProperty); }
            private set { this.SetValue(DepthTreatmentsPropertyKey, value); }
        }

        public KinectDepthTreatment SelectedDepthTreatment
        {
            get { return (KinectDepthTreatment)GetValue(SelectedDepthTreatmentProperty); }
            set { SetValue(SelectedDepthTreatmentProperty, value); }
        }

        // Skeleton properties
        // Targets for bindings to reflect sensor status
        public bool CurrentSensorSkeletonEnabled
        {
            get { return (bool)GetValue(CurrentSensorSkeletonEnabledProperty); }
            set { SetValue(CurrentSensorSkeletonEnabledProperty, value); }
        }

        public SkeletonTrackingMode CurrentSensorSkeletonTrackingMode
        {
            get { return (SkeletonTrackingMode)GetValue(CurrentSensorSkeletonTrackingModeProperty); }
            set { SetValue(CurrentSensorSkeletonTrackingModeProperty, value); }
        }

        // Skeleton Modes and Chooser Mode
        public ReadOnlyObservableCollection<SkeletonMode> SkeletonModes
        {
            get { return (ReadOnlyObservableCollection<SkeletonMode>)this.GetValue(SkeletonModesProperty); }
            private set { this.SetValue(SkeletonModesPropertyKey, value); }
        }

        public SkeletonMode SelectedSkeletonMode
        {
            get { return (SkeletonMode)GetValue(SelectedSkeletonModeProperty); }
            set { SetValue(SelectedSkeletonModeProperty, value); }
        }

        public ReadOnlyObservableCollection<SkeletonChooserMode> SkeletonChooserModes
        {
            get { return (ReadOnlyObservableCollection<SkeletonChooserMode>)this.GetValue(SkeletonChooserModesProperty); }
            private set { this.SetValue(SkeletonChooserModesPropertyKey, value); }
        }
        
        // Change handlers
        private static void CurrentSensorColorEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            KinectSettingsViewModel ksvm = dependencyObject as KinectSettingsViewModel;

            if ((null == ksvm) || (null == ksvm.KinectSensorManager) || !(args.NewValue is bool))
            {
                return;
            }

            var isEnabled = (bool)args.NewValue;

            if (!isEnabled || (null == ksvm.KinectSensorManager) || (ColorImageFormat.Undefined == ksvm.KinectSensorManager.ColorFormat))
            {
                ksvm.SelectedColorMode = ColorMode.Off;
            }
            else
            {
                ksvm.SelectedColorMode = (ColorMode)ksvm.KinectSensorManager.ColorFormat;
            }
        }

        private static void CurrentSensorColorFormatChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            KinectSettingsViewModel ksvm = dependencyObject as KinectSettingsViewModel;

            if ((null == ksvm) || (null == ksvm.KinectSensorManager) || !(args.NewValue is ColorImageFormat))
            {
                return;
            }

            var newMode = (ColorImageFormat)args.NewValue;

            if ((ColorImageFormat.Undefined == newMode) || (null == ksvm.KinectSensorManager) || !ksvm.KinectSensorManager.ColorStreamEnabled)
            {
                ksvm.SelectedColorMode = ColorMode.Off;
            }
            else
            {
                ksvm.SelectedColorMode = (ColorMode)newMode;
            }
        }

        private static void ColorModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            KinectSettingsViewModel ksvm = dependencyObject as KinectSettingsViewModel;

            if ((null == ksvm) || (null == ksvm.KinectSensorManager) || !(args.NewValue is ColorMode))
            {
                return;
            }

            var newMode = (ColorMode)args.NewValue;

            if (ColorMode.Off == newMode)
            {
                ksvm.KinectSensorManager.ColorStreamEnabled = false;
            }
            else
            {
                ksvm.KinectSensorManager.ColorStreamEnabled = true;
                ksvm.KinectSensorManager.ColorFormat = (ColorImageFormat)newMode;
            }
        }

        private static void CurrentSensorDepthEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            KinectSettingsViewModel ksvm = dependencyObject as KinectSettingsViewModel;

            if ((null == ksvm) || (null == ksvm.KinectSensorManager) || !(args.NewValue is bool))
            {
                return;
            }

            var isEnabled = (bool)args.NewValue;

            if (!isEnabled || (null == ksvm.KinectSensorManager) || (DepthImageFormat.Undefined == ksvm.KinectSensorManager.DepthFormat))
            {
                ksvm.SelectedDepthMode = DepthMode.Off;
            }
            else
            {
                ksvm.SelectedDepthMode = (DepthMode)ksvm.KinectSensorManager.DepthFormat;
            }
        }

        private static void CurrentSensorDepthFormatChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            KinectSettingsViewModel ksvm = dependencyObject as KinectSettingsViewModel;

            if ((null == ksvm) || (null == ksvm.KinectSensorManager) || !(args.NewValue is DepthImageFormat))
            {
                return;
            }

            var newMode = (DepthImageFormat)args.NewValue;

            if ((DepthImageFormat.Undefined == newMode) || (null == ksvm.KinectSensorManager) || !ksvm.KinectSensorManager.DepthStreamEnabled)
            {
                ksvm.SelectedDepthMode = DepthMode.Off;
            }
            else
            {
                ksvm.SelectedDepthMode = (DepthMode)newMode;
            }
        }

        private static void DepthModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            KinectSettingsViewModel ksvm = dependencyObject as KinectSettingsViewModel;

            if ((null == ksvm) || (null == ksvm.KinectSensorManager) || !(args.NewValue is DepthMode))
            {
                return;
            }

            var newMode = (DepthMode)args.NewValue;

            if (DepthMode.Off == newMode)
            {
                ksvm.KinectSensorManager.DepthStreamEnabled = false;
            }
            else
            {
                ksvm.KinectSensorManager.DepthStreamEnabled = true;
                ksvm.KinectSensorManager.DepthFormat = (DepthImageFormat)newMode;
            }
        }

        private static void CurrentSensorSkeletonEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            KinectSettingsViewModel ksvm = dependencyObject as KinectSettingsViewModel;

            if ((null == ksvm) || (null == ksvm.KinectSensorManager) || !(args.NewValue is bool))
            {
                return;
            }

            var isEnabled = (bool)args.NewValue;

            if (!isEnabled || (null == ksvm.KinectSensorManager))
            {
                ksvm.SelectedSkeletonMode = SkeletonMode.Off;
            }
            else
            {
                ksvm.SelectedSkeletonMode = (SkeletonMode)ksvm.KinectSensorManager.SkeletonTrackingMode;
            }
        }

        private static void CurrentSensorSkeletonTrackingModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            KinectSettingsViewModel ksvm = dependencyObject as KinectSettingsViewModel;

            if ((null == ksvm) || (null == ksvm.KinectSensorManager) || !(args.NewValue is SkeletonTrackingMode))
            {
                return;
            }

            var newMode = (SkeletonTrackingMode)args.NewValue;

            // If the skeleton stream is not enabled, we do not need to react to a mode change
            if ((null != ksvm.KinectSensorManager) && (SkeletonMode.Off != ksvm.SelectedSkeletonMode))
            {
                ksvm.SelectedSkeletonMode = (SkeletonMode)newMode;
            }
        }

        private static void SkeletonModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            KinectSettingsViewModel ksvm = dependencyObject as KinectSettingsViewModel;

            if ((null == ksvm) || (null == ksvm.KinectSensorManager) || !(args.NewValue is SkeletonMode))
            {
                return;
            }

            var newMode = (SkeletonMode)args.NewValue;

            if (SkeletonMode.Off == newMode)
            {
                ksvm.KinectSensorManager.SkeletonStreamEnabled = false;
            }
            else
            {
                // Always set the mode
                ksvm.KinectSensorManager.SkeletonTrackingMode = (SkeletonTrackingMode)newMode;

                // Are we transitioning from off to on?
                if (SkeletonMode.Off == (SkeletonMode)args.OldValue)
                {
                    // If so, enable the Skeleton Stream
                    ksvm.KinectSensorManager.SkeletonStreamEnabled = true;

                    // This may not "take" if there's no skeletal tracking available
                    if (!ksvm.KinectSensorManager.SkeletonStreamEnabled)
                    {
                        // It didn't work - set the SkeletonMode back to Off.
                        // We could use validation or coercion to ensure that the SkeletonMode can't be enabled if there's no ST pipeline available.
                        // Alternately, we could just set ksvm.SelectedSkeletonMode to Off here directly.
                        // However, ListBox and other controls do not respond to property updates that occur during a change
                        // callback of that same property, so we need to post a work item to change this value after we return.
                        ksvm.Dispatcher.BeginInvoke((Action)(() => { ksvm.SelectedSkeletonMode = SkeletonMode.Off; }));
                    }
                }
            }
        }
    }
}
