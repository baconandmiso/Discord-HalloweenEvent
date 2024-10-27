using System.Collections.Concurrent;
using ReadonlyLocalVariables;

namespace Common.Throttle;

public interface IThrottleService
{
    /// <summary>
    ///     クールダウンリセットまでの時間を取得
    /// </summary>
    /// <param name="throttleBy">実行者単位(ユーザー/サーバー)</param>
    /// <param name="requestsLimit">リクエスト可能回数</param>
    /// <param name="intervalSeconds">クールダウン(秒)</param>
    /// <param name="user">ユーザー</param>
    /// <param name="guild">サーバー</param>
    /// <param name="command">コマンド</param>
    /// <returns></returns>
    TimeSpan GetThrottleReset(ThrottleBy throttleBy, int requestsLimit, int intervalSeconds, IUser user, IGuild guild, string? command = null);

    /// <summary>
    ///     クールダウン中か調べる
    /// </summary>
    /// <param name="throttleBy"></param>
    /// <param name="limit"></param>
    /// <param name="intervalSeconds"></param>
    /// <param name="user"></param>
    /// <param name="guild"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    bool CheckThrottle(ThrottleBy throttleBy, int limit, int intervalSeconds, IUser user, IGuild guild, string? command = null);
}

public class ThrottleService : IThrottleService
{
    class ThrottleInfo
    {
        /// <summary>
        ///     最初にリクエストした時間
        /// </summary>
        public DateTime FirstRequestTime { get; set; }

        /// <summary>
        ///     リクエストした回数
        /// </summary>
        public int RequestCount { get; set; }
    }

    class ThrottleKey
    {
        /// <summary>
        ///     ユーザーId / サーバーId が入る
        /// </summary>
        public ulong ObjectId { get; }

        /// <summary>
        ///     コマンド名
        /// </summary>
        public string Command { get; }

        public ThrottleKey(ulong objectId, string command)
        {
            ObjectId = objectId;
            Command = command;
        }

        protected bool Equals(ThrottleKey other)
        {
            return ObjectId == other.ObjectId && Command == other.Command;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ThrottleKey)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ObjectId, Command);
        }
    }

    /// <summary>
    ///     ユーザー単位
    /// </summary>
    private ConcurrentDictionary<ThrottleKey, ThrottleInfo> UserThrottles { get; set; } = new ConcurrentDictionary<ThrottleKey, ThrottleInfo>();

    /// <summary>
    ///     サーバー単位
    /// </summary>
    private ConcurrentDictionary<ThrottleKey, ThrottleInfo> GuildThrottles { get; set; } = new ConcurrentDictionary<ThrottleKey, ThrottleInfo>();

    /// <summary>
    ///     制限リセットまでの残り時間を求める
    /// </summary>
    /// <param name="throttleBy">スロットリング対象</param>
    /// <param name="requestsLimit">許可されたリクエスト数</param>
    /// <param name="intervalSeconds">スロットリング期間(秒)</param>
    /// <param name="user">ユーザーオブジェクト</param>
    /// <param name="guild">サーバーオブジェクト</param>
    /// <param name="command">リクエストされたコマンド名</param>
    /// <returns>リセットまでの残り時間</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [ReassignableVariable("throttles")]
    [ReassignableVariable("throttleObjectId")]
    /// <summary>
    ///     制限リセットまでの残り時間を求める
    /// </summary>
    /// <param name="throttleBy">スロットリング対象</param>
    /// <param name="requestsLimit">許可されたリクエスト数</param>
    /// <param name="intervalSeconds">スロットリング期間(秒)</param>
    /// <param name="user">ユーザーオブジェクト</param>
    /// <param name="guild">サーバーオブジェクト</param>
    /// <param name="command">リクエストされたコマンド名</param>
    /// <returns>リセットまでの残り時間</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public TimeSpan GetThrottleReset(ThrottleBy throttleBy, int requestsLimit, int intervalSeconds, IUser user, IGuild guild, string? command)
    {
        ConcurrentDictionary<ThrottleKey, ThrottleInfo>? throttles = null;
        var interval = TimeSpan.FromSeconds(intervalSeconds);
        ulong throttleObjectId;

        switch (throttleBy)
        {
            case ThrottleBy.User: // "ユーザーごと"
                throttles = UserThrottles;
                throttleObjectId = user.Id;
                break;
            case ThrottleBy.Guild: // "サーバーごと"
                throttles = GuildThrottles;
                throttleObjectId = guild.Id;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(throttleBy), throttleBy, null);
        }

        var throttleKey = new ThrottleKey(throttleObjectId, command!);
        if (throttles.TryGetValue(throttleKey, out var throttleInfo))
        { // 残り時間を取得
            return interval - (DateTime.Now - throttleInfo.FirstRequestTime);
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    ///     リクエストが可能か調べる
    /// </summary>
    /// <param name="throttleBy">スロットリング対象</param>
    /// <param name="requestsLimit">許可されたリクエスト数</param>
    /// <param name="intervalSeconds">スロットリング期間(秒)</param>
    /// <param name="user">ユーザーオブジェクト</param>
    /// <param name="guild">サーバーオブジェクト</param>
    /// <param name="command">リクエストされたコマンド名</param>
    /// <returns>有効: true, 無効: false</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [ReassignableVariable("throttleObjectId")]
    [ReassignableVariable("throttles")]
    /// <summary>
    ///     リクエストが可能か調べる
    /// </summary>
    /// <param name="throttleBy">スロットリング対象</param>
    /// <param name="requestsLimit">許可されたリクエスト数</param>
    /// <param name="intervalSeconds">スロットリング期間(秒)</param>
    /// <param name="user">ユーザーオブジェクト</param>
    /// <param name="guild">サーバーオブジェクト</param>
    /// <param name="command">リクエストされたコマンド名</param>
    /// <returns>有効: true, 無効: false</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public bool CheckThrottle(ThrottleBy throttleBy, int requestLimit, int intervalSeconds, IUser user, IGuild guild, string? command)
    {
        ConcurrentDictionary<ThrottleKey, ThrottleInfo>? throttles = null;
        var interval = TimeSpan.FromSeconds(intervalSeconds);
        ulong throttleObjectId;

        switch (throttleBy)
        {
            case ThrottleBy.User: // "ユーザーごと"
                throttles = UserThrottles;
                throttleObjectId = user.Id;
                break;
            case ThrottleBy.Guild: // "サーバーごと"
                throttles = GuildThrottles;
                throttleObjectId = guild.Id;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(throttleBy), throttleBy, null);
        }

        var throttleKey = new ThrottleKey(throttleObjectId, command!);
        if (throttles.TryGetValue(throttleKey, out var throttleInfo))
        {
            var checkThrottle = ValidateThrottle(requestLimit, throttleInfo, interval, throttles, throttleKey);
            return checkThrottle;
        }
        else
        {
            if (!throttles.TryAdd(throttleKey, new ThrottleInfo() { RequestCount = 1, FirstRequestTime = DateTime.Now }))
            {
                throttleInfo = throttles[throttleKey];
                return ValidateThrottle(requestLimit, throttleInfo, interval, throttles, throttleKey);
            }

            return true;
        }
    }

    /// <summary>
    ///     リクエストの頻度を制御し、許可されたリクエスト数を超えないようにする。
    /// </summary>
    /// <param name="requestsLimit">許可されたリクエスト数</param>
    /// <param name="throttleInfo">スロットリング情報</param>
    /// <param name="interval">スロットリング期間</param>
    /// <param name="throttles">スロットリング情報保持データ</param>
    /// <param name="throttleKey">スロットリング情報を特定するキー</param>
    /// <returns>有効: true / 無効: false</returns>
    private bool ValidateThrottle(int requestsLimit, ThrottleInfo throttleInfo, TimeSpan interval, ConcurrentDictionary<ThrottleKey, ThrottleInfo> throttles, ThrottleKey throttleKey)
    {
        if (DateTime.Now - throttleInfo.FirstRequestTime > interval)
        { // 現在時刻 - 最初に実行した時間 が インターバルより大きい
            if (!throttles.TryUpdate(throttleKey, new ThrottleInfo() { FirstRequestTime = DateTime.Now, RequestCount = 1 }, throttleInfo))
            {
                return ValidateThrottle(requestsLimit, throttles[throttleKey], interval, throttles, throttleKey);
            }

            return true;
        }
        else
        {
            if (throttleInfo.RequestCount + 1 <= requestsLimit)
            {
                if (!throttles.TryUpdate(throttleKey, new ThrottleInfo { FirstRequestTime = throttleInfo.FirstRequestTime, RequestCount = throttleInfo.RequestCount + 1 }, throttleInfo))
                {
                    return ValidateThrottle(requestsLimit, throttles[throttleKey], interval, throttles, throttleKey);
                }

                return true;
            }
        }

        return false;
    }
}
