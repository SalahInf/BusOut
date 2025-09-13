using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FunGames.Analytics;
using FunGames.Core.Settings;
using FunGames.Core.Utils;
using FunGames.Tools.Debugging;
using UnityEngine;

namespace FunGames.Core.Modules
{
    /// <summary>
    /// Abstract class for all FunGames modules
    /// </summary>
    /// <typeparam name="M"></typeparam> Module class
    /// <typeparam name="C"></typeparam> Module's callbacks class
    /// <typeparam name="S"></typeparam> Module's settings class
    [DefaultExecutionOrder(-42)]
    public abstract class FGModuleAbstract<M, C, S> : FGSingleton<M>, FGModule
        where M : FGModuleAbstract<M, C, S>
        where C : FGModuleCallbacks, new()
        where S : IFGModuleSettings
    {
        public InitializationMode InitializationMode => _initializationMode;
        public InitializationStatus InitializationStatus => _initializationStatus;
        public FGModuleInfo ModuleInfo => Settings.ModuleInfo;
        public C Callbacks => _callbacks;
        public abstract S Settings { get; }
        protected abstract FGModule Parent { get; }
        public List<FGModule> SubModules => _children;
        protected abstract string EventName { get; }
        protected abstract string RemoteConfigKey { get; }

        protected abstract void InitializeCallbacks();
        protected abstract void OnAwake();
        protected abstract void OnStart();
        protected abstract void InitializeModule();
        protected abstract void ClearInitialization();

        private C _callbacks = new C();
        private readonly List<FGModule> _children = new List<FGModule>();
        private bool _isSelfInitialized = false;
        private float _timer = 0;
        private float _totalInitTime = 0;
        private IEnumerator _checkInitCoroutine;
        private bool _useCheckInit = true;
        private float _maxInitTime = 5;
        private InitializationMode _initializationMode = InitializationMode.DEFAULT;
        private InitializationStatus _initializationStatus = InitializationStatus.NOT_INITIALIZED;

        private readonly Dictionary<FGModule, InitializationStatus> _recursiveChildrenInitializationStatus =
            new Dictionary<FGModule, InitializationStatus>();

        public const string EVENT_INITIALISATION_START = "InitialisationStart";
        public const string EVENT_INITIALISATION_COMPLETE = "InitialisationComplete";
        private const string EVENT_PARAM_VERSION = "version";
        private const string EVENT_PARAM_SUCCESS = "success";
        private const string EVENT_PARAM_INIT_TIME = "initTime";

        public bool IsInitialized
        {
            get
            {
                bool isInitialized = true;
                foreach (var child in _children)
                {
                    isInitialized &= child.IsInitialized;
                }

                isInitialized &= _isSelfInitialized;
                return isInitialized;
            }
        }

        public float TotalSubModules => _recursiveChildrenInitializationStatus.Count;

        public float TotalSubModulesCompleted
        {
            get
            {
                int i = 0;
                foreach (var subModule in _recursiveChildrenInitializationStatus)
                {
                    if (subModule.Value == InitializationStatus.COMPLETED) i++;
                }

                return i;
            }
        }

        public bool IsChildCompleted
        {
            get
            {
                foreach (var child in _children)
                {
                    if (child.InitializationStatus != InitializationStatus.COMPLETED) return false;
                }

                return true;
            }
        }

        private void Awake()
        {
            try
            {
                var instances = FindObjectsOfType(typeof(M));
                if (instances.Length > 1 && !instances[0].Equals(this))
                {
                    Log("Module has already been instantiated.");
                    Destroy(gameObject);
                    return;
                }

                DontDestroyOnLoad(this);

                Log("Version : " + Settings.ModuleInfo.Version);
                Parent?.AddChild(this);
                InitializeCallbacks();
                OnAwake();
            }
            catch (Exception e)
            {
                LogCritical("An exception was raised during Awake :" + e.Message + "\n" + e.StackTrace);
            }
        }

        private void Start()
        {
            try
            {
                if (Settings == null)
                {
                    LogError(EventName + "Settings is null !");
                    return;
                }

                MapRecursiveChildren(this);
                OnStart();
            }
            catch (Exception e)
            {
                LogCritical("An exception was raised during Start :" + e.Message + "\n" + e.StackTrace);
            }
        }

        public void Initialize()
        {
            try
            {
                if (!MustBeInitialized())
                {
                    Clear();
                    return;
                }

                if (IsInitialized)
                {
                    LogWarning("Module already initialized !");
                    return;
                }

                Log("[" + ModuleInfo.Version + "] Initialization started...");
                _initializationStatus = InitializationStatus.IN_PROGRESS;
                _timer = Time.time;

                if (_useCheckInit)
                {
                    _checkInitCoroutine = CheckInitialization();
                    StartCoroutine(_checkInitCoroutine);
                }

                InitializeModule();
                FGAnalytics.NewDesignEvent(FGAnalytics.CreateEventId(EVENT_INITIALISATION_START, EventName),
                    new Dictionary<string, object>()
                    {
                        { EVENT_PARAM_VERSION, Settings.ModuleInfo.Version }
                    });
                Callbacks._onInitialization?.Invoke();
            }
            catch (Exception e)
            {
                LogCritical("An exception was raised during init flow :" + e.Message + "\n" + e.StackTrace);
                InitializationComplete(false);
            }
        }

        public virtual bool MustBeInitialized()
        {
            bool mustBeInitialized = true;
            mustBeInitialized &= _initializationMode != InitializationMode.MANUAL;
            return mustBeInitialized;
        }

        private bool _initAlreadySucceeded = false;
        private bool _initAlreadyFailed = false;

        public void CheckModuleInitialization()
        {
            if (!IsChildCompleted) return;
            if (IsInitialized && _initAlreadySucceeded) return;
            if (!IsInitialized && _initAlreadyFailed) return;

            _totalInitTime = Time.time - _timer;
            if (_checkInitCoroutine != null) StopCoroutine(_checkInitCoroutine);

            if (IsInitialized)
            {
                Log("...initialization succeeded after " + _totalInitTime + " secs !");
                _initAlreadySucceeded = true;
            }
            else
            {
                LogError("...initialization failed after " + _totalInitTime + " secs !");
                _initAlreadyFailed = true;
            }

            _initializationStatus = InitializationStatus.COMPLETED;
            string uniqueId = FGAPIHelpers.CreateUniqueId(DateTime.Now.ToString(), EventName,
                Settings.ModuleInfo.Version,
                IsInitialized.ToString(), _totalInitTime.ToString());
            string eventId = FGAnalytics.CreateEventId(EVENT_INITIALISATION_COMPLETE, EventName, uniqueId);
            FGAnalytics.NewDesignEvent(eventId, new Dictionary<string, object>()
            {
                { EVENT_PARAM_VERSION, Settings.ModuleInfo.Version },
                { EVENT_PARAM_SUCCESS, IsInitialized },
                { EVENT_PARAM_INIT_TIME, _totalInitTime }
            });

            Callbacks._onInitialized?.Invoke(IsInitialized);
            Parent?.CheckModuleInitialization();
        }

        protected void InitializationComplete(bool success)
        {
            if (IsInitialized)
            {
                LogWarning("End of initialization has already been triggered !");
                return;
            }

            _isSelfInitialized = success;
            CheckModuleInitialization();
        }

        public void SetInitializationMode(InitializationMode mode)
        {
            _initializationMode = mode;
        }

        public void AddChild(FGModule child)
        {
            _children.Add(child);
        }

        public void Log(params string[] message)
        {
            if (!Settings.LogEnabled) return;
            if (message.ToString().Contains(EVENT_INITIALISATION_START) ||
                message.ToString().Contains(EVENT_INITIALISATION_COMPLETE)) return;
            FGDebug.Log(FormatLog(message), Settings.Color);
        }

        public void LogError(params string[] message)
        {
            if (!Settings.LogEnabled) return;
            FGDebug.Log(FormatLog(message), Color.red);
        }

        public void LogWarning(params string[] message)
        {
            if (!Settings.LogEnabled) return;
            Debug.LogWarning(FormatLog(message));
        }
        
        public void LogCritical(params string[] message)
        {
            if (!Settings.LogEnabled) return;
            Debug.LogError(FormatLog(message));
        }

        private string FormatLog(string[] messages)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < messages.Length; i++)
            {
                sb.Append(messages[i]);
                if (i < messages.Length - 1) sb.Append("\n");
            }

            return "[FG " + EventName.Trim() + "] " + sb.ToString();
        }

        private IEnumerator CheckInitialization()
        {
            float counter = 0;
            float iteration = 0.2f;
            float maxWaitingDelay = _maxInitTime;
            while (counter <= maxWaitingDelay && !IsInitialized)
            {
                yield return new WaitForSeconds(iteration);
                counter += iteration;
            }

            InitializationComplete(IsInitialized);
        }

        protected void InitWithoutTimer()
        {
            _useCheckInit = false;
        }

        protected void SetMaxInitTime(float time)
        {
            _maxInitTime = time;
        }

        private void MapRecursiveChildren(FGModule module)
        {
            foreach (var child in module.SubModules)
            {
                if (!_recursiveChildrenInitializationStatus.ContainsKey(child))
                {
                    _recursiveChildrenInitializationStatus.Add(child, child.InitializationStatus);
                }

                MapRecursiveChildren(child);
            }
        }

        public void Clear()
        {
            ClearInitialization();
            Callbacks.Clear();
            _isSelfInitialized = false;
        }
    }
}