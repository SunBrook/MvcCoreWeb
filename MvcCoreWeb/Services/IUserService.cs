using Microsoft.AspNetCore.Mvc;
using MvcCoreWeb.Models;
using MvcCoreWeb.Models.DbModels;
using System.Collections.Generic;

namespace MvcCoreWeb.Services
{
    public interface IUserService
    {
        ApiResult<List<User>> GetUsers();

        ApiResult<User> GetUserById(int id);

        ApiResult<User> AddUser(User user);

        ApiResult<User> EditUser(User user);

        ActionResult RemoveUser_RESTFUL(int id);

        ApiResult RemoveUser(int id);
    }
}
