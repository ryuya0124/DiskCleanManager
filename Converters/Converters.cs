using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using DiskCleanManager.Models;

namespace DiskCleanManager.Converters;

public class SafetyLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (SafetyLevel)value switch
        {
            SafetyLevel.Safe     => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 34, 197, 94)),   // green-500
            SafetyLevel.Caution  => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 234, 179, 8)),   // yellow-500
            SafetyLevel.Forbidden => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 239, 68, 68)),  // red-500
            _                    => new SolidColorBrush(Colors.Gray),
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class SafetyLevelToEmojiConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (SafetyLevel)value switch
        {
            SafetyLevel.Safe      => "🟢",
            SafetyLevel.Caution   => "🟡",
            SafetyLevel.Forbidden => "⛔",
            _                     => "？",
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class SafetyLevelToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (SafetyLevel)value switch
        {
            SafetyLevel.Safe      => "安全",
            SafetyLevel.Caution   => "注意",
            SafetyLevel.Forbidden => "削除不可",
            _                     => "",
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class ActionTypeToCommandVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (ActionType)value == ActionType.Command ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class ActionTypeToSymlinkVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (ActionType)value == ActionType.Symlink ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class ActionTypeToForbiddenVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (ActionType)value == ActionType.Forbidden ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (bool)value ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (bool)value ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class LinkedStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 99, 102, 241))   // indigo-500
            : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 114, 128)); // gray-500
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class LinkedStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (bool)value ? "🔗 リンク済み" : "未リンク";
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => !(bool)value;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
