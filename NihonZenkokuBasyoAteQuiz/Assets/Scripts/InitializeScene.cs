using System;
using UniRx;
using UniRx.Async;
using UnityEngine;

public class InitializeScene : SceneBase
{

    void Awake()
    {
        //Androidナビゲーションバーを表示する
        Screen.fullScreen = false;
    }

    async void Start()
    {
        await LoadScoreAsync();

        // マスターデータ読み込み後 Onloadを呼び出す
#if UNITY_EDITOR || UNITY_ANDROID
        Observable.FromMicroCoroutine<int>(observer => QuestionMaster.Instance().InitializeByDownloadedAssetBundle(observer))
            .Subscribe(
            _ => { }, // OnNext
            ex => { //OnError
                Debug.LogError(ex);

                // 失敗時はローカルを読む
                Observable.FromMicroCoroutine<int>(observer => QuestionMaster.Instance().InitializeByLocalAssetBundleMaster(observer))
                    .Subscribe(
                    _ => { }, // OnNext
                    () => { // OnComplete
                        this.OnLoad();
                    })
                    .AddTo(gameObject);
            },
            () => { // OnComplete
                this.OnLoad();
            })
            .AddTo(gameObject);
#elif UNITY_WEBGL
        Observable.FromMicroCoroutine<int>(observer => QuestionMaster.Instance().InitializeByLocalAssetBundleMaster(observer))
            .Subscribe(
            _ => { }, // OnNext
            () => { // OnComplete
                // マスターデータ読み込み後 Onloadを呼び出す
                this.OnLoad();
            })
            .AddTo(gameObject);
#endif
    }

    public override void OnLoad(object options = null)
    {
        SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget();
    }

    /// <summary>
    /// DB用意, アプリ初起動の場合スコアをDBからダウンロード
    /// </summary>
    /// <returns></returns>
    public static async UniTask LoadScoreAsync()
    {
        try
        {
            var task1 = ScoreStorage.Instance().SetupDatabaseAsync();
            var task2 = ScoreStorage.Instance().InitializeScore();
            await UniTask.WhenAll(task1, task2).Timeout(TimeSpan.FromSeconds(ScoreStorage.TimeoutSecond));
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
