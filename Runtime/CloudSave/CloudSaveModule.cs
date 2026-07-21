using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace susaplay.SDK
{
    public class CloudSaveModule
    {
        private HttpClient _httpClient;
        private Dictionary<string, int> _slotVersions = new Dictionary<string, int>();
        private Dictionary<string, float> _lastSaveTime = new Dictionary<string, float>();
        private string _gameId;

        private const int MaxRetries = 3;
        private const float MinSaveIntervalSeconds = 2f;

        public CloudSaveModule(HttpClient httpClient, string gameId)
        {
            _httpClient = httpClient;
            _gameId = gameId;
        }

        public async Task<SaveResult> Save(string slot, string data)
        {
            float now = Time.realtimeSinceStartup;
            if (_lastSaveTime.TryGetValue(slot, out float lastTime))
            {
                float elapsed = now - lastTime;
                if (elapsed < MinSaveIntervalSeconds)
                {
                    int delayMs = (int)((MinSaveIntervalSeconds - elapsed) * 1000);
                    await Task.Delay(delayMs);
                }
            }

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                var version = _slotVersions.ContainsKey(slot) ? _slotVersions[slot] : 0;
                var body = "{\"gameId\":\"" + _gameId + "\",\"slot\":\"" + slot + "\",\"data\":" + data + ",\"version\":" + version + "}";

                var response = await _httpClient.Post("/save/write", body);

                if (response.Success)
                {
                    var envelope = JsonUtility.FromJson<SaveWriteEnvelope>(response.Data);
                    if (envelope == null || !envelope.success || envelope.data == null)
                    {
                        return SaveResult.Fail("Malformed save response");
                    }

                    var saveResult = new SaveResult
                    {
                        Success = true,
                        Version = envelope.data.version,
                        Error = null
                    };
                    _slotVersions[slot] = saveResult.Version;
                    _lastSaveTime[slot] = Time.realtimeSinceStartup;
                    return saveResult;
                }

                if (response.StatusCode == 409 && attempt < MaxRetries - 1)
                {
                    Logger.Warn("Save conflict on slot " + slot + " — re-fetching and retrying (attempt " + (attempt + 1) + ")");
                    var refreshed = await Load(slot);
                    if (!refreshed.Success)
                    {
                        return SaveResult.Fail("Failed to refresh after conflict: " + refreshed.Error);
                    }
                    continue;
                }

                return SaveResult.Fail(response.Error);
            }

            return SaveResult.Fail("Save failed after " + MaxRetries + " retries");
        }

        public async Task<LoadResult> Load(string slot)
        {
            var response = await _httpClient.Get("/save/read?gameId=" + _gameId + "&slot=" + slot);
            if (!response.Success)
            {
                return LoadResult.Fail(response.Error);
            }
            var envelope = JsonUtility.FromJson<SaveReadEnvelope>(response.Data);
            if (envelope == null || !envelope.success || envelope.data == null)
            {
                return LoadResult.Fail("Malformed load response");
            }

            var loadResult = new LoadResult
            {
                Success = true,
                Data = envelope.data.data,
                Version = envelope.data.version,
                Error = null
            };
            _slotVersions[slot] = loadResult.Version;
            return loadResult;
        }
    }

    [System.Serializable]
    class SaveWriteEnvelope
    {
        public bool success;
        public SaveWriteData data;
    }

    [System.Serializable]
    class SaveWriteData
    {
        public string slot;
        public int version;
    }

    [System.Serializable]
    class SaveReadEnvelope
    {
        public bool success;
        public SaveReadData data;
    }

    [System.Serializable]
    class SaveReadData
    {
        public string slot;
        public string data;
        public int version;
        public string savedAt;
    }
}
