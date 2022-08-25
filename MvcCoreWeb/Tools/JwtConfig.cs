namespace MvcCoreWeb.Tools
{
    public class JwtConfig
    {
        public static JwtConfig Instance { get; set; }
        /// <summary>
        /// 颁发者
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 哪些客户端可以使用
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 密钥（长度必须大于等于16）
        /// </summary>
        public string SecretKey { get; set; }
    }
}
