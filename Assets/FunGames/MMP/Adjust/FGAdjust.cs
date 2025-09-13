using com.adjust.sdk;
using System;
using FunGames.Analytics;
using FunGames.Core.Modules;

namespace FunGames.MMP.AdjustMMP
{
    public class FGAdjust : FGModuleAbstract<FGAdjust, FGMMPCallbacks, FGAdjustSettings>
    {
        public override FGAdjustSettings Settings => FGAdjustSettings.settings;
        protected override FGModule Parent => FGMMPManager.Instance;
        protected override string EventName => "Adjust";
        protected override string RemoteConfigKey => "FGAdjust";

        private static bool _subscribed = false;

        protected override void InitializeCallbacks()
        {
            FGMMPManager.Instance.Callbacks.Initialization += Initialize;
        }
        
        protected override void OnAwake()
        {
            //
        }

        protected override void OnStart()
        {
           //
        }

        protected override void InitializeModule()
        {
            AdjustEnvironment environment = GetEnvironment();
            Log("Environment : " + environment);

            AdjustConfig adjustConfig = new AdjustConfig(FGAdjustSettings.settings.AppToken.Trim(), environment);
            adjustConfig.setLogLevel(FGAdjustSettings.settings.logLevel);
            adjustConfig.setAttributionChangedDelegate(attributionChangedDelegate);
            adjustConfig.setPreinstallTrackingEnabled(true);
            adjustConfig.setSendInBackground(FGAdjustSettings.settings.sendInBackground);
            Adjust.start(adjustConfig);
            
            InitializationComplete(!String.IsNullOrEmpty(FGAdjustSettings.settings.AppToken));
        }

        
        public void attributionChangedDelegate(AdjustAttribution attribution)
        {
            Log("Attribution changed");
            FGAnalytics.NewDesignEvent("NetworkAttribution:" + attribution.network);
        }

        public static AdjustEnvironment GetEnvironment()
        {
            return AdjustEnvironment.Production;
        }

        protected override void ClearInitialization()
        {
            FGMMPManager.Instance.Callbacks.Initialization -= Initialize;
        }
    }
}