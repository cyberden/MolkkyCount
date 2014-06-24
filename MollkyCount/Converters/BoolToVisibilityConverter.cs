using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MollkyCount.Converters
{
    /// <summary>
    /// Allows to convert a boolean to a xaml visibility value and vice versa
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {

        private bool _inverted = false;

        /// <summary>
        /// Gets or sets a value indicating whether the conversion must be inverted Boolean -> Visibilty or Visibilty -> Boolean in the Convert method.
        /// </summary>
        /// <value><c>true</c> if inverted; otherwise, <c>false</c>.</value>
        public bool Inverted
        {
            get { return _inverted; }
            set { _inverted = value; }
        }

        private bool _not = false;

        /// <summary>
        /// Gets or sets a value indicating whether the value Visible of the Visibilty is convert to true or to false and vice versa.
        /// </summary>
        /// <value><c>true</c> if not; otherwise, <c>false</c>.</value>
        public bool Not
        {
            get { return _not; }
            set { _not = value; }
        }

        private object VisibilityToBool(object value)
        {
            if (!(value is Visibility))
                return DependencyProperty.UnsetValue;

            return (((Visibility)value) == Visibility.Visible) ^ Not;
        }

        private object BoolToVisibility(object value)
        {
            if (!(value is bool))
                return DependencyProperty.UnsetValue;

            return ((bool)value ^ Not) ? Visibility.Visible : Visibility.Collapsed;
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Inverted ? VisibilityToBool(value) : BoolToVisibility(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Inverted ? BoolToVisibility(value) : VisibilityToBool(value);

        }


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Inverted ? VisibilityToBool(value) : BoolToVisibility(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Inverted ? BoolToVisibility(value) : VisibilityToBool(value);
        }
    }
}
