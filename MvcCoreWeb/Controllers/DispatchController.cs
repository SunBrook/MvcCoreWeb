using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MvcCoreWeb.HangfireJobs;
using MvcCoreWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCoreWeb.Controllers
{
    /// <summary>
    /// 调度队列任务
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DispatchController
    {
        /// <summary>
        /// 添加一个任务到队列，并立即执行
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<DispatchResult> AddEnqueue()
        {
            try
            {
                var jobId = BackgroundJob.Enqueue<IHangfireJob>(x => x.DaliyWork());
                return ApiResult.Ok(new DispatchResult
                {
                    JobId = jobId,
                    Message = "已加入队列，马上执行"
                });
            }
            catch (Exception ex)
            {
                return ApiResult<DispatchResult>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 添加一个延时任务到队列
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<DispatchResult> AddSchedule()
        {
            try
            {
                var jobId = BackgroundJob.Schedule<IHangfireJob>(x => x.DaliyWork(), TimeSpan.FromMinutes(5));
                return ApiResult.Ok(new DispatchResult
                {
                    JobId = jobId,
                    Message = "已加入队列，5分钟后执行"
                });
            }
            catch (Exception ex)
            {
                return ApiResult<DispatchResult>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 添加一个定时任务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<DispatchResult> AddRecurring()
        {
            try
            {
                RecurringJob.AddOrUpdate<IHangfireJob>("12点创建人员", x => x.DaliyWork(), Cron.Daily(12), TimeZoneInfo.Local);
                return ApiResult.Ok(new DispatchResult
                {
                    JobId = "定时任务",
                    Message = "12点创建人员"
                });
            }
            catch (Exception ex)
            {
                return ApiResult<DispatchResult>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 删除后台任务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<DispatchResult> DeleteBgJob(string jobId)
        {
            try
            {
                var deleteResult = BackgroundJob.Delete(jobId);
                return ApiResult.Ok(new DispatchResult
                {
                    JobId = jobId,
                    Message = $"删除: {(deleteResult ? "成功" : "失败")}"
                });
            }
            catch (Exception ex)
            {
                return ApiResult<DispatchResult>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 删除定时任务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<DispatchResult> DeleteReJob(string jobId)
        {
            try
            {
                RecurringJob.RemoveIfExists(jobId);
                return ApiResult.Ok(new DispatchResult
                {
                    JobId = jobId,
                    Message = "定时任务已删除"
                });
            }
            catch (Exception ex)
            {
                return ApiResult<DispatchResult>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 触发定时任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<DispatchResult> TriggerReJob(string jobId)
        {
            try
            {
                var triggerResult = RecurringJob.TriggerJob(jobId);
                return ApiResult.Ok(new DispatchResult
                {
                    JobId = jobId,
                    Message = $"触发：{triggerResult}"
                });
            }
            catch (Exception ex)
            {
                return ApiResult<DispatchResult>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 队列任务立即执行
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<DispatchResult> FlagDone(string jobId)
        {
            try
            {
                var requeueResult = BackgroundJob.Requeue(jobId);
                return ApiResult.Ok(new DispatchResult
                {
                    JobId = jobId,
                    Message = $"标记: {(requeueResult ? "成功" : "失败")}"
                });
            }
            catch (Exception ex)
            {
                return ApiResult<DispatchResult>.Error(ex.ToString());
            }
        }


    }
}
