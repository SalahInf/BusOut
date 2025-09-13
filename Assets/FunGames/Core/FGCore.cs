using System;
using System.Globalization;
using FunGames.Core.Modules;
using FunGames.Tools.Utils;
using UnityEngine;

namespace FunGames.Core
{
    public class FGCore : FGModuleAbstract<FGCore, FGCoreCallbacks, FGMainSettings>
    {
        public override FGMainSettings Settings => FGMainSettings.settings;
        protected override FGModule Parent { get; }
        protected override string EventName => "SDK";
        protected override string RemoteConfigKey => "FunGamesSDK";

        private int _moduleCount = 0;

        private const string PP_NO_ADS = "noAds";
        private const string PP_DATE_FIRST_CO = "dateFirstCo";
        private const string PP_DATE_LAST_CO = "dateLastCo";
        private const string PP_CURRENT_SESSION_NUMBER = "CurrentSessionNumber";
        private bool _isNoAds = false;
        private bool _isFirstConnection = false;
        private int _daysSinceFirstConnection = 0;
        private int _daysSinceLastConnection = 0;
        private int _currentSessionNumber = 0;

        protected override void InitializeCallbacks()
        {
            InitWithoutTimer();
        }

        protected override void OnAwake()
        {
            _isNoAds = PlayerPrefs.HasKey(PP_NO_ADS);
        
            if (!PlayerPrefs.HasKey(PP_CURRENT_SESSION_NUMBER))
            {
                PlayerPrefs.SetInt(PP_CURRENT_SESSION_NUMBER, 1);
            }
            else
            {
                int currentSession = PlayerPrefs.GetInt(PP_CURRENT_SESSION_NUMBER) + 1;
                PlayerPrefs.SetInt(PP_CURRENT_SESSION_NUMBER, currentSession);
            }
            _currentSessionNumber = PlayerPrefs.GetInt(PP_CURRENT_SESSION_NUMBER);
        }

        protected override void OnStart()
        {
            Initialize();
        }

        protected override void InitializeModule()
        {
            DateTime today = DateTime.Now;
            DateTime dateFirstCo;
            DateTime dateLastCo;

            _isFirstConnection = !PlayerPrefs.HasKey(PP_DATE_FIRST_CO);
            if (_isFirstConnection)
            {
                string dateAsString = today.ToString(CultureInfo.InvariantCulture);
                PlayerPrefs.SetString(PP_DATE_FIRST_CO, dateAsString);
                Log("Save Date First Connection : " + dateAsString);
                dateFirstCo = today;
                dateLastCo = today;
            }
            else
            {
                dateFirstCo = DateUtils.ConvertInvariant(PlayerPrefs.GetString(PP_DATE_FIRST_CO));
                dateLastCo = DateUtils.ConvertInvariant(PlayerPrefs.GetString(PP_DATE_LAST_CO));
            }

            _daysSinceFirstConnection = (int)today.Subtract(dateFirstCo).TotalDays;
            _daysSinceLastConnection = (int)today.Subtract(dateLastCo).TotalDays;

            PlayerPrefs.SetString(PP_DATE_LAST_CO, today.ToString(CultureInfo.InvariantCulture));
            Log("Day since first connection : " + _daysSinceFirstConnection);
            Log("Day since last connection : " + _daysSinceLastConnection);
            Log("Session #: " + _currentSessionNumber);
            InitializationComplete(true);
        }

        public void RemoveAds()
        {
            Log("Ads removed !");
            PlayerPrefs.SetInt(PP_NO_ADS, 1);
            _isNoAds = true;
        }

        public bool IsNoAd()
        {
            return _isNoAds;
        }


        public bool IsFirstConnection()
        {
            return _isFirstConnection;
        }

        public int DaysSinceFirstConnection()
        {
            return _daysSinceFirstConnection;
        }

        public int DaysSinceLastConnection()
        {
            return _daysSinceLastConnection;
        }
        
        public int GetCurrentSessionNumber()
        {
            return _currentSessionNumber;
        }

        protected override void ClearInitialization()
        {
            // Nothing to do
        }
    }
}