using System;
using Common;
using Services;
using Spectrum;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    /// <summary>
    /// SpectrumScene管理クラス
    /// </summary>
    public class SpectrumSceneManager : MonoBehaviour
    {
        /// <summary>
        /// テスト用ボタン群
        /// </summary>
        [SerializeField] private Button _uiStopBgmButton;
        [SerializeField] private Button _uiPlayBgm01Button;
        [SerializeField] private Button _uiPlayBgm02Button;

        [SerializeField] private Button _uiSpectrumTypeNoneButton;
        [SerializeField] private Button _uiSpectrumTypeLineButton;
        [SerializeField] private Button _uiSpectrumTypeCubeButton;

        [SerializeField] private SpectrumVisualizer _lineSpectrumVisualizer;
        [SerializeField] private SpectrumVisualizer _cubeSpectrumVisualizer;

        private enum SpectrumType
        {
            None,
            Line,
            Cube,
        }

        private IAudioService AudioService => ServiceLocator.Resolve<IAudioService>();
        private Func<int, float[]> GetSpectrumData => (resolution) => AudioService.GetBgmSpectrumData(resolution);

        private void Start()
        {
            _uiStopBgmButton.onClick.AddListener(StopBgm);
            _uiPlayBgm01Button.onClick.AddListener(PlayBgm01);
            _uiPlayBgm02Button.onClick.AddListener(PlayBgm02);

            _uiSpectrumTypeNoneButton.onClick.AddListener(() => ChangeSpectrumType(SpectrumType.None));
            _uiSpectrumTypeLineButton.onClick.AddListener(() => ChangeSpectrumType(SpectrumType.Line));
            _uiSpectrumTypeCubeButton.onClick.AddListener(() => ChangeSpectrumType(SpectrumType.Cube));

            _lineSpectrumVisualizer.Initialize(GetSpectrumData);
            _cubeSpectrumVisualizer.Initialize(GetSpectrumData);
            ChangeSpectrumType(SpectrumType.None);
        }

        private void ChangeSpectrumType(SpectrumType type)
        {
            _lineSpectrumVisualizer.gameObject.SetActive(type == SpectrumType.Line);
            _cubeSpectrumVisualizer.gameObject.SetActive(type == SpectrumType.Cube);
        }

        private void StopBgm()
        {
            AudioService.StopAllBgm();
        }

        private void PlayBgm01()
        {
            AudioService.PlayBgm(AudioName.BgmMain);
        }

        private void PlayBgm02()
        {
            AudioService.PlayBgm(AudioName.BgmPlay);
        }
    }
}
