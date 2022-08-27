using MvcCoreWeb.Models;
using System;

namespace MvcCoreWeb.Tools
{
    public static class ModifiedTimeExtensions
    {
        public static void SetModified(this IModified obj, DateTime? modifyTime = null)
        {
            obj.ModifyTime = modifyTime ?? DateTime.Now;
            var user = Current.User;
            if (user != null)
            {
                obj.ModifyId = user.Id;
                obj.ModifyName = user.UserName;
            }
            else
            {
                obj.ModifyId = 0;
                obj.ModifyName = "";
            }
        }
    }
}
