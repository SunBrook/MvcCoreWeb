using Hangfire;
using Hangfire.MySql;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MvcCoreWeb.Filters;
using MvcCoreWeb.HangfireJobs;
using MvcCoreWeb.Models.DbModels;
using MvcCoreWeb.Services;
using MvcCoreWeb.Tools;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
            services.AddSwaggerGen(c =>
            {
                //Bearer 的scheme定义
                var securityScheme = new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    //参数添加在头部
                    In = ParameterLocation.Header,
                    //使用Authorize头部
                    Type = SecuritySchemeType.Http,
                    //内容为以 bearer开头
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                //把所有方法配置为增加bearer头部信息
                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "bearerAuth"
                            }
                        },
                        new string[] {}
                    }
                };

                //注册到swagger中
                c.AddSecurityDefinition("bearerAuth", securityScheme);
                c.AddSecurityRequirement(securityRequirement);
            });
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

            // 为不同角色创建策略
            services.AddAuthorization(option =>
            {
                option.AddPolicy("accountSystem", policy => policy.RequireRole("Account", "Employee"));
                option.AddPolicy("Permission", policy => policy.Requirements.Add(new PolicyRequirement()));
            });

            // 添加 jwt 认证
            JwtConfig.Instance = Configuration.GetSection(nameof(JwtConfig)).Get<JwtConfig>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        
                        
                        ValidateLifetime = true,//是否验证失效时间
                        ClockSkew = TimeSpan.FromSeconds(30),

                        ValidateAudience = true,//是否验证Audience
                        //ValidAudience = JwtConfig.Instance.Audience,//Audience
                        // 动态验证，重新登录时，刷新token，旧token强制失效
                        AudienceValidator = (m, n, z) =>
                        {
                            var jwtToken = n as JwtSecurityToken;
                            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value ?? "";
                            var userAudience = RedisCacheHelper.Get<string>($"user_audience_{userId}");
                            return m != null && userAudience != null && m.Contains(userAudience);
                        },

                        ValidateIssuer = true,//是否验证Issuer
                        ValidIssuer = JwtConfig.Instance.Issuer,//Issuer，这两项和前面签发jwt的设置一致

                        ValidateIssuerSigningKey = true,//是否验证SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConfig.Instance.SecretKey))//拿到SecurityKey
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Query["access_token"];
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            // 将过期返回到请求头
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // 注入授权Handler
            //services.AddSingleton<IAuthorizationHandler, PolicyHandler>();
            services.AddScoped<IAuthorizationHandler, PolicyHandler>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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
                //c.SerializeAsV2 = true;
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

            // 添加jwt验证
            app.UseAuthentication();
            // 允许抛出 jwt 异常
            IdentityModelEventSource.ShowPII = true;
            // 使用 Auth
            app.UseAuthorization();

            MyHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());


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
