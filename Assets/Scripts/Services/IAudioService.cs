using UnityEngine;

namespace Services
{
    public interface IAudioService
    {
        /// <summary>
        /// オプション
        /// </summary>
        public class AudioOption
        {
            /// <summary>
            /// ボリューム
            /// </summary>
            public float Volume = 1f;

            /// <summary>
            /// ピッチ
            /// </summary>
            public float Pitch = 1f;

            public AudioOption(float volume, float pitch)
            {
                Volume = volume;
                Pitch = pitch;
            }
        }

        /// <summary>
        /// マスターボリューム
        /// </summary>
        public float MasterVolume { get; set; }

        /// <summary>
        /// BGMボリューム
        /// </summary>
        public float BgmVolume { get; set; }

        /// <summary>
        /// SEボリューム
        /// </summary>
        public float SeVolume { get; set; }

        /// <summary>
        /// BGMエフェクト変更
        /// </summary>
        public void ChangeBgmEffectSnapshot(string snapshotName, float fadeTime);

        /// <summary>
        /// SEエフェクト変更
        /// </summary>
        public void ChangeSeEffectSnapshot(string snapshotName, float fadeTime);

        /// <summary>
        /// BGM用AudioSource生成
        /// </summary>
        public AudioSource CreateBgmAudioSource(GameObject parentObject);

        /// <summary>
        /// SE用AudioSource生成
        /// </summary>
        public AudioSource CreateSeAudioSource(GameObject parentObject);

        /// <summary>
        /// 一時停止
        /// </summary>
        public void Pause();

        /// <summary>
        /// 一時停止解除
        /// </summary>
        public void Resume();

        /// <summary>
        /// 効果音再生
        /// </summary>
        public void PlayOneShot(string audioName, AudioOption option = null);

        /// <summary>
        /// 効果音再生
        /// </summary>
        public void PlayOneShot(AudioSource audioSource, string audioName, IAudioService.AudioOption option = null);

        /// <summary>
        /// BGM再生
        /// </summary>
        public void PlayBgm(string audioName, AudioOption option = null);

        /// <summary>
        /// BGMフェードイン再生
        /// </summary>
        public void PlayBgmFadeIn(MonoBehaviour mono, string audioName, float fadeTime, IAudioService.AudioOption option = null);

        /// <summary>
        /// BGM停止
        /// </summary>
        public void StopAllBgm();

        /// <summary>
        /// BGMフェードアウト停止
        /// </summary>
        public void StopAllBgmFadeOut(MonoBehaviour mono, float fadeTime);

        /// <summary>
        /// 再生中BGMのスペクトラム周波数を取得
        /// </summary>
        public float[] GetBgmSpectrumData(int sampleCount);
    }
}
