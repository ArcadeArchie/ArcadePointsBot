using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ArcadePointsBot.Util;

public static class EnumUtils
{
    public static IEnumerable GetValues<T>() where T : Enum
    => typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.GetValue(typeof(T)));

    public static EnumDescription ToDescription(this Enum value)
    {
        string? description;
        string? help = null;
        
        var attributes = value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes?.Any() ?? false)
        {
            description = Resources.Enums.ResourceManager.GetString((attributes.First() as DescriptionAttribute)!.Description, Resources.Enums.Culture);
        }
        else
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            description = ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
        }
        
        if(description!.IndexOf(';') is var index && index != -1)
        {
            help = description.Substring(index + 1);
            description = description.Substring(0, index);
        }

        return new EnumDescription() { Value = value, Description = description, Help = help };
    }
}

public class EnumBindingSource : AvaloniaObject /*: MarkupExtension*/
{
    private Type? _enumType;
    public Type? EnumType
    {
        get => _enumType;
        set
        {
            if (value is not null)
            {
                Type enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (!enumType.IsEnum)
                    throw new ArgumentException("Type must be for an Enum.");
            }
            if (_enumType != value)
                _enumType = value;
        }
    }

    public EnumBindingSource() { }

    public EnumBindingSource(Type enumType)
    {
        EnumType = enumType;
    }

    public Array ProvideValue(IServiceProvider serviceProvider)
    {
        if (EnumType is null)
            throw new InvalidOperationException("The EnumType must be specified.");

        Type actualEnumType = Nullable.GetUnderlyingType(EnumType) ?? EnumType;
        Array enumValues = Enum.GetValues(actualEnumType);

        if (EnumType == actualEnumType)
            return enumValues;

        Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
        enumValues.CopyTo(tempArray, 1);
        return tempArray;
    }
}

public class EnumDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is null) return null;
        if (value.GetType().IsEnum)
        {
            return ((Enum)value).ToDescription();
        }
        throw new ArgumentException("Convert:Value must be an enum.");
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is null) return null;
        if(value is EnumDescription enumDescription)
        {
            return enumDescription.Value;
        }
        throw new ArgumentException("ConvertBack:EnumDescription must be an enum.");
    }
}
public class EnumDescriptionsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable list)
        {
            var tmp = new List<EnumDescription>();
            foreach (Enum item in list)
            {
                tmp.Add(item.ToDescription());
            }
            return tmp;
        }
        throw new ArgumentException("Convert:Value must be an enum.");
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is IEnumerable<EnumDescription> enumDescriptions)
        {
            return enumDescriptions.Select(x => x.Value);
        }
        throw new ArgumentException("ConvertBack:EnumDescription must be an enum.");
    }
}

public record EnumDescription
{
    public object Value { get; set; } = null!;

    public string Description { get; set; }= null!;
    
    public string? Help { get; set; }
    
    public override string ToString()
    {
        return Description;
    }
}