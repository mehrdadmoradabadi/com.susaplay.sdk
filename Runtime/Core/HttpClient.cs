using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace susaplay.SDK
{
    public class HttpClient
    {
        private SDKConfig _config;
        private TokenManager _tokenManager;
        private AppCheckManager _appCheckManager;

        public HttpClient(SDKConfig config, TokenManager tokenManager, AppCheckManager appCheckManager = null)
        {
            _config = config;
            _tokenManager = tokenManager;
            _appCheckManager = appCheckManager;
        }

        public async Task<HttpResponse> Post(string endpoint, string body)
        {
            var token = await _tokenManager.GetTokenAsync();
            var url = _config.ApiBaseUrl + endpoint;
            var request = new UnityWebRequest(url, "POST");
            var bodyBytes = System.Text.Encoding.UTF8.GetBytes(body);
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + token);
            await AttachAppCheckHeader(request);
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                return new HttpResponse { Success = true, Data = request.downloadHandler.text, StatusCode = request.responseCode };
            }
            else
            {
                return HttpResponse.Fail(request.error, request.responseCode);
            }
        }

        public async Task<HttpResponse> Get(string endpoint)
        {
            var token = await _tokenManager.GetTokenAsync();
            var url = _config.ApiBaseUrl + endpoint;
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", "Bearer " + token);
            await AttachAppCheckHeader(request);
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                return new HttpResponse { Success = true, Data = request.downloadHandler.text, StatusCode = request.responseCode };
            }
            else
            {
                return HttpResponse.Fail(request.error, request.responseCode);
            }
        }

        private async Task AttachAppCheckHeader(UnityWebRequest request)
        {
            if (_appCheckManager == null) return;
            var appCheckToken = await _appCheckManager.GetTokenAsync();
            if (!string.IsNullOrEmpty(appCheckToken))
                request.SetRequestHeader("X-Firebase-AppCheck", appCheckToken);
        }
    }
}