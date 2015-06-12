// -----------------------------------------------------------------------
// <copyright file="Converters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// This enum is used for generic enabled/disabled bindings.
    /// </summary>
    public enum ToggleState
    {
        /// <summary>
        /// The feature is enabled (true).
        /// </summary>
        Enabled,

        /// <summary>
        /// The feature is disabled (false).
        /// </summary>
        Disabled
    }

    /// <summary>
    /// Converts a true bool value to a ToggleState.Enabled.
    /// </summary>
    public class BoolToToggleStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = ((value is bool) && (bool)value) ^ (null != parameter);
            return isTrue ? ToggleState.Enabled : ToggleState.Disabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ToggleState state = (ToggleState)value;
            return state == ToggleState.Enabled;
        }
    }

    /// <summary>
    /// Converts a true bool value to a false bool value.
    /// This is negated if a non-null parameter is passed.
    /// </summary>
    public class BoolToInverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = ((value is bool) && (bool)value) ^ (null != parameter);
            return !isTrue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = ((value is bool) && (bool)value) ^ (null != parameter);
            return !isTrue;
        }
    }


    /// <summary>
    /// Converts a null value to Visiblity.Collapsed and non-null to Visibility.Visible
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value != null;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a true bool value to Visiblity.Visibility and everything else to Visibility.Collapsed
    /// This is negated if a non-null parameter is passed.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = ((value is bool) && (bool)value) ^ (null != parameter);
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a bool value to a GridLength.
    /// If the value is a true bool, it returns a GridLength of type Star and count equal to the parameter, which
    /// must be convertable to a bool.
    /// If the value is not a true bool, it returns a GridLength of 0.
    /// </summary>
    public class BoolToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = (value is bool) && (bool)value;

            double starCount = DoubleScalerConverter.ObjectToDouble(parameter, 1);
            
            return isVisible ? new GridLength(starCount, GridUnitType.Star) : new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Multiplies a double by a double.
    /// </summary>
    public class DoubleScalerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double valAsDouble = ObjectToDouble(value, 0);
            double factor = ObjectToDouble(parameter, 0);

            return valAsDouble * factor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        internal static double ObjectToDouble(object value, double fallbackValue)
        {
            double retVal = fallbackValue;

            // We need to convert here because the parameter isn't strongly typed.
            // This means that values (int, double, string literals in XAML) won't
            // be auto-converted by the compiler.
            if (value is int)
            {
                retVal = (double)(int)value;
            }
            else if (value is double)
            {
                retVal = (double)value;
            }
            else if (value is string)
            {
                retVal = (double)TypeDescriptor.GetConverter(typeof(double)).ConvertFrom(null, System.Globalization.CultureInfo.InvariantCulture, value);
            }

            return retVal;
        }
    }
}
