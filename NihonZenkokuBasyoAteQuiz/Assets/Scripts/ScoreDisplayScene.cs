using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Async;

public class ScoreDisplayScene : SceneBase
{
    [SerializeField] Text modeNameText; //ジャンル（モード名）
    [SerializeField] Text rawScoreText; //スコア
    [SerializeField] Text questionNumberText; //総問数
    [SerializeField] Text correctAnswerNumberText; //正解数
    [SerializeField] Text titleText; //称号（タイトル）
    [SerializeField] GameObject highScoreText; //ハイスコア表示
    [SerializeField] GameObject toRankingSubmitButton; //RankingSubmitSceneに移動
    [SerializeField] GameObject promptPanel; //終了確認プロンプト
    [SerializeField] Button toStartButton; //StartSceneに移動
    [SerializeField] Button backButton; //NavigationBar左矢印

    [SerializeField] GameObject simpleSceneNavigator; //デバッグ用

    private TitleModel titleModel; //称号提供モデル

    //デバッグ用
    public void Start()
    {
        if (!GameObject.Find("SimpleSceneNavigator"))
        {
            Instantiate(simpleSceneNavigator);
            OnLoad();
        }
    }

    public override void OnLoad(object options = null)
    {
        if (options == null)
        {
            Debug.Log("QuizScore does not exist!");
            options = new QuizScore(Genre.All, 80000, 100, 80, 20, 1); //モック
        }
        QuizScore quizScore = (QuizScore)options;

        //テキストのセット
        modeNameText.text = GenreUtility.Instance().GetDisplayName(quizScore.Genre.GenreKey, quizScore.QuestionCount);
        rawScoreText.text = quizScore.Score.ToString();
        questionNumberText.text = quizScore.QuestionCount.ToString();
        correctAnswerNumberText.text = quizScore.RightCount.ToString();

        //称号の取得
        titleModel = new TitleModel((QuizScore)options);
        titleText.text = titleModel.Title;

        SetButtonEvent(quizScore.Genre == Genre.All, quizScore);

        Debug.Log("current high score:" + ScoreStorage.Instance().GetHighScore(quizScore.Genre.GenreKey, quizScore.QuestionCount));
        bool isHighScore = ScoreStorage.Instance().SaveScore(quizScore);

#if UNITY_ANDROID || UNITY_EDITOR
        if (isHighScore)
        {
            highScoreText.SetActive(true);
            Debug.Log("updated high score:" + ScoreStorage.Instance().GetHighScore(quizScore.Genre.GenreKey, quizScore.QuestionCount));
        }
        else
        {
            toRankingSubmitButton.SetActive(false);
        }
#else
        toRankingSubmitButton.SetActive(false);
#endif
        if (quizScore.QuestionCount == quizScore.RightCount)
        {
            ScoreStorage.Instance().SetAnswerAllFlag();
        }
        if (quizScore.LifeCount > 0)
        {
            ScoreStorage.Instance().SetFinishedFlag();
        }
    }

    /// <summary>
    /// ボタンイベントのセット
    /// </summary>
    /// <param name="isRanking">true:ランキング登録, false:トレーニング</param>
    /// <param name="quizScore">成績</param>
    private void SetButtonEvent(bool isRanking, QuizScore quizScore)
    {
        if (isRanking)
        {
            toRankingSubmitButton.SetActive(true);

            toRankingSubmitButton
            .GetComponent<Button>()
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                SimpleSceneNavigator.Instance.GoForwardAsync<RankingSubmitScene>(quizScore).Forget();
            });

            backButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                promptPanel.SetActive(true);
            });

            toStartButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                promptPanel.SetActive(true);
            });
        }
        else
        {
            backButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget();
            });

            toStartButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget();
            });
        }

        //プロンプトパネル
        promptPanel
            .GetComponent<PromptPanelScript>()
            .okButton
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());
    }
}
