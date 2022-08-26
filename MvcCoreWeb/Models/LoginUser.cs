namespace MvcCoreWeb.Models
{
    public class LoginUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string[] Roles { get; set; }
        public string Project { get; set; }
    }
}
