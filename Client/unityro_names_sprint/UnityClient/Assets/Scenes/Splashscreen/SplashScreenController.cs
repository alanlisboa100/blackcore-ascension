using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ROIO;
using ROIO.Utils;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreenController : MonoBehaviour {

    [SerializeField]
    private AssetLabelReference[] LabelsToPrefetch;

    [SerializeField]
    private TextMeshProUGUI labelText;

    [SerializeField]
    private TextMeshProUGUI DownloadSizeText;

    [SerializeField]
    private Slider Slider;

    void Start() {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize() {
        labelText.text = $"{BlackCoreBrand.GameName}\nChecking for updates...";
        yield return Addressables.InitializeAsync();
        yield return new WaitForSeconds(1f);

        StartCoroutine(PrefetchAssets());
    }

    private IEnumerator PrefetchAssets() {
#if !UNITY_EDITOR
        var downloadSizeHandle = Addressables.GetDownloadSizeAsync(LabelsToPrefetch);
        yield return downloadSizeHandle;
        var downloadSize = downloadSizeHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded
            ? downloadSizeHandle.Result
            : 0L;

        if (downloadSize <= 0) {
            yield return FetchConfigs();
            yield break;
        }

        foreach (var label in LabelsToPrefetch) {
            var handle = Addressables.DownloadDependenciesAsync(label, true);

            while(!handle.IsDone) {
                var downloadStatus = handle.GetDownloadStatus();
                var downloadedMbs = downloadStatus.DownloadedBytes / 1024f / 1024f;
                var totalMbs = (downloadStatus.TotalBytes / 1024f / 1024f);

                var progress = Conversions.SafeDivide(downloadedMbs, totalMbs);

                var text = $"{BlackCoreBrand.ShortName} • Downloading {label.labelString}";
                labelText.text = text;
                DownloadSizeText.text = $"{downloadedMbs}MB / {totalMbs}MB";
                Slider.value = progress;

                yield return null;
            }

            yield return handle;
        }
#endif
        yield return FetchConfigs();
    }

    private IEnumerator FetchConfigs() {
        labelText.text = "Carregando configuração do Black Core...";
        var localRequest = Addressables.LoadAssetAsync<TextAsset>("LocalConfigs.json.txt");
        yield return localRequest;

        var localConfig = JObject.Parse(localRequest.Result.text);
        var localConfiguration = JsonConvert.DeserializeObject<LocalConfiguration>(localConfig.ToString());

        const string cachedRemoteConfigKey = "blackcore.remote_config_cache";
        string remoteConfigJson = null;

        if (!string.IsNullOrWhiteSpace(localConfiguration.remoteConfigLocation)) {
            for (int attempt = 1; attempt <= 3 && string.IsNullOrEmpty(remoteConfigJson); attempt++) {
                labelText.text = attempt == 1
                    ? "Buscando servidor Black Core..."
                    : $"Tentando novamente... {attempt}/3";

                using (var remoteRequest = UnityWebRequest.Get(localConfiguration.remoteConfigLocation)) {
                    remoteRequest.timeout = 10;
                    yield return remoteRequest.SendWebRequest();

                    if (remoteRequest.result == UnityWebRequest.Result.Success && !string.IsNullOrWhiteSpace(remoteRequest.downloadHandler.text)) {
                        remoteConfigJson = remoteRequest.downloadHandler.text;
                        PlayerPrefs.SetString(cachedRemoteConfigKey, remoteConfigJson);
                        PlayerPrefs.Save();
                    }
                }

                if (string.IsNullOrEmpty(remoteConfigJson) && attempt < 3) {
                    yield return new WaitForSecondsRealtime(attempt);
                }
            }
        }

        if (string.IsNullOrEmpty(remoteConfigJson) && PlayerPrefs.HasKey(cachedRemoteConfigKey)) {
            labelText.text = "Usando configuração salva...";
            remoteConfigJson = PlayerPrefs.GetString(cachedRemoteConfigKey);
        }

        RemoteConfiguration remoteConfiguration = null;
        if (!string.IsNullOrEmpty(remoteConfigJson)) {
            try {
                var remoteConfig = JObject.Parse(remoteConfigJson);
                remoteConfiguration = JsonConvert.DeserializeObject<RemoteConfiguration>(remoteConfig.ToString());
            } catch (System.Exception ex) {
                Debug.LogWarning($"Invalid cached/remote Black Core configuration: {ex.Message}");
            }
        }

        // Bundled fallback makes development independent from the old UnityRO config host.
        if (remoteConfiguration == null && !string.IsNullOrWhiteSpace(localConfiguration.fallbackLoginServer)) {
            labelText.text = "Usando servidor configurado no aplicativo...";
            remoteConfiguration = new RemoteConfiguration {
                loginServer = localConfiguration.fallbackLoginServer,
                loginPort = string.IsNullOrWhiteSpace(localConfiguration.fallbackLoginPort) ? "6900" : localConfiguration.fallbackLoginPort,
                useSameIpForEveryServer = localConfiguration.fallbackUseSameIpForEveryServer
            };
        }

        if (remoteConfiguration == null || string.IsNullOrWhiteSpace(remoteConfiguration.loginServer)) {
            labelText.text = "Servidor Black Core não configurado.";
            yield break;
        }

        FindObjectOfType<GameManager>().SetConfigurations(remoteConfiguration, localConfiguration);
        SceneManager.LoadScene("LoginScene");
    }
}
