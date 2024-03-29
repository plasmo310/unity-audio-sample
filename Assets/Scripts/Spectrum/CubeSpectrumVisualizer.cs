using UnityEngine;

namespace Spectrum
{
    /// <summary>
    /// 3DCubeによるスペクトラム周波数の描画
    /// </summary>
    public class CubeSpectrumVisualizer : SpectrumVisualizer
    {
        [Tooltip("CubeのX方向の大きさ")]
        [Range(0f, 1f)]
        [SerializeField] private float _cubeScaleX = 0.1f;

        [Tooltip("CubeのZ方向の大きさ")]
        [Range(0f, 1f)]
        [SerializeField] private float _cubeScaleZ = 0.1f;

        private GameObject[] _cubeObjectArray;

        protected override void InitializeRenderer()
        {
            base.InitializeRenderer();

            CreateCubeObjects(_rendererSampleCount, _rendererRange, RendererRootPosition);
        }

        protected override void UpdateRenderer(float[] dataArray)
        {
            base.UpdateRenderer(dataArray);

            if (UpdateCubeRendererInfo())
            {
                CreateCubeObjects(_rendererSampleCount, _rendererRange, RendererRootPosition);
            }
            UpdateCubeObjects(dataArray);
        }

        private void CreateCubeObjects(int sampleCount, float range, Vector3 rootPosition)
        {
            if (_cubeObjectArray != null && _cubeObjectArray.Length > 0)
            {
                foreach (var cubeObject in _cubeObjectArray)
                {
                    Destroy(cubeObject);
                }
            }

            _cubeObjectArray = new GameObject[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.parent = gameObject.transform;
                _cubeObjectArray[i] = cube;
            }

            for (var i = 0; i < _cubeObjectArray.Length; i++)
            {
                _cubeObjectArray[i].transform.localPosition = GetCubePosition(i, sampleCount, range, rootPosition);
                _cubeObjectArray[i].transform.localScale = GetCubeScale(0f);
            }
        }

        private int _cubeRendererSampleCount;
        private float _cubeRendererRange;
        private Vector3 _cubeRendererRootPosition;

        private bool UpdateCubeRendererInfo()
        {
            if (_rendererSampleCount != _cubeRendererSampleCount
                || !Mathf.Approximately(_rendererRange, _cubeRendererRange)
                || RendererRootPosition != _cubeRendererRootPosition)
            {
                _cubeRendererSampleCount = _rendererSampleCount;
                _cubeRendererRange = _rendererRange;
                _cubeRendererRootPosition = RendererRootPosition;
                return true;
            }
            return false;
        }

        private void UpdateCubeObjects(float[] dataArray)
        {
            for (var i = 0; i < _rendererSampleCount; i++)
            {
                _cubeObjectArray[i].transform.localScale = GetCubeScale(dataArray[i]);
            }
        }

        private Vector3 GetCubePosition(int index, int sampleCount, float range, Vector3 rootPosition)
        {
            var x = range * (index / ((float) sampleCount / 2) - 1);
            return rootPosition + new Vector3(x, 0f, 0f);
        }

        private Vector3 GetCubeScale(float dataValue)
        {
            var y = Mathf.Min(_rendererMaxHeight, (dataValue * _rendererFrequencyGain));
            return new Vector3(_cubeScaleX, y, _cubeScaleZ);
        }
    }
}
