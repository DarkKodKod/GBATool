using GBATool.ViewModels;
using System.ComponentModel;
using System.Globalization;

namespace GBATool.Utils.CustomTypeConverters
{
    public class ProjectItemTypeConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            string? content = value.ToString();

            return new ProjectItem(content);
        }
    }
}
