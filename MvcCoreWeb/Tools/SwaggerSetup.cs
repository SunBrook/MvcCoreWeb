using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;

namespace MvcCoreWeb.Tools
{
    public static class SwaggerSetup
    {
        public static void AddSwaggerSetup(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            const string APINAME = "MvcCoreWeb";
            serviceCollection.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = $"{APINAME} 接口文档",
                    Description = $"{APINAME} HTTP API v1",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "联系人员",
                        Url = new Uri("https://example.com/contact")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "许可证",
                        Url = new Uri("https://example.com/license")
                    }
                });

                c.OrderActionsBy(m => m.RelativePath);

                // 获取xml注释文件的目录
                var xmlPath = Path.Combine(AppContext.BaseDirectory, "MvcCoreWeb.xml");
                c.IncludeXmlComments(xmlPath, true);

            });
        }
    }
}
