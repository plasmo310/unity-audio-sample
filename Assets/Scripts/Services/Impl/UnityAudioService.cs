using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Services.Impl
{
    /// <summary>
    /// UnityAudioサービスクラス
    /// </summary>
    public class UnityAudioService : IAudioService
    {
        /// <summary>
        /// AudioManagerオブジェクト名
        /// </summary>
        private static readonly string AudioManagerObjectName = "AudioManager";

        #region AudioMixer

        /// <summary>
        /// AudioMixerGroup関連
        /// </summary>
        private static class AudioMixerConst
        {
            // AudioMixer名
            public static readonly string MainAudioMixerName = "MainAudioMixer";
            public static readonly string EffectBgmAudioMixerName = "EffectBgmAudioMixer";
            public static readonly string EffectSeAudioMixerName = "EffectSeAudioMixer";

            // AudioMixerGroup名
            public static readonly string MainMasterGroupName = "Master";
            public static readonly string MainBgmGroupName = "BGM";
            public static readonly string MainSeGroupName = "SE";
            public static readonly string EffectBgmMasterGroupName = "Master";
            public static readonly string EffectSeMasterGroupName = "Master";

            // Exposeしたパラメータ名
            public static readonly string MasterVolumeParamName = "MasterVolume";
            public static readonly string BgmVolumeParamName = "BGMVolume";
            public static readonly string SeVolumeParamName = "SeVolume";
        }

        /// <summary>
        /// AudioMixer
        /// </summary>
        private readonly AudioMixer _mainAudioMixer;
        private readonly AudioMixer _effectBgmAudioMixer;
        private readonly AudioMixer _effectSeAudioMixer;
        private readonly AudioMixerGroup _mainAudioMixerGroupMaster;
        private readonly AudioMixerGroup _mainAudioMixerGroupBgm;
        private readonly AudioMixerGroup _mainAudioMixerGroupSe;
        private readonly AudioMixerGroup _effectBgmAudioMixerGroupMaster;
        private readonly AudioMixerGroup _effectSeAudioMixerGroupMaster;

        /// <summary>
        /// マスターボリューム
        /// </summary>
        public float MasterVolume
        {
            get => GetAudioMixerVolume(_mainAudioMixer, AudioMixerConst.MasterVolumeParamName);
            set => SetAudioMixerVolume(_mainAudioMixer, AudioMixerConst.MasterVolumeParamName, value );
        }

        /// <summary>
        /// BGMボリューム
        /// </summary>
        public float BgmVolume
        {
            get => GetAudioMixerVolume(_mainAudioMixer, AudioMixerConst.BgmVolumeParamName);
            set => SetAudioMixerVolume(_mainAudioMixer, AudioMixerConst.BgmVolumeParamName, value );
        }

        /// <summary>
        /// SEボリューム
        /// </summary>
        public float SeVolume
        {
            get => GetAudioMixerVolume(_mainAudioMixer, AudioMixerConst.SeVolumeParamName);
            set => SetAudioMixerVolume(_mainAudioMixer, AudioMixerConst.SeVolumeParamName, value );
        }

        private static float GetAudioMixerVolume(AudioMixer audioMixer, string exposedParamName)
        {
            audioMixer.GetFloat(exposedParamName, out var decibel);
            if (decibel <= -96f)
            {
                return 0f;
            }
            return Mathf.Pow(10f, decibel / 20f);
        }

        private static void SetAudioMixerVolume(AudioMixer audioMixer, string exposedParamName, float volume)
        {
            var decibel = 20f * Mathf.Log10(volume);
            if (float.IsNegativeInfinity(decibel))
            {
                decibel = -96f;
            }
            audioMixer.SetFloat(exposedParamName, decibel);
        }

        /// <summary>
        /// BGMエフェクト変更
        /// </summary>
        /// <param name="snapshotName"></param>
        /// <param name="fadeTime"></param>
        public void ChangeBgmEffectSnapshot(string snapshotName, float fadeTime)
        {
            if (_effectBgmAudioMixer == null)
            {
                return;
            }

            var snapshot = _effectBgmAudioMixer.FindSnapshot(snapshotName);
            if (snapshot != null)
            {
                snapshot.TransitionTo(fadeTime);
            }
        }

        /// <summary>
        /// SEエフェクト変更
        /// </summary>
        /// <param name="snapshotName"></param>
        /// <param name="fadeTime"></param>
        public void ChangeSeEffectSnapshot(string snapshotName, float fadeTime)
        {
            if (_effectSeAudioMixer == null)
            {
                return;
            }

            var snapshot = _effectSeAudioMixer.FindSnapshot(snapshotName);
            if (snapshot != null)
            {
                snapshot.TransitionTo(fadeTime);
            }
        }

        #endregion

        /// <summary>
        /// AudioSource
        /// </summary>
        private readonly AudioSource _seAudioSource; // SE再生用
        private readonly List<AudioSource> _bgmAudioSourceList; // BGM再生用

        /// <summary>
        /// 一時停止中か？
        /// </summary>
        private bool _isPause = false;

        public UnityAudioService()
        {
            // AudioMixer、AudioGroup読込
            _mainAudioMixer = LoadAudioMixer(AudioMixerConst.MainAudioMixerName);
            if (_mainAudioMixer != null)
            {
                _mainAudioMixerGroupMaster = _mainAudioMixer.FindMatchingGroups(AudioMixerConst.MainMasterGroupName)[0];
                _mainAudioMixerGroupBgm = _mainAudioMixer.FindMatchingGroups(AudioMixerConst.MainBgmGroupName)[0];
                _mainAudioMixerGroupSe = _mainAudioMixer.FindMatchingGroups(AudioMixerConst.MainSeGroupName)[0];

                _effectBgmAudioMixer = LoadAudioMixer(AudioMixerConst.EffectBgmAudioMixerName);
                if (_effectBgmAudioMixer != null)
                {
                    _effectBgmAudioMixerGroupMaster = _effectBgmAudioMixer.FindMatchingGroups(AudioMixerConst.EffectBgmMasterGroupName)[0];
                }
                _effectSeAudioMixer = LoadAudioMixer(AudioMixerConst.EffectSeAudioMixerName);
                if (_effectSeAudioMixer != null)
                {
                    _effectSeAudioMixerGroupMaster = _effectSeAudioMixer.FindMatchingGroups(AudioMixerConst.EffectSeMasterGroupName)[0];
                }
            }

            // AudioManagerオブジェクトを作成
            var audioManager = GameObject.Find(AudioManagerObjectName);
            if (audioManager == null)
            {
                audioManager = new GameObject(AudioManagerObjectName);
                Object.DontDestroyOnLoad(audioManager);
                _seAudioSource = CreateSeAudioSource(audioManager);

                // BGMはフェード用で2つ生成しておく
                _bgmAudioSourceList = new List<AudioSource>();
                _bgmAudioSourceList.Add(CreateBgmAudioSource(audioManager));
                _bgmAudioSourceList.Add(CreateBgmAudioSource(audioManager));
            }
            _isPause = false;
        }

        /// <summary>
        /// BGM用AudioSource生成
        /// </summary>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        public AudioSource CreateBgmAudioSource(GameObject parentObject)
        {
            return CreateAudioSource(parentObject, true, _effectBgmAudioMixerGroupMaster != null ? _effectBgmAudioMixerGroupMaster : _mainAudioMixerGroupBgm);
        }

        /// <summary>
        /// SE用AudioSource生成
        /// </summary>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        public AudioSource CreateSeAudioSource(GameObject parentObject)
        {
            return CreateAudioSource(parentObject, false, _effectSeAudioMixerGroupMaster != null ? _effectSeAudioMixerGroupMaster : _mainAudioMixerGroupSe);
        }

        /// <summary>
        /// AudioSource生成
        /// </summary>
        /// <param name="parentObject"></param>
        /// <param name="isLoop"></param>
        /// <param name="audioMixerGroup"></param>
        /// <returns></returns>
        private AudioSource CreateAudioSource(GameObject parentObject, bool isLoop, AudioMixerGroup audioMixerGroup)
        {
            var audioSource = parentObject.AddComponent<AudioSource>();
            audioSource.loop = isLoop;
            audioSource.playOnAwake = false; // デフォルトでtrueのため明示的にオフにする
            if (audioMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = audioMixerGroup;
            }
            return audioSource;
        }

        // TODO 厳密にはフェード中のCoroutineも停止対象に含めなければならない

        /// <summary>
        /// 一時停止
        /// </summary>
        public void Pause()
        {
            if (_isPause)
            {
                return;
            }
            _isPause = true;

            _seAudioSource.Pause();
            _bgmAudioSourceList.ForEach(audioSource => audioSource.Pause());
        }

        /// <summary>
        /// 一時停止解除
        /// </summary>
        public void Resume()
        {
            if (!_isPause)
            {
                return;
            }
            _isPause = false;

            _seAudioSource.UnPause();
            _bgmAudioSourceList.ForEach(audioSource => audioSource.UnPause());
        }

        /// <summary>
        /// 効果音再生
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="option"></param>
        public void PlayOneShot(string audioName, IAudioService.AudioOption option = null)
        {
            PlayOneShot(_seAudioSource, audioName, option);
        }

        /// <summary>
        /// 効果音再生
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="audioName"></param>
        /// <param name="option"></param>
        public void PlayOneShot(AudioSource audioSource, string audioName, IAudioService.AudioOption option = null)
        {
            var audioClip = LoadAudioClip(audioName);
            audioSource.volume = option?.Volume ?? 1f;
            audioSource.pitch = option?.Pitch ?? 1f;
            audioSource.PlayOneShot(audioClip);
        }

        /// <summary>
        /// BGM再生
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="option"></param>
        public void PlayBgm(string audioName, IAudioService.AudioOption option = null)
        {
            StopAllBgm();

            var audioSource = _bgmAudioSourceList[0];
            audioSource.clip = LoadAudioClip(audioName);
            audioSource.volume = option?.Volume ?? 1f;
            audioSource.pitch = option?.Pitch ?? 1f;
            audioSource.Play();
        }

        /// <summary>
        /// BGMフェードイン再生
        /// </summary>
        public void PlayBgmFadeIn(MonoBehaviour mono, string audioName, float fadeTime, IAudioService.AudioOption option = null)
        {
            var audioSource = _bgmAudioSourceList.FirstOrDefault(audioSource => !audioSource.isPlaying);
            if (audioSource == null)
            {
                Debug.LogError($"all play audio sources!!");
                return;
            }

            StopAllBgmFadeOut(mono, fadeTime);
            mono.StartCoroutine(PlayBgmFadeInCoroutine(audioSource, audioName, fadeTime, option));
        }

        private IEnumerator PlayBgmFadeInCoroutine(AudioSource audioSource, string audioName, float fadeTime, IAudioService.AudioOption option = null)
        {
            var startVolume = 0f;
            var targetVolume = option?.Volume ?? 1f;

            audioSource.clip = LoadAudioClip(audioName);
            audioSource.volume = startVolume;
            audioSource.pitch = option?.Pitch ?? 1f;
            audioSource.Play();

            for (var t = 0f; t < fadeTime; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, Mathf.Clamp01(t / fadeTime));
                yield return null;
            }
            audioSource.volume = targetVolume;
        }

        /// <summary>
        /// BGM停止
        /// </summary>
        public void StopAllBgm()
        {
            // 再生中のBGMを全て取得して停止する
            var audioSources = _bgmAudioSourceList.Where(audioSource => audioSource.isPlaying);
            if (audioSources.Count() <= 0)
            {
                return;
            }

            foreach (var audioSource in audioSources)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }

        /// <summary>
        /// BGMフェードアウト停止
        /// </summary>
        public void StopAllBgmFadeOut(MonoBehaviour mono, float fadeTime)
        {
            // 再生中のBGMを全て取得して停止する
            var audioSources = _bgmAudioSourceList.Where(audioSource => audioSource.isPlaying);
            if (audioSources.Count() <= 0)
            {
                return;
            }

            foreach (var audioSource in audioSources)
            {
                mono.StartCoroutine(StopBgmFadeOutCoroutine(audioSource, fadeTime));
            }
        }

        private IEnumerator StopBgmFadeOutCoroutine(AudioSource audioSource, float fadeTime)
        {
            var startVolume = audioSource.volume;
            var targetVolume = 0f;
            for (var t = 0f; t < fadeTime; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, Mathf.Clamp01(t / fadeTime));
                yield return null;
            }
            audioSource.volume = targetVolume;

            audioSource.Stop();
            audioSource.clip = null;
        }

        /// <summary>
        /// 再生中BGMのスペクトラム周波数を取得
        /// </summary>
        public float[] GetBgmSpectrumData(int sampleCount)
        {
            var spectrumData = new float[sampleCount];
            var bgmAudioResource = _bgmAudioSourceList.FirstOrDefault(audioSource => audioSource.isPlaying);
            if (bgmAudioResource == null)
            {
                return spectrumData;
            }
            bgmAudioResource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
            return spectrumData;
        }

        #region ファイル読込処理

        // とりあえずResources配下からの読込にしている.

        /// <summary>
        /// Audioファイル格納パス
        /// </summary>
        private static readonly string AudioFileDirPath = "Audio/";
        private static readonly string AudioMixerFileDirPath = "Audio/Mixer/";

        /// <summary>
        /// キャッシュしたAudioClip
        /// key: ファイル名
        /// </summary>
        private readonly IDictionary<string, AudioClip> _cachedAudioDictionary = new Dictionary<string, AudioClip>();

        /// <summary>
        /// AudioClipの読み込み
        /// </summary>
        /// <param name="audioName"></param>
        /// <returns></returns>
        private AudioClip LoadAudioClip(string audioName)
        {
            // ファイル名をキーとしてキャッシュする
            if (!_cachedAudioDictionary.ContainsKey(audioName))
            {
                var audioClip = Resources.Load(AudioFileDirPath + audioName) as AudioClip;
                _cachedAudioDictionary.Add(audioName, audioClip);
            }
            return _cachedAudioDictionary[audioName];
        }

        /// <summary>
        /// AudioMixerの読み込み
        /// </summary>
        /// <returns></returns>
        private AudioMixer LoadAudioMixer(string audioMixerName)
        {
            return Resources.Load(AudioMixerFileDirPath + audioMixerName) as AudioMixer;
        }

        #endregion

    }
}