using FunGames.Core.Utils;
using UnityEngine;

namespace FunGames.Analytics.GA
{
    [CreateAssetMenu(fileName = FGPath.ASSETS_RESOURCES + PATH, menuName = PATH, order = ORDER)]
    public class FGGameAnalyticsSettings : FGModuleSettingsAbstract<FGGameAnalyticsSettings>
    {
        public const string NAME = "FGGameAnalyticsSettings";
        const string PATH = FGPath.FUNGAMES + "/" + NAME;

        protected override FGGameAnalyticsSettings LoadResources()
        {
            return Resources.Load<FGGameAnalyticsSettings>(PATH);
        }

        [Header("GameAnalytics")] [Tooltip("GameAnalytics Ios Game Key")]
        public string gameAnalyticsIosGameKey;

        [Tooltip("GameAnalytics Ios Secret Key")]
        public string gameAnalyticsIosSecretKey;

        [Tooltip("GameAnalytics Android Game Key")]
        public string gameAnalyticsAndroidGameKey;

        [Tooltip("GameAnalytics Android Secret Key")]
        public string gameAnalyticsAndroidSecretKey;
    }
}