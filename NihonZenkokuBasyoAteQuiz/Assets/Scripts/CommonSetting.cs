
// シーン間を跨いで共有する設定
public static class CommonSetting
{
    // ミュート ON/OFF
    public static bool IsMute { get; set; }
    // 解答時間[sec]
    public static int MaxTimeSec { get; private set; } = 10;
    // ミスタップ時のマイナス秒
    public static int PenaltySec { get; private set; } = 3;
    // 最大ライフ数
    public static int MaxLife { get; private set; } = 3;
}
