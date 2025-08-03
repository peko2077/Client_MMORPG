
using Mymmorpg;

namespace Client.service
{
    public static class PlayersService
    {
        public static void RequestPlayersByUserId(int userId)
        {
            // if (userId <= 0)
            // {
            //     UnityEngine.Debug.LogWarning("[PlayerServiceClient] userId 非法，不能发送请求");
            //     return;
            // }

            ApiRequest playerListRequest = new ApiRequest
            {
                Command = "GetPlayersByUserId",
                GetPlayersByUserId = new GetPlayersByUserIdRequest
                {
                    UserId = userId
                }
            };
            NetworkClient.Instance.SendRequest(playerListRequest);
        }

    }
}


