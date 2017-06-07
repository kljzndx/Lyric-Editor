using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.Attributes
{
    [System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public abstract class SettingFieldAttributeBase : Attribute
    {
        /// <summary>
        /// 设置项名
        /// </summary>
        public string SettingName { get; private set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; private set; }
        /// <summary>
        /// 值转换器
        /// </summary>
        public Func<string, object> Convert { get; protected set; }


        public SettingFieldAttributeBase(string settingName, object defaultValue)
        {
            SettingName = settingName;
            DefaultValue = defaultValue;
        }
    }
}
