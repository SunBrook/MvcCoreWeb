using System;
using System.ComponentModel.DataAnnotations;

namespace MvcCoreWeb.Models
{
    public interface IProjectId
    {
        [Range(0, int.MaxValue)]
        int ProjectId { get; set; }
    }
}
