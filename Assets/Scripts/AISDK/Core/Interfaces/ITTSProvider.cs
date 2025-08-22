using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AISDK.Core.Models;

namespace AISDK.Core.Interfaces
{
    /// <summary>
    /// Interface for TTS providers
    /// </summary>
    public interface ITTSProvider
    {
        /// <summary>
        /// TTS provider type
        /// </summary>
        TTSProviderType Type { get; }
        
        /// <summary>
        /// Provider name
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Whether the provider is initialized
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Current volume (0-1)
        /// </summary>
        float Volume { get; set; }
        
        /// <summary>
        /// Whether audio is currently playing
        /// </summary>
        bool IsPlaying { get; }
        
        /// <summary>
        /// Initialize the TTS provider
        /// </summary>
        void Initialize(TTSConfig config);
        
        /// <summary>
        /// Set the API key
        /// </summary>
        void SetApiKey(string apiKey);
        
        /// <summary>
        /// Set the voice ID
        /// </summary>
        void SetVoiceId(string voiceId);
        
        /// <summary>
        /// Set the base URL
        /// </summary>
        void SetBaseUrl(string baseUrl);
        
        /// <summary>
        /// Generate speech from text
        /// </summary>
        IEnumerator GenerateSpeech(string text, Action<AudioClip> onComplete);
        
        /// <summary>
        /// Generate speech with custom settings
        /// </summary>
        IEnumerator GenerateSpeech(string text, TTSRequest request, Action<AudioClip> onComplete);
        
        /// <summary>
        /// Play an audio clip
        /// </summary>
        void PlayAudio(AudioClip clip);
        
        /// <summary>
        /// Stop current audio playback
        /// </summary>
        void StopAudio();
        
        /// <summary>
        /// Pause current audio playback
        /// </summary>
        void PauseAudio();
        
        /// <summary>
        /// Resume current audio playback
        /// </summary>
        void ResumeAudio();
        
        /// <summary>
        /// Set audio volume
        /// </summary>
        void SetVolume(float volume);
        
        /// <summary>
        /// Get available voices
        /// </summary>
        IEnumerator GetAvailableVoices(Action<string[]> onComplete);
        
        /// <summary>
        /// Test the TTS provider
        /// </summary>
        IEnumerator TestProvider(Action<bool, string> onComplete);
        
        /// <summary>
        /// Get provider statistics
        /// </summary>
        Dictionary<string, object> GetStatistics();
    }
}
