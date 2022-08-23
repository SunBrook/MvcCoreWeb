using Hangfire;
using Hangfire.MySql;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvcCoreWeb.Filters;
using MvcCoreWeb.HangfireJobs;
using MvcCoreWeb.Models.DbModels;
using MvcCoreWeb.Services;
using MvcCoreWeb.Tools;
using System;

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

            //// 添加 Hangfire - sqlserver
            //services.AddHangfire(config => config
            //.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            //.UseSimpleAssemblyNameTypeSerializer()
            //.UseRecommendedSerializerSettings()
            //.UseSqlServerStorage(Configuration.GetConnectionString("SqlServer"), new SqlServerStorageOptions
            //{
            //    QueuePollInterval = TimeSpan.FromSeconds(15),             //- 作业队列轮询间隔。默认值为15秒。
            //    JobExpirationCheckInterval = TimeSpan.FromHours(1),       //- 作业到期检查间隔（管理过期记录）。默认值为1小时。
            //    CountersAggregateInterval = TimeSpan.FromMinutes(5),      //- 聚合计数器的间隔。默认为5分钟。
            //    PrepareSchemaIfNecessary = true,                          //- 如果设置为true，则创建数据库表。默认是true。
            //    DashboardJobListLimit = 50000,                            //- 仪表板作业列表限制。默认值为50000。
            //    TransactionTimeout = TimeSpan.FromMinutes(1),             //- 交易超时。默认为1分钟。
            //    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            //    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            //    UseRecommendedIsolationLevel = true,
            //    DisableGlobalLocks = true
            //}));

            // 添加 Hangfire - mysql
            services.AddHangfire(x => x.UseStorage(new MySqlStorage(
                Configuration.GetConnectionString("MySQL"),
                new MySqlStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(15),             //- 作业队列轮询间隔。默认值为15秒。
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),       //- 作业到期检查间隔（管理过期记录）。默认值为1小时。
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),      //- 聚合计数器的间隔。默认为5分钟。
                    PrepareSchemaIfNecessary = true,                          //- 如果设置为true，则创建数据库表。默认是true。
                    DashboardJobListLimit = 50000,                            //- 仪表板作业列表限制。默认值为50000。
                    TransactionTimeout = TimeSpan.FromMinutes(1),             //- 交易超时。默认为1分钟。
                    TablesPrefix = "Hangfire_"
                })));

            //// 添加 Hangfire - redis
            //services.AddHangfire(configuration =>
            //{
            //    configuration.UseRedisStorage(redisConnectionString, new RedisStorageOptions
            //    {
            //        Prefix = "hangfire_",
            //        UseTransactions = true,
            //        InvisibilityTimeout = TimeSpan.FromMinutes(5),
            //        ExpiryCheckInterval = TimeSpan.FromMinutes(5)
            //    });
            //});

            services.AddHangfireServer(); // 代替 启动 hangfire 服务
            services.AddTransient<IHangfireJob, MyBackgroundJob>();

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

            // hangfire 授权配置
            app.UseHangfireDashboard("/hangfire", new DashboardOptions()
            {
                Authorization = new[] { new CustomAuthorizeFilter() }
            });

            app.UseHangfireDashboard(); // 使用 hangfire 面板
            //app.UseHangfireServer(); // 启动 hangfire 服务 [弃用]

            // 周期性任务
            RecurringJob.AddOrUpdate<IHangfireJob>("每日人员新增", j => j.DaliyWork(), "0 0 0 * * ?", TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate<IHangfireJob>("每日人员新增", j => j.DaliyWork(), Cron.Daily(0, 0), TimeZoneInfo.Local);

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
