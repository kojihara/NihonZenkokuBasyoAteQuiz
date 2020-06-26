using System;
using UnityEngine;
using UniRx.Async;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

public class RankingViewScene : SceneBase
{
    [SerializeField] GameObject barrier;
    [SerializeField] GameObject rankingPanel;
    [SerializeField] GameObject alertMessage;
    [SerializeField] Button leftArrow;
    
    /// <summary>
    /// デバッグ用
    /// </summary>
    private void Start()
    {
        if (!GameObject.Find("SimpleSceneNavigator"))
        {
            OnLoad();
        }
    }

    public override void OnLoad(object options = null)
    {
        //androidのバックボタン対応
        this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.Escape)).Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());

        //ナビゲーションバーの左矢印の戻るボタン対応
        leftArrow.OnClickAsObservable().Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());

        barrier.SetActive(true);
#if UNITY_ANDROID || UNITY_EDITOR
        try
        {
            ScoreStorage.Instance().LoadScoreAsync().ToObservable().Subscribe(list =>
            {
                if (list != null)
                {
                    rankingPanel.GetComponent<RankingPanelScript>().CreateRankingElements(list);
                }
                barrier.SetActive(false);
            });
        }
        catch (Exception e) //TimeoutException or Exception
        {
            Debug.Log(e.Message);
            alertMessage.SetActive(true);
            barrier.SetActive(false);
            return;
        }
#endif
    }
}
