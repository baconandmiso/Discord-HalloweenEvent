namespace Modules.Event;

/// <summary>
///     得点の管理をするオブジェクトです。
/// </summary>
public class Point
{
    /// <summary>
    ///     ユーザーID
    /// </summary>
    public ulong UserId { get; }

    /// <summary>
    ///     得点
    /// </summary>
    public int Score { get; }

    /// <summary>
    ///     オブジェクトを初期化します。
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="score">得点</param>
    /// <exception cref="ArgumentException"></exception>
    public Point(ulong userId, int score)
    {
        if (score < 0)
        {
            throw new ArgumentException("得点は0以上である必要があります。", nameof(score));
        }

        UserId = userId;
        Score = score;
    }

    /// <summary>
    ///     加点します。
    /// </summary>
    /// <param name="point">加算する得点を持つPointオブジェクト。</param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentException"></exception>"
    public Point Add(Point point)
    {
        if (point == null)
        {
            throw new NullReferenceException("オブジェクトがnullです。");
        }

        if (point.UserId != UserId)
        {
            throw new ArgumentException("ユーザーIDが一致しません。", nameof(point));
        }

        if (point.Score < 0)
        {
            throw new ArgumentException("得点は0以上である必要があります。", nameof(point));
        }

        var newPoint = Score + point.Score;
        return new Point(UserId, newPoint);
    }

    /// <summary>
    ///    減点します。
    /// </summary>
    /// <param name="point">減算する得点を持つPointオブジェクト。</param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public Point Sub(Point point)
    {
        if (point == null)
        {
            throw new NullReferenceException("オブジェクトがnullです。");
        }

        if (point.UserId != UserId)
        {
            throw new ArgumentException("ユーザーIDが一致しません。", nameof(point));
        }

        if (point.Score < 0)
        {
            throw new ArgumentException("得点は0以上である必要があります。", nameof(point));
        }

        var newPoint = Score - point.Score;
        if (newPoint < 0) // 0未満にならないようにする
        {
            return new Point(UserId, 0);
        }

        return new Point(UserId, newPoint);
    }
}

