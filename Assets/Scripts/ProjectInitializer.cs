using Services;
using Services.Impl;
using UnityEngine;

/// <summary>
/// プロジェクト初期化クラス
/// </summary>
public static class ProjectInitializer
{
    /// <summary>
    /// 初期化処理(シーンのロード前に呼ばれる)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // サービス登録
        ServiceLocator.Register<IAudioService>(new UnityAudioService());
    }
}