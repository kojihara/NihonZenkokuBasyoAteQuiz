using UnityEngine;
using UniRx;
using UnityEngine.UI;
using UniRx.Async;
using System;

public class StartScene : SceneBase
{
    [SerializeField] Button trainingButton;
    [SerializeField] Button rankingModeButton;
    [SerializeField] Button rankingButton;
    [SerializeField] Button howToPlayButton;
    [SerializeField] Button copyrightButton;
    [SerializeField] Button soundButton;
    [SerializeField] CanvasScaler cs;

    //デバッグ用
    private void Start()
    {
        if (!GameObject.Find("SimpleSceneNavigator"))
        {
            OnLoad();
        }
    }

    public override void OnLoad(object options = null)
    {

        soundButton.GetComponent<SoundButtonScript>().Toggle(CommonSetting.IsMute);

        // Training
        trainingButton
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<ModeSelectScene>(this).Forget());

        // Ranking Mode
        rankingModeButton
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<PlayingQuizScene>(new QuestionManager(Genre.All)).Forget());

        //RankingViewシーン
        rankingButton
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<RankingViewScene>().Forget());

        //HowToPlayシーン
        howToPlayButton
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<HowToPlayScene>().Forget());

        //CopyrightPlayシーン
        copyrightButton
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<CopyrightScene>().Forget());

        //音声設定を取得
        bool isMute = Convert.ToBoolean(PlayerPrefs.GetInt("IsMute", 0));
        soundButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                isMute = !isMute;
                soundButton.GetComponent<SoundButtonScript>().Toggle(isMute);
                PlayerPrefs.SetInt("IsMute", Convert.ToInt32(isMute));
                CommonSetting.IsMute = Convert.ToBoolean(isMute);
            });

#if UNITY_WEBGL
        GameObject cardOverviewGameObject = GameObject.Find("TrainingButtonText");
        Text cardOverviewText = cardOverviewGameObject.GetComponent<Text>();
        cardOverviewText.text = "ジャンル選択";

        rankingModeButton.gameObject.SetActive(false);
        rankingButton.gameObject.SetActive(false);
        copyrightButton.gameObject.SetActive(false);

        var howToSize = howToPlayButton.GetComponent<RectTransform>().sizeDelta;
        howToPlayButton.GetComponent<RectTransform>().sizeDelta = new Vector2(howToSize.x, howToSize.y * 2);

        var soundPosition = soundButton.GetComponent<RectTransform>().position;
        soundButton.GetComponent<RectTransform>().transform.position = new Vector3(soundPosition.x + 20, 20, soundPosition.z);
        var howToPosition = howToPlayButton.GetComponent<RectTransform>().position;
        howToPlayButton.GetComponent<RectTransform>().transform.position = new Vector3(howToPosition.x - 20, 20, howToPosition.z);
        var trainingPosition = trainingButton.GetComponent<RectTransform>().position;
        trainingButton.GetComponent<RectTransform>().transform.position = new Vector3(trainingPosition.x, 90, trainingPosition.z);
#endif
    }
}
