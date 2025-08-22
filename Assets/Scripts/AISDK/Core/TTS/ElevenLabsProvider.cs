using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UniBuddhi.Core.Interfaces;
using UniBuddhi.Core.Models;

namespace UniBuddhi.Core.TTS
{
    /// <summary>
    /// ElevenLabs TTS provider implementation
    /// </summary>
    public class ElevenLabsProvider : MonoBehaviour, ITTSProvider
    {
        #region Properties
        public TTSProviderType Type => TTSProviderType.ElevenLabs;
        public string Name => "ElevenLabs";
        public bool IsInitialized { get; private set; }
        public float Volume { get; set; } = 1.0f;
        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;
        #endregion

        #region Private Fields
        private TTSConfig _config;
        private AudioSource _audioSource;
        private MonoBehaviour _coroutineRunner;
        private Dictionary<string, object> _statistics = new Dictionary<string, object>();
        #endregion

        #region ITTSProvider Implementation
        public void Initialize(TTSConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                throw new ArgumentException("ElevenLabs API key is required");
            }

            IsInitialized = true;
            InitializeStatistics();
            Debug.Log("[AISDK] ElevenLabs TTS provider initialized");
        }

        public void SetApiKey(string apiKey)
        {
            if (_config != null)
            {
                _config.ApiKey = apiKey;
            }
        }

        public void SetVoiceId(string voiceId)
        {
            if (_config != null)
            {
                _config.VoiceId = voiceId;
            }
        }

        public void SetBaseUrl(string baseUrl)
        {
            if (_config != null)
            {
                _config.BaseUrl = baseUrl;
            }
        }

        public IEnumerator GenerateSpeech(string text, Action<AudioClip> onComplete)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[AISDK] ElevenLabs provider not initialized");
                onComplete?.Invoke(null);
                yield break;
            }

            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("[AISDK] Text is null or empty");
                onComplete?.Invoke(null);
                yield break;
            }

            var request = new TTSRequest(text, _config.VoiceId);
            yield return GenerateSpeech(text, request, onComplete);
        }

        public IEnumerator GenerateSpeech(string text, TTSRequest request, Action<AudioClip> onComplete)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[AISDK] ElevenLabs provider not initialized");
                onComplete?.Invoke(null);
                yield break;
            }

            var startTime = Time.time;
            UnityWebRequest unityRequest = null;
            
            try
            {
                // Prepare request data
                var postData = new
                {
                    text = text,
                    model_id = request.ModelId ?? "eleven_monolingual_v1",
                    voice_settings = new
                    {
                        stability = request.VoiceSettings.Stability,
                        similarity_boost = request.VoiceSettings.SimilarityBoost,
                        style = request.VoiceSettings.Style,
                        use_speaker_boost = request.VoiceSettings.UseSpeakerBoost
                    }
                };

                var json = JsonConvert.SerializeObject(postData);
                var url = $"{_config.BaseUrl}/v1/text-to-speech/{_config.VoiceId}";

                // Create UnityWebRequest
                unityRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                var uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                var downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

                unityRequest.uploadHandler = uploadHandler;
                unityRequest.downloadHandler = downloadHandler;
                unityRequest.SetRequestHeader("Content-Type", "application/json");
                unityRequest.SetRequestHeader("xi-api-key", _config.ApiKey);
                unityRequest.SetRequestHeader("Accept", "audio/mpeg");
                unityRequest.timeout = 30;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AISDK] ElevenLabs TTS setup exception: {ex.Message}");
                _statistics["errors"] = (int)_statistics["errors"] + 1;
                onComplete?.Invoke(null);
                yield break;
            }

            // Send request (outside try-catch)
            yield return unityRequest.SendWebRequest();

            try
            {
                if (unityRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[AISDK] ElevenLabs TTS Error: {unityRequest.responseCode} {unityRequest.error}");
                    _statistics["errors"] = (int)_statistics["errors"] + 1;
                    onComplete?.Invoke(null);
                    yield break;
                }

                // Get audio clip
                var audioClip = DownloadHandlerAudioClip.GetContent(unityRequest);
                if (audioClip != null)
                {
                    var responseTime = Time.time - startTime;
                    _statistics["total_requests"] = (int)_statistics["total_requests"] + 1;
                    _statistics["total_characters"] = (int)_statistics["total_characters"] + text.Length;
                    _statistics["average_response_time"] = responseTime;
                    
                    Debug.Log($"[AISDK] ElevenLabs audio generated successfully! Length: {audioClip.length}s, Response time: {responseTime:F2}s");
                    onComplete?.Invoke(audioClip);
                }
                else
                {
                    Debug.LogError("[AISDK] Failed to generate audio clip from ElevenLabs");
                    _statistics["errors"] = (int)_statistics["errors"] + 1;
                    onComplete?.Invoke(null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AISDK] ElevenLabs TTS processing exception: {ex.Message}");
                _statistics["errors"] = (int)_statistics["errors"] + 1;
                onComplete?.Invoke(null);
            }
            finally
            {
                unityRequest?.Dispose();
            }
        }

        public void PlayAudio(AudioClip clip)
        {
            if (clip == null) return;

            // Find or create AudioSource
            if (_audioSource == null)
            {
                var audioObject = new GameObject("ElevenLabsAudioSource");
                _audioSource = audioObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }

            _audioSource.clip = clip;
            _audioSource.volume = Volume;
            _audioSource.Play();

            // Start coroutine to track playback
            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(WaitForClipToFinish(clip.length));
            }

            Debug.Log($"[AISDK] Playing ElevenLabs audio: {clip.name}");
        }

        public void StopAudio()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
                Debug.Log("[AISDK] ElevenLabs audio stopped");
            }
        }

        public void PauseAudio()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Pause();
                Debug.Log("[AISDK] ElevenLabs audio paused");
            }
        }

        public void ResumeAudio()
        {
            if (_audioSource != null && !_audioSource.isPlaying && _audioSource.clip != null)
            {
                _audioSource.UnPause();
                Debug.Log("[AISDK] ElevenLabs audio resumed");
            }
        }

        public void SetVolume(float volume)
        {
            Volume = Mathf.Clamp01(volume);
            if (_audioSource != null)
            {
                _audioSource.volume = Volume;
            }
        }

        public IEnumerator GetAvailableVoices(Action<string[]> onComplete)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[AISDK] ElevenLabs provider not initialized");
                onComplete?.Invoke(new string[0]);
                yield break;
            }

            var url = $"{_config.BaseUrl}/v1/voices";
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("xi-api-key", _config.ApiKey);
            request.timeout = 15;

            // Send request outside try-catch
            yield return request.SendWebRequest();

            try
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[AISDK] Failed to get voices: {request.error}");
                    onComplete?.Invoke(new string[0]);
                    yield break;
                }

                var response = JsonConvert.DeserializeObject<ElevenLabsVoicesResponse>(request.downloadHandler.text);
                var voiceIds = new List<string>();
                
                if (response.voices != null)
                {
                    foreach (var voice in response.voices)
                    {
                        voiceIds.Add(voice.voice_id);
                    }
                }

                onComplete?.Invoke(voiceIds.ToArray());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AISDK] GetAvailableVoices exception: {ex.Message}");
                onComplete?.Invoke(new string[0]);
            }
            finally
            {
                request?.Dispose();
            }
        }

        public IEnumerator TestProvider(Action<bool, string> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(false, "Provider not initialized");
                yield break;
            }

            // Test with a simple text
            bool testComplete = false;
            AudioClip testClip = null;

            // Test with a simple text
            StartCoroutine(GenerateSpeech("Hello, this is a test.", (clip) =>
            {
                testClip = clip;
                testComplete = true;
            }));

            while (!testComplete)
            {
                yield return null;
            }

            if (testClip != null)
            {
                onComplete?.Invoke(true, "Test successful");
            }
            else
            {
                onComplete?.Invoke(false, "Failed to generate test audio");
            }
        }

        public Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>(_statistics)
            {
                ["provider_type"] = Type.ToString(),
                ["provider_name"] = Name,
                ["is_initialized"] = IsInitialized,
                ["current_volume"] = Volume,
                ["is_playing"] = IsPlaying
            };
            return stats;
        }
        #endregion

        #region Private Methods
        private void InitializeStatistics()
        {
            _statistics["total_requests"] = 0;
            _statistics["total_characters"] = 0;
            _statistics["average_response_time"] = 0.0f;
            _statistics["errors"] = 0;
            _statistics["initialized"] = DateTime.Now;
        }

        private IEnumerator WaitForClipToFinish(float duration)
        {
            yield return new WaitForSeconds(duration);
            Debug.Log("[AISDK] ElevenLabs audio playback finished");
        }
        #endregion

        #region Response Models
        [Serializable]
        private class ElevenLabsVoicesResponse
        {
            [JsonProperty("voices")]
            public ElevenLabsVoice[] voices;
        }

        [Serializable]
        private class ElevenLabsVoice
        {
            [JsonProperty("voice_id")]
            public string voice_id;
            
            [JsonProperty("name")]
            public string name;
            
            [JsonProperty("category")]
            public string category;
        }
        #endregion
    }
}
