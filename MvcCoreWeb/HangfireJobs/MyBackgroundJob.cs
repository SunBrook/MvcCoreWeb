using MvcCoreWeb.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCoreWeb.HangfireJobs
{
    public class MyBackgroundJob : IHangfireJob
    {
        private readonly MyDbContext _myContext;
        private readonly SqlDbContext _sqlContext;
        public MyBackgroundJob(MyDbContext myDbContext, SqlDbContext sqlDbContext)
        {
            _myContext = myDbContext;
            _sqlContext = sqlDbContext;
        }

        public void DaliyWork()
        {
            //// 随机添加一个人员
            //using (_myContext)
            //{
            //    var now = DateTime.Now;
            //    var user = new User
            //    {
            //        Name = now.ToString("yyyyMMdd"),
            //        Sex = Models.Enums.Sex.未填,
            //        ModifyTime = now,
            //        CreateTime = now
            //    };

            //    _myContext.Users.Add(user);
            //    _myContext.SaveChanges();
            //}

            // 随机添加一个人员
            using (_sqlContext)
            {
                var account = new Account
                {
                    UserName = Guid.NewGuid().ToString("N").Substring(0,8),
                    Password = "123456"
                };

                _sqlContext.Account.Add(account);
                _sqlContext.SaveChanges();
            }
        }
    }
}
