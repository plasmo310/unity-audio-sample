using System.Collections.Generic;
using Characters;
using Common;
using Services;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    /// <summary>
    /// SoundTestScene管理クラス
    /// </summary>
    public class SoundTestSceneManager : MonoBehaviour
    {
        /// <summary>
        /// テスト用ボタン群
        /// </summary>
        [SerializeField] private Button _uiStopBgmButton;
        [SerializeField] private Button _uiStopFadeBgmButton;
        [SerializeField] private Button _uiPlayBgm01Button;
        [SerializeField] private Button _uiPlayBgm02Button;
        [SerializeField] private Button _uiPlayFadeBgm01Button;
        [SerializeField] private Button _uiPlayFadeBgm02Button;

        [SerializeField] private Button _uiStart3dSeButton;
        [SerializeField] private Button _uiStop3dSeButton;

        [SerializeField] private Button _uiBgmEffectNormalButton;
        [SerializeField] private Button _uiBgmEffectReverbButton;
        [SerializeField] private Button _uiBgmEffectDistortionButton;

        [SerializeField] private Button _uiSeEffectNormalButton;
        [SerializeField] private Button _uiSeEffectReverbButton;
        [SerializeField] private Button _uiSeEffectDistortionButton;

        [SerializeField] private Button _uiPauseButton;
        [SerializeField] private Button _uiResumeButton;
        [SerializeField] private Button _uiOpenConfigButton;

        /// <summary>
        /// オーディオ設定UI
        /// </summary>
        [SerializeField] private UIAudioConfig _uiAudioConfig;

        /// <summary>
        /// 3DSE確認用のロボット
        /// </summary>
        [SerializeField] private List<RobotBehaviour> _robots;

        private IAudioService AudioService => ServiceLocator.Resolve<IAudioService>();

        private void Start()
        {
            _uiStopBgmButton.onClick.AddListener(StopBgm);
            _uiStopFadeBgmButton.onClick.AddListener(StopFadeBgm);
            _uiPlayBgm01Button.onClick.AddListener(PlayBgm01);
            _uiPlayBgm02Button.onClick.AddListener(PlayBgm02);
            _uiPlayFadeBgm01Button.onClick.AddListener(PlayFadeBgm01);
            _uiPlayFadeBgm02Button.onClick.AddListener(PlayFadeBgm02);

            _uiStart3dSeButton.onClick.AddListener(Start3dSeSample);
            _uiStop3dSeButton.onClick.AddListener(Stop3dSeSample);

            _uiPauseButton.onClick.AddListener(Pause);
            _uiResumeButton.onClick.AddListener(Resume);
            _uiOpenConfigButton.onClick.AddListener(OpenAudioConfig);

            _uiBgmEffectNormalButton.onClick.AddListener(ChangeBgmSnapshotNormal);
            _uiBgmEffectReverbButton.onClick.AddListener(ChangeBgmSnapshotReverb);
            _uiBgmEffectDistortionButton.onClick.AddListener(ChangeBgmSnapshotDistortion);
            _uiSeEffectNormalButton.onClick.AddListener(ChangeSeSnapshotNormal);
            _uiSeEffectReverbButton.onClick.AddListener(ChangeSeSnapshotReverb);
            _uiSeEffectDistortionButton.onClick.AddListener(ChangeSeSnapshotDistortion);

            _uiAudioConfig.gameObject.SetActive(false);
            _uiAudioConfig.SetListenerBgImage(() => _uiAudioConfig.gameObject.SetActive(false));
            _uiAudioConfig.SetValueMasterVolumeSlider(AudioService.MasterVolume);
            _uiAudioConfig.SetValueBgmVolumeSlider(AudioService.BgmVolume);
            _uiAudioConfig.SetValueSeVolumeSlider(AudioService.SeVolume);
            _uiAudioConfig.SetListenerMasterVolumeSlider(value => AudioService.MasterVolume = value);
            _uiAudioConfig.SetListenerBgmVolumeSliderCallback(value => AudioService.BgmVolume = value);
            _uiAudioConfig.SetListenerSeVolumeSliderCallback(value => AudioService.SeVolume = value);
        }

        private void StopBgm()
        {
            AudioService.StopAllBgm();
        }

        private void StopFadeBgm()
        {
            AudioService.StopAllBgmFadeOut(this, 1f);
        }

        private void PlayBgm01()
        {
            AudioService.PlayBgm(AudioName.BgmMain);
        }

        private void PlayBgm02()
        {
            AudioService.PlayBgm(AudioName.BgmPlay);
        }

        private void PlayFadeBgm01()
        {
            AudioService.PlayBgmFadeIn(this, AudioName.BgmMain, 1f);
        }

        private void PlayFadeBgm02()
        {
            AudioService.PlayBgmFadeIn(this, AudioName.BgmPlay, 1f);
        }

        private void Start3dSeSample()
        {
            foreach (var robot in _robots)
            {
                robot.StartMove();
            }
        }

        private void Stop3dSeSample()
        {
            foreach (var robot in _robots)
            {
                robot.StopMove();
            }
        }

        private void ChangeBgmSnapshotNormal()
        {
            AudioService.ChangeBgmEffectSnapshot(BgmEffectName.Normal, 1f);
        }

        private void ChangeBgmSnapshotReverb()
        {
            AudioService.ChangeBgmEffectSnapshot(BgmEffectName.Reverb, 1f);
        }

        private void ChangeBgmSnapshotDistortion()
        {
            AudioService.ChangeBgmEffectSnapshot(BgmEffectName.Distortion, 1f);
        }

        private void ChangeSeSnapshotNormal()
        {
            AudioService.ChangeSeEffectSnapshot(SeEffectName.Normal, 1f);
        }

        private void ChangeSeSnapshotReverb()
        {
            AudioService.ChangeSeEffectSnapshot(SeEffectName.Reverb, 1f);
        }

        private void ChangeSeSnapshotDistortion()
        {
            AudioService.ChangeSeEffectSnapshot(SeEffectName.Distortion, 1f);
        }

        private void Pause()
        {
            AudioService.Pause();
            foreach (var robot in _robots)
            {
                robot.Pause();
            }
        }

        private void Resume()
        {
            AudioService.Resume();
            foreach (var robot in _robots)
            {
                robot.Resume();
            }
        }

        private void OpenAudioConfig()
        {
            _uiAudioConfig.gameObject.SetActive(true);
        }
    }
}
