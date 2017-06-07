using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.Attributes
{
    public class SettingFieldByEnumAttribute : SettingFieldAttributeBase
    {
        public SettingFieldByEnumAttribute(string settingName, object defaultValue, Type enumType) : base(settingName, defaultValue)
        {
            base.Convert = s => Enum.Parse(enumType, s);
        }
    }
}
