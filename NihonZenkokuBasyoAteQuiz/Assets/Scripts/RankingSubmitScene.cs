using System;
using System.Threading;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

public class RankingSubmitScene : SceneBase
{
    [SerializeField] Button submitButton;
    [SerializeField] GameObject barrier;
    [SerializeField] InputField nameInput;
    [SerializeField] GameObject alertMessage;
    [SerializeField] GameObject promptPanel;
    [SerializeField] Button returnTitle;
    [SerializeField] Button leftArrow;

    /// <summary>
    /// デバッグ用
    /// </summary>
    private void Start()
    {
        if (!GameObject.Find("SimpleSceneNavigator"))
        {
            OnLoad(new QuizScore(Genre.Random, 1, 1, 1, 0, 1));
        }
    }

    public override void OnLoad(object options = null)
    {
        QuizScore quizScore = (QuizScore)options;
        nameInput.text = ScoreStorage.Instance().GetRegisterdName();
        //タイトルへ戻る
        returnTitle.OnClickAsObservable().Subscribe(_ => promptPanel.SetActive(true));

        //左矢印
        leftArrow.OnClickAsObservable().Subscribe(_ => promptPanel.SetActive(true));

        //プロンプトパネル
        promptPanel
            .GetComponent<PromptPanelScript>()
            .okButton
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());


        //登録ボタン
        submitButton
            .OnClickAsObservable()
            .Where(_ => IsNameValid()) //名前のバリデーション
            .ThrottleFirst(TimeSpan.FromSeconds(2)) //TODO
            .Subscribe(_ =>
            {
                Debug.Log("スレッドID: " + Thread.CurrentThread.ManagedThreadId); //別スレッドで実行されている

                alertMessage.SetActive(false);
                try
                {
                    barrier.SetActive(true);

                    //timeoutSecond内に接続できなければタイムアウト例外をスロー
                    ScoreStorage.Instance().SaveAsyncRankingScore(quizScore.Score, nameInput.text);

                    SimpleSceneNavigator.Instance.GoForwardAsync<RankingViewScene>().Forget();
                }
                catch (TimeoutException e)
                {
                    Debug.LogError(e.Message);
                    barrier.SetActive(false);
                    alertMessage.SetActive(true);
                }
                finally
                {
                    barrier.SetActive(false);
                }
            });
    }

    /// <summary>
    /// 名前フィールドのバリデーション
    /// </summary>
    /// <returns></returns>
    private bool IsNameValid()
    {
        alertMessage.SetActive(false);
        string name = nameInput.text;
        if (string.IsNullOrWhiteSpace(name))
        {
            alertMessage.GetComponentInChildren<Text>().text = "無効な名前です";
            alertMessage.SetActive(true);

            return false;
        }
        else if (name.Length < 2 || name.Length > 8)
        {
            alertMessage.GetComponentInChildren<Text>().text = "２～８文字で入力してください";
            alertMessage.SetActive(true);

            return false;
        }
        else
        {
            return true;
        }
    }
}
