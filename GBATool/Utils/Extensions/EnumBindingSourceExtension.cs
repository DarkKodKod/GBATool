using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace GBATool.Utils.Extensions
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type? _enumType;

        public Type? EnumType
        {
            get { return _enumType; }
            set
            {
                if (_enumType == value)
                    return;

                if (value == null)
                    return;

                Type enumType = Nullable.GetUnderlyingType(value) ?? value;
                if (!enumType.IsEnum)
                {
                    throw new ArgumentException("Type must be for an Enum.");
                }

                _enumType = value;
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_enumType == null)
            {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            Type actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;

            Array enumValues = Enum.GetValues(actualEnumType);

            return (
                from object enumValue in enumValues
                select new EnumerationMember
                {
                    Value = enumValue,
                    Description = GetDescription(enumValue)
                }).ToArray();
        }

        public string GetDescription(object enumValue)
        {
            Type? type = EnumType;

            if (type == null)
                return string.Empty;

            string? enumValueString = enumValue.ToString();

            if (enumValueString == null)
                return string.Empty;

            return type?.GetField(enumValueString)?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute descriptionAttribute ? descriptionAttribute.Description : enumValueString;
        }

        public class EnumerationMember
        {
            public string? Description { get; set; }
            public object? Value { get; set; }
        }
    }
}
