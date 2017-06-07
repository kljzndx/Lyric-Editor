using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.Attributes
{
    public class SettingFieldByNormalAttribute : SettingFieldAttributeBase
    {
        public SettingFieldByNormalAttribute(string settingName, object defaultValue) : base(settingName, defaultValue)
        {

        }
    }
}
