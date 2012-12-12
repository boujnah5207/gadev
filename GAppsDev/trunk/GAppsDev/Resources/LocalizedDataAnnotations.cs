using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace GAppsDev
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LocalizedNameAttribute : DisplayNameAttribute
    {
        public string ResourceName { get; set; }

        public LocalizedNameAttribute(string resourceName)
            : base()
        {
            ResourceName = resourceName;
        }

        public override string DisplayName
        {
            get
            {
                return Loc.Dic.ResourceManager.GetString(ResourceName, CultureInfo.CurrentCulture);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LocalizedRequiredAttribute : RequiredAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return String.Format("{0} {1} {2}", Loc.Dic.validation_TheField, name, Loc.Dic.validation_IsRequired);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LocalizedIntegerAttribute : DataTypeAttribute
    {
        public LocalizedIntegerAttribute()
            : base("integer")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format("{0} {1} {2}", Loc.Dic.validation_TheField, name, Loc.Dic.validation_IsNotInt);
        }

        public override bool IsValid(object value)
        {
            string stringValue = Convert.ToString(value);
            int intValue;

            if (String.IsNullOrEmpty(stringValue)) return true;

            return int.TryParse(stringValue, out intValue);
        }
    }

    public class LocalizedFileAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return Loc.Dic.validation_IsNotFile;
        }
        
        public override bool IsValid(object value)
        {
            if (value == null) return false;

            HttpPostedFileBase file = (HttpPostedFileBase)value;

            return file.ContentLength > 0;
        }
    }

    public class LocalizedMaxLengthAttribute : ValidationAttribute
    {
        public int MaxLength { get; set; }

        public LocalizedMaxLengthAttribute(int maxLength)
            : base()
        {
            MaxLength = maxLength;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format("{0} {1} {2} {3} {4}", Loc.Dic.validation_TheField, name, Loc.Dic.validation_LengthMustBeSmallerThen, MaxLength, Loc.Dic.Characters);
        }

        public override bool IsValid(object value)
        {
            if (String.IsNullOrEmpty((string)value)) return true;

            return ((string)value).Length <= MaxLength;
        }
    }
}