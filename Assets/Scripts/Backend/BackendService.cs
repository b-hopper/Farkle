using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Farkle.Backend
{
    public static class BackendService
    {
        static BackendConfig _config;

        public static void Initialize(BackendConfig config)
        {
            _config = config;
        }

        static void EnsureConfig()
        {
            if (_config == null)
            {
                throw new InvalidOperationException("BackendService not initialized. Call BackendService.Initialize(config) once at startup.");
            }
        }

        static string Url(string path) => $"{_config.BaseUrl}{path}";

        static async Task<UnityWebRequest> SendAsync(UnityWebRequest req)
        {
            req.timeout = Mathf.Max(1, _config.TimeoutSeconds);
            var tcs = new TaskCompletionSource<UnityWebRequest>();
            var op  = req.SendWebRequest();
            op.completed += _ => tcs.TrySetResult(req);
            return await tcs.Task;
        }

        static UnityWebRequest BuildJsonPost(string url, string json)
        {
            var bodyRaw = Encoding.UTF8.GetBytes(json);
            var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            return req;
        }

        static T DeserializeOrThrow<T>(UnityWebRequest req)
        {
            if (req.result != UnityWebRequest.Result.Success)
                throw new Exception($"HTTP error: {req.responseCode} {req.error} | {req.downloadHandler?.text}");
            var text = req.downloadHandler.text;
            if (string.IsNullOrEmpty(text))
                throw new Exception("Empty response body.");
            return JsonConvert.DeserializeObject<T>(text);
        }

        // ------------- Endpoints -------------

        public static async Task<CreatePlayerResponse> CreatePlayerAsync(string userId, string displayName)
        {
            EnsureConfig();
            var payload = new CreatePlayerRequest { UserId = userId, DisplayName = displayName };
            var json = JsonConvert.SerializeObject(payload);
            using var req = BuildJsonPost(Url("/create-player"), json);
            var res = await SendAsync(req);
            return DeserializeOrThrow<CreatePlayerResponse>(res);
        }

        public static async Task<PostGameResultResponse> PostGameResultAsync(string userId, System.Collections.Generic.List<GameResultEntry> results)
        {
            EnsureConfig();
            var payload = new PostGameResultRequest { UserId = userId, Results = results };
            var json = JsonConvert.SerializeObject(payload);
            using var req = BuildJsonPost(Url("/game-result"), json);
            var res = await SendAsync(req);
            return DeserializeOrThrow<PostGameResultResponse>(res);
        }

        public static async Task<PlayerStatsResponse> GetPlayerStatsAsync(string playerId)
        {
            EnsureConfig();
            using var req = UnityWebRequest.Get(Url($"/player-stats?player_id={UnityWebRequest.EscapeURL(playerId)}"));
            var res = await SendAsync(req);
            return DeserializeOrThrow<PlayerStatsResponse>(res);
        }

        public static async Task<LeaderboardResponse> GetLeaderboardAsync(string sort = "avg_score")
        {
            EnsureConfig();
            using var req = UnityWebRequest.Get(Url($"/leaderboard?sort={UnityWebRequest.EscapeURL(sort)}"));
            var res = await SendAsync(req);
            return DeserializeOrThrow<LeaderboardResponse>(res);
        }


        public static async Task<UserPlayersResponse> GetUserPlayersAsync(string userId)
        {
            EnsureConfig();
            using var req = UnityWebRequest.Get(Url($"/user-players?user_id={UnityWebRequest.EscapeURL(userId)}"));
            var res = await SendAsync(req);
            return DeserializeOrThrow<UserPlayersResponse>(res);
        }

        private static string _userId;
        
        /// <summary>
        /// Gets the user ID for the current session.
        /// If not set, gets Google Play Games ID or generates a new GUID.
        /// </summary>
        public static string GetUserId()
        {
            if (!string.IsNullOrEmpty(_userId))
                return _userId;

            EnsureConfig();

            // TODO: Replace with actual Google Play Games ID retrieval
            _userId = SystemInfo.deviceUniqueIdentifier;
            return _userId;
        }
    }
}
