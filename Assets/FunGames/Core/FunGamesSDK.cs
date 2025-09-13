namespace FunGames.Core
{
    public class FunGamesSDK
    {
        public static FGCoreCallbacks Callbacks => FGCore.Instance.Callbacks;

        public static bool IsInitialized => FGCore.Instance.IsInitialized;
        public static void RemoveAds() => FGCore.Instance.RemoveAds();
        public static bool IsNoAds => FGCore.Instance.IsNoAd();
        public static bool IsFirstConnection => FGCore.Instance.IsFirstConnection();
        public static int DaysSinceFirstCo => FGCore.Instance.DaysSinceFirstConnection();
        public static int DaysSinceLastCo => FGCore.Instance.DaysSinceLastConnection();
        public static int CurrentSessionNumber => FGCore.Instance.GetCurrentSessionNumber();
    }
}