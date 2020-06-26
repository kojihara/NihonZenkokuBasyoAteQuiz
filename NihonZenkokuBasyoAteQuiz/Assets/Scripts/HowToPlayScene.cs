using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;

public class HowToPlayScene : SceneBase
{
    [SerializeField] Button backButton;
    [SerializeField] Button helpPageButton;

    private void Start()
    {
        if (!GameObject.Find("SimpleSceneNavigator"))
        {
            OnLoad();
        }
    }

    public override void OnLoad(object options = null)
    {
        //左矢印ボタン
        base.OnLoad(options);
        backButton.OnClickAsObservable().Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());

        //Androidのバックボタン
        this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.Escape)).Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());

#if UNITY_ANDROID
        /*
        helpPageButton.gameObject.SetActive(true);
        helpPageButton.OnClickAsObservable().Subscribe(_ =>
        {
            SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget();
        });
        */
#endif

#if UNITY_WEBGL
        GameObject cardOverviewGameObject = GameObject.Find("CardOverview");
        Text cardOverviewText = cardOverviewGameObject.transform.Find("CardBody").gameObject.GetComponent<Text>();
        cardOverviewText.text = cardOverviewText.text.Replace("タップ", "クリック");

        GameObject cardModeGameObject = GameObject.Find("CardMode");
        cardModeGameObject.SetActive(false);

        GameObject cardRuleGameObject = GameObject.Find("CardRule");
        Text ruleText = cardRuleGameObject.transform.Find("CardBody").gameObject.GetComponent<Text>();
        ruleText.text = ruleText.text.Replace("タップ", "クリック").Replace("スワイプ", "ドラッグ").Replace("ピンチイン・ピンチアウト", "スクロール");
#endif
    }
}
