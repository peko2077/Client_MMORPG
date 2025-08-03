using Mymmorpg;

namespace Client.service
{
    public static class LoginUserService
    {
        public static void LoginUser(string userName, string password)
        {
            ApiRequest request = new ApiRequest
            {
                Command = "LoginUser",
                LoginUser = new LoginUserRequest
                {
                    UserName = userName,
                    Password = password,
                }
            };
            NetworkClient.Instance.SendRequest(request); // 发送请求
        }
    }
}
