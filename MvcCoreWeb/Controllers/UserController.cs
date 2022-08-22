using Microsoft.AspNetCore.Mvc;
using MvcCoreWeb.Models;
using MvcCoreWeb.Models.DbModels;
using MvcCoreWeb.Services;
using System;
using System.Collections.Generic;

namespace MvcCoreWeb.Controllers
{
    /// <summary>
    /// 人员
    /// </summary>
    [ApiController]
    [Route("api/user")]
    public class UserController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// 获取人员列表
        /// </summary>
        /// <returns></returns>
        [Route("all"), HttpGet]
        public ApiResult<List<User>> GetUsers()
        {
            try
            {
                return _userService.GetUsers();
            }
            catch (Exception ex)
            {
                return ApiResult<List<User>>.Error(ex.ToString());
            }

        }

        /// <summary>
        /// 获取人员
        /// </summary>
        /// <param name="id">人员Id</param>
        /// <returns></returns>
        [Route("get/{id}"), HttpGet]
        public ApiResult<User> GetUserById(int id)
        {
            try
            {
                return _userService.GetUserById(id);
            }
            catch (Exception ex)
            {
                return ApiResult<User>.Error(ex.ToString());
            }

        }

        /// <summary>
        /// 创建人员
        /// </summary>
        /// <param name="user">人员信息</param>
        /// <returns></returns>
        [Route("create"), HttpPost]
        public ApiResult<User> AddUser([FromBody] User user)
        {
            try
            {
                return _userService.AddUser(user);
            }
            catch (Exception ex)
            {
                return ApiResult<User>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 编辑人员
        /// </summary>
        /// <param name="user">人员信息</param>
        /// <returns></returns>
        [Route("edit"), HttpPut]
        public ApiResult<User> EditUser([FromBody] User user)
        {
            try
            {
                return _userService.EditUser(user);
            }
            catch (Exception ex)
            {

                return ApiResult<User>.Error(ex.ToString());
            }
        }

        ///// <summary>
        ///// 删除人员
        ///// </summary>
        ///// <param name="id">人员Id</param>
        ///// <returns></returns>
        //[Route("remove/{id}"), HttpDelete]
        //public ActionResult RemoveUser_RESTFUL(int id)
        //{
        //    try
        //    {
        //        return _userService.RemoveUser_RESTFUL(id);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new StatusCodeResult(500);
        //    }
        //}

        /// <summary>
        /// 删除人员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("remove/{id}"), HttpDelete]
        public ApiResult RemoveUser(int id)
        {
            try
            {
                return _userService.RemoveUser(id);
            }
            catch (Exception ex)
            {
                return ApiResult.Error(ex.ToString());
            }
        }
    }
}