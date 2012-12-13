using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using BL;

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

    public class LocalizedFileAttribute : ValidationAttribute
    {
        public long? MaxBytes { get; set; }
        public string FileValidationError { get; set; }

        public LocalizedFileAttribute(long maxBytes = Validations.MAX_FILE_SIZE) : base()
        {
            MaxBytes = maxBytes;
            FileValidationError = null;
        }

        public override string FormatErrorMessage(string name)
        {
            return FileValidationError ?? Loc.Dic.validation_IsNotFile;
        }
        
        public override bool IsValid(object value)
        {
            if(value == null) return false;
            FileValidationError = Validations.UploadedFile((HttpPostedFileBase)value, MaxBytes);

            return FileValidationError == null;
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

    public class LocalizedNumberStringAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return String.Format("{0} {1} {2}", Loc.Dic.validation_TheField, name, Loc.Dic.validation_IsNotNumber);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            string stringValue = Convert.ToString(value);

            return Regex.IsMatch(stringValue, "^[0-9]+$");
        }
    }
}