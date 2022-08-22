using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvcCoreWeb.Models.DbModels;
using MvcCoreWeb.Services;
using MvcCoreWeb.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCoreWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllersWithViews();

            // 设置MySQL
            services.AddDbContext<MyDbContext>(options => options.UseMySql(Configuration.GetConnectionString("MySQL"), ServerVersion.Parse("5.7"))); //MySqlServerVersion.LatestSupportedServerVersion

            // 设置 SqlServer
            services.AddDbContext<SqlDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlServer")));

            // 设置Redis
            var redisConnectionString = Configuration.GetSection("RedisConnectionString:Connection").Value;
            var redisInstanceName = Configuration.GetSection("RedisConnectionString:InstanceName").Value;
            services.AddSingleton(new RedisCacheHelper(redisConnectionString, redisInstanceName));

            // 添加 Swagger 中间件
            //services.AddSwaggerGen();
            services.AddSwaggerSetup();

            services.AddScoped<IUserService, UserService>();
            services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Latest);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //app.UseSwagger();
                //app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // 以下两个配置应当写在 env.IsDevelopment() 判断中，调试模式启动
            // 为 Swagger 生成json文档和 Swagger UI 提供服务
            app.UseSwagger(c =>
            {
                // Swashbuckle 会在 3.0 版规范（官方称为 OpenAPI 规范）中生成并公开 Swagger JSON。 为了支持向后兼容性，可以改为选择以 2.0 格式公开 JSON
                c.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "v1");
                // https://localhost:<port>/apiDoc 访问 swagger，默认为空 https://localhost:<port> 即可访问
                // 自定义路径的话，还需要设置 Properties/launchSetting.json，注释 profiles下的IIS Express，appsettings.json 注释掉 urls
                //c.RoutePrefix = "apiDoc";
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            
            // 使用 wwwroot 静态文件
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});

            app.UseMvc(route =>
            {
                route.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}"
                    );
                route.MapRoute(
                        name: "api",
                        template: "api/{controller}/{action}/{id?}"
                    );
            });

        }
    }
}
