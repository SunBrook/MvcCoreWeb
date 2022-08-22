using Microsoft.AspNetCore.Mvc;
using MvcCoreWeb.Models;
using MvcCoreWeb.Models.DbModels;
using MvcCoreWeb.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcCoreWeb.Services
{
    public class UserService : IUserService
    {
        private readonly MyDbContext _myDbContext;

        public UserService(MyDbContext myDbContext)
        {
            _myDbContext = myDbContext;
        }

        public ApiResult<User> AddUser(User user)
        {
            using (_myDbContext)
            {
                _myDbContext.Users.Add(user);
                int result = _myDbContext.SaveChanges();
                if (result > 0)
                {
                    var userKey = $"_user_{user.Id}";
                    RedisCacheHelper.Set(userKey, user);

                    var allUsers = _myDbContext.Users.Where(c => !c.IsDeleted).ToList();
                    var allUserKey = $"_userlist";
                    RedisCacheHelper.Set(allUserKey, allUsers);

                    return ApiResult.Ok(user);
                }
                return ApiResult<User>.Error("添加失败");
            }
        }

        public ApiResult<User> GetUserById(int id)
        {
            using (_myDbContext)
            {
                var userKey = $"_user_{id}";
                var redisUser = RedisCacheHelper.Get<User>(userKey);
                if (redisUser == null)
                {
                    // 没有找到数据
                    var mysqlUser = _myDbContext.Users.FirstOrDefault(c => c.Id == id);
                    if (mysqlUser != null)
                    {
                        if (mysqlUser.IsDeleted)
                        {
                            return ApiResult<User>.NotFound("人员已删除");
                        }
                        else
                        {
                            RedisCacheHelper.Set(userKey, mysqlUser);
                            return ApiResult.Ok(mysqlUser);
                        }
                    }
                    return ApiResult<User>.NotFound("人员不存在");
                }
                return ApiResult.Ok(redisUser);
            }
        }

        public ApiResult<List<User>> GetUsers()
        {
            using (_myDbContext)
            {
                var usersKey = "_userlist";
                var redisUserList = RedisCacheHelper.Get<List<User>>(usersKey);
                if (redisUserList == null || redisUserList.Count > 0)
                {
                    // 没有找到数据
                    var mySqlUserList = _myDbContext.Users.Where(c => !c.IsDeleted).ToList();
                    if (mySqlUserList.Count == 0)
                    {
                        return ApiResult<List<User>>.NotFound("没有用户数据");
                    }
                    else
                    {
                        RedisCacheHelper.Set(usersKey, mySqlUserList);
                        return ApiResult.Ok(mySqlUserList);
                    }
                }
                else
                {
                    return ApiResult.Ok(redisUserList);
                }
            }
        }

        public ApiResult<User> EditUser(User user)
        {
            using (_myDbContext)
            {
                var mysqlUser = _myDbContext.Users.FirstOrDefault(c => c.Id == user.Id);
                if (mysqlUser == null)
                {
                    return ApiResult<User>.NotFound("修改失败，用户不存在");
                }
                mysqlUser.Name = user.Name;
                mysqlUser.Sex = user.Sex;
                mysqlUser.ModifyTime = DateTime.Now;
                int result = _myDbContext.SaveChanges();
                if (result > 0)
                {
                    var userKey = $"_user_{mysqlUser.Id}";
                    RedisCacheHelper.Set(userKey, mysqlUser);

                    var allUsers = _myDbContext.Users.ToList();
                    var allUserKey = $"_userlist";
                    RedisCacheHelper.Set(allUserKey, allUsers);
                }
                return ApiResult.Ok(mysqlUser);
            }
        }

        public ActionResult RemoveUser_RESTFUL(int id)
        {
            using (_myDbContext)
            {
                var mysqlUser = _myDbContext.Users.FirstOrDefault(c => c.Id == id);
                if (mysqlUser == null)
                {
                    return new NotFoundResult();
                }
                else
                {
                    mysqlUser.IsDeleted = true;
                    mysqlUser.ModifyTime = DateTime.Now;
                    _myDbContext.SaveChanges();

                    var userKey = $"_user_{mysqlUser.Id}";
                    RedisCacheHelper.Remove(userKey);

                    var allUsers = _myDbContext.Users.Where(c => !c.IsDeleted).ToList();
                    var allUserKey = $"_userlist";
                    if (allUsers.Count == 0)
                    {
                        RedisCacheHelper.Remove(allUserKey);
                    }
                    else
                    {
                        RedisCacheHelper.Set(allUserKey, allUsers);
                    }

                    return new NoContentResult();
                }
            }
        }

        public ApiResult RemoveUser(int id)
        {
            using (_myDbContext)
            {
                var mysqlUser = _myDbContext.Users.FirstOrDefault(c => c.Id == id);
                if (mysqlUser == null)
                {
                    return ApiResult.NotFound();
                }
                else
                {
                    mysqlUser.IsDeleted = true;
                    mysqlUser.ModifyTime = DateTime.Now;
                    _myDbContext.SaveChanges();

                    var userKey = $"_user_{mysqlUser.Id}";
                    RedisCacheHelper.Remove(userKey);

                    var allUsers = _myDbContext.Users.Where(c => !c.IsDeleted).ToList();
                    var allUserKey = $"_userlist";
                    if (allUsers.Count == 0)
                    {
                        RedisCacheHelper.Remove(allUserKey);
                    }
                    else
                    {
                        RedisCacheHelper.Set(allUserKey, allUsers);
                    }

                    return ApiResult.NoContent();
                }
            }
        }
    }
}
