using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spectrum
{
    /// <summary>
    /// スペクトラム周波数の描画基底クラス
    /// </summary>
    public abstract class SpectrumVisualizer : MonoBehaviour
    {
        /// <summary>
        /// 周波数の分割数
        /// ※2の冪乗かつ64-8192の範囲で設定
        /// </summary>
        protected enum FrequencyResolutionType
        {
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8192 = 8192,
        }

        [Tooltip("スペクトラム周波数のサンプル数")]
        [SerializeField]
        protected FrequencyResolutionType _frequencyResolution = FrequencyResolutionType._2048;
        protected int FrequencyResolution => (int)_frequencyResolution;

        [Tooltip("取得する最小周波数")]
        [Range(0, 5000)]
        [SerializeField] protected int _filterMinFrequency = 2000;

        [Tooltip("取得する最大周波数")]
        [Range(20000, 44100)]
        [SerializeField] protected int _filterMaxFrequency = 20000;

        [Tooltip("Rendererの表示サンプル数")]
        [Range(0, 512)]
        [SerializeField] protected int _rendererSampleCount = 128;

        [Tooltip("Rendererの周波数表示倍率")]
        [Range(0, 500)]
        [SerializeField] protected int _rendererFrequencyGain = 100;

        [Tooltip("Rendererの最大高さ")]
        [Range(0, 10)]
        [SerializeField] protected float _rendererMaxHeight = 3f;

        [Tooltip("Rendererの表示範囲")]
        [Range(0, 30)]
        [SerializeField] protected float _rendererRange = 10f;

        /// <summary>
        /// Renderのルート位置
        /// </summary>
        protected Vector3 RendererRootPosition => transform.localPosition;

        private Func<int, float[]> _getSpectrumDataFunc;
        private bool _isInitialized = false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="getSpectrumDataFunc">周波数データの取得処理</param>
        public void Initialize(Func<int, float[]> getSpectrumDataFunc)
        {
            _getSpectrumDataFunc = getSpectrumDataFunc;
            _isInitialized = true;
            InitializeRenderer();
        }

        /// <summary>
        /// Renderer初期化処理
        /// </summary>
        protected virtual void InitializeRenderer()
        {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            // 周波数データを取得して指定範囲にフィルタ
            var spectrumDataArray = _getSpectrumDataFunc.Invoke(FrequencyResolution);
            spectrumDataArray = GetFilteredSpectrumDataArray(spectrumDataArray, _filterMinFrequency, _filterMaxFrequency);
            UpdateRenderer(spectrumDataArray);
        }

        /// <summary>
        /// Renderer更新処理
        /// </summary>
        /// <param name="dataArray"></param>
        protected virtual void UpdateRenderer(float[] dataArray)
        {
        }

        /// <summary>
        /// 周波数データ配列を指定範囲に切り取る
        /// </summary>
        /// <param name="dataArray"></param>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        /// <returns></returns>
        private float[] GetFilteredSpectrumDataArray(float[] dataArray, int minFrequency, int maxFrequency)
        {
            // 1indexあたりの値
            var indexValue = (float) AudioSettings.outputSampleRate / FrequencyResolution;

            // 開始、終了周波数に該当するindexを求めてフィルタ
            var startIndex = Mathf.CeilToInt(minFrequency / indexValue);
            var endIndex = Mathf.FloorToInt(maxFrequency / indexValue);
            return new List<float>(dataArray).GetRange(startIndex, endIndex - startIndex).ToArray();
        }

        /// <summary>
        /// 周波数データ配列の指定範囲の平均値を返却する
        /// </summary>
        /// <param name="dataArray"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        protected float GetAverageSpectrumDataValue(float[] dataArray, int startIndex, int endIndex)
        {
            var value = 0f;
            var count = 0;
            for (var i = startIndex; i <= endIndex; i++)
            {
                value += dataArray[i];
                count++;
            }
            return value / count;
        }
    }
}
