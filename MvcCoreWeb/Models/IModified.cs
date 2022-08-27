using System;
using System.ComponentModel.DataAnnotations;

namespace MvcCoreWeb.Models
{
    public interface IModified
    {
        [ScaffoldColumn(false)]
        int ModifyId { get; set; }
        [ScaffoldColumn(false)]
        string ModifyName { get; set; }
        [ScaffoldColumn(false)]
        DateTime ModifyTime { get; set; }
    }
}
