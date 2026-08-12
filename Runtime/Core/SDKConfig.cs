using UnityEngine;
namespace susaplay.SDK
{
    public class SDKConfig : ScriptableObject
    {
        private const string LiveUrl = "https://europe-west1-susaplay-2e8e3.cloudfunctions.net";
        private const string EmulatorUrl = "http://localhost:5001";
        private const float MinimumAnalyticsFlushIntervalSeconds = 10f;
        [SerializeField] private string _gameKey;
        [SerializeField] private bool _isEmulatorMode;
        [SerializeField] private string _liveUrlOverride;
        [SerializeField] private string _liveOpsContentBaseUrl;
        [SerializeField] private bool _automaticAnalyticsFlushEnabled = true;
        [SerializeField] private float _analyticsFlushIntervalSeconds = 300f;
        [SerializeField] private bool _flushAnalyticsOnInitialize = true;
        [SerializeField] private bool _flushAnalyticsOnPause = true;
        [SerializeField] private bool _flushAnalyticsOnQuit = true;
        // Trimmed: the key is pasted into the inspector field, and a trailing space would be
        // sent verbatim in SDK_INIT and fail player-init with GAME_NOT_FOUND.
        public string GameKey => string.IsNullOrWhiteSpace(_gameKey) ? string.Empty : _gameKey.Trim();
        public bool AutomaticAnalyticsFlushEnabled => _automaticAnalyticsFlushEnabled;
        public float AnalyticsFlushIntervalSeconds => Mathf.Max(MinimumAnalyticsFlushIntervalSeconds, _analyticsFlushIntervalSeconds);
        public bool FlushAnalyticsOnInitialize => _flushAnalyticsOnInitialize;
        public bool FlushAnalyticsOnPause => _flushAnalyticsOnPause;
        public bool FlushAnalyticsOnQuit => _flushAnalyticsOnQuit;
        public string LiveOpsContentBaseUrl => string.IsNullOrWhiteSpace(_liveOpsContentBaseUrl)
            ? string.Empty
            : _liveOpsContentBaseUrl.TrimEnd('/');

        public string ApiBaseUrl
        {
            get
            {
                if (!_isEmulatorMode && !string.IsNullOrWhiteSpace(_liveUrlOverride))
                {
                    return _liveUrlOverride.TrimEnd('/');
                }
                return _isEmulatorMode ? EmulatorUrl : LiveUrl;
            }
        }

        public static SDKConfig Load()
        {
            return Resources.Load<SDKConfig>("PlatformConfig");
        }
        public void SetGameKey(string key)
        {
            _gameKey = key;
        }

        public void SetLiveOpsContentBaseUrl(string url)
        {
            _liveOpsContentBaseUrl = string.IsNullOrWhiteSpace(url) ? string.Empty : url.TrimEnd('/');
        }

        public void SetAutomaticAnalyticsFlushEnabled(bool enabled)
        {
            _automaticAnalyticsFlushEnabled = enabled;
        }

        public void SetAnalyticsFlushIntervalSeconds(float seconds)
        {
            _analyticsFlushIntervalSeconds = Mathf.Max(MinimumAnalyticsFlushIntervalSeconds, seconds);
        }

        public void SetAnalyticsLifecycleFlushes(bool onInitialize, bool onPause, bool onQuit)
        {
            _flushAnalyticsOnInitialize = onInitialize;
            _flushAnalyticsOnPause = onPause;
            _flushAnalyticsOnQuit = onQuit;
        }
    }
}
