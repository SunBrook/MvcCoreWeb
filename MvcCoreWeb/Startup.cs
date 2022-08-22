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

            // ����MySQL
            services.AddDbContext<MyDbContext>(options => options.UseMySql(Configuration.GetConnectionString("MySQL"), ServerVersion.Parse("5.7"))); //MySqlServerVersion.LatestSupportedServerVersion

            // ���� SqlServer
            services.AddDbContext<SqlDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlServer")));

            // ����Redis
            var redisConnectionString = Configuration.GetSection("RedisConnectionString:Connection").Value;
            var redisInstanceName = Configuration.GetSection("RedisConnectionString:InstanceName").Value;
            services.AddSingleton(new RedisCacheHelper(redisConnectionString, redisInstanceName));

            // ��� Swagger �м��
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

            // ������������Ӧ��д�� env.IsDevelopment() �ж��У�����ģʽ����
            // Ϊ Swagger ����json�ĵ��� Swagger UI �ṩ����
            app.UseSwagger(c =>
            {
                // Swashbuckle ���� 3.0 ��淶���ٷ���Ϊ OpenAPI �淶�������ɲ����� Swagger JSON�� Ϊ��֧���������ԣ����Ը�Ϊѡ���� 2.0 ��ʽ���� JSON
                c.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "v1");
                // https://localhost:<port>/apiDoc ���� swagger��Ĭ��Ϊ�� https://localhost:<port> ���ɷ���
                // �Զ���·���Ļ�������Ҫ���� Properties/launchSetting.json��ע�� profiles�µ�IIS Express��appsettings.json ע�͵� urls
                //c.RoutePrefix = "apiDoc";
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            
            // ʹ�� wwwroot ��̬�ļ�
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
