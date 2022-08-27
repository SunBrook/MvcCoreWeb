using System;
using System.Collections.Generic;

namespace MvcCoreWeb.Tools
{
    public class EnumTools
    {
        public static List<EnumEntity> EnumToList<T>() where T : Enum
        {
            List<EnumEntity> list = new List<EnumEntity>();
            foreach (var e in Enum.GetValues(typeof(T)))
            {
                EnumEntity m = new EnumEntity
                {
                    EnumValue = Convert.ToInt32(e),
                    EnumName = e.ToString()
                };
                list.Add(m);
            }
            return list;
        }
    }

    public class EnumEntity
    {
        /// <summary>  
        /// 枚举名称  
        /// </summary>  
        public string EnumName { set; get; }

        /// <summary>  
        /// 枚举对象的值  
        /// </summary>  
        public int EnumValue { set; get; }
    }
}
