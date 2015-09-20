using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Revolver.Core.Commands.Parameters
{
  public class KeyValueConverter : TypeConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      return sourceType == typeof(string[]);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      var parts = (string[])value;

      if (parts.Length != 2)
        throw new ArgumentException("Wrong number of arguments provided");

      return new KeyValuePair<string, string>(parts[0], parts[1]);
    }
  }
}