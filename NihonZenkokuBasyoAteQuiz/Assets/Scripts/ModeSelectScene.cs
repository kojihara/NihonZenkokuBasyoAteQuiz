using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;

public class ModeSelectScene : SceneBase
{
    [SerializeField] Button backButton; //画面上部の左矢印
    [SerializeField] Button buttonPrefab;
    [SerializeField] GameObject genreSelectButtonContainer;

    //デバッグ用
    private void Start()
    {
        if (!GameObject.Find("SimpleSceneNavigator"))
        {
            OnLoad();
        }
    }

    /// <summary>
    /// ボタンオブジェクトにハイスコアを表示する
    /// </summary>
    /// <param name="genreKey">ジャンル</param>
    /// <param name="buttonObject">ボタン</param>
    private void InitButtonHighScore(string genreKey, Button buttonObject)
    {
        int score = ScoreStorage.Instance().GetHighScore(genreKey);
        if (score != 0)
        {
            if (buttonObject != null)
            {
                buttonObject.GetComponent<GenreSelectButtonScript>().SetHighScoreText(score.ToString());
            }
        }
    }

    /// <summary>
    /// ボタンオブジェクトを作成
    /// </summary>
    /// <param name="genreKey">ジャンル</param>
    /// <param name="buttonGameObject">ボタン</param>
    private Button CreateButton(string genreKey, int? count = null)
    {
        Button buttonGameObject = Instantiate(buttonPrefab, new Vector3(), Quaternion.identity);
        buttonGameObject.name = genreKey + count + "Button";
        buttonGameObject.transform.SetParent(genreSelectButtonContainer.transform, false);
        Text text = buttonGameObject.transform.Find("GenreSelectButtonText").gameObject.GetComponent<Text>();
        text.text = GenreUtility.Instance().GetDisplayName(genreKey, count);

        return buttonGameObject;
    }

    public override void OnLoad(object options = null)
    {
        // key, Button Name
        List<(string, Button)> genreList = new List<(string, Button)>();

        foreach (var genreKey in Genre.EnableKeySet())
        {
            Button buttonGameObject = CreateButton(genreKey);

            buttonGameObject
                .OnClickAsObservable()
                .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<PlayingQuizScene>(new QuestionManager(new Genre(genreKey), null)).Forget());

            genreList.Add((genreKey, buttonGameObject));
        }

        // ランダム
        Button buttonRandom30 = CreateButton(Genre.Random.GenreKey, 30);
        Button buttonRandom50 = CreateButton(Genre.Random.GenreKey, 50);
        buttonRandom30.gameObject.SetActive(ScoreStorage.Instance().GetFinishedFlag());
        buttonRandom50.gameObject.SetActive(ScoreStorage.Instance().GetAnswerAllFlag());
        genreList.Add((GenreUtility.Instance().GetKey(Genre.Random.GenreKey, 30), buttonRandom30));
        genreList.Add((GenreUtility.Instance().GetKey(Genre.Random.GenreKey, 50), buttonRandom50));
        buttonRandom30
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<PlayingQuizScene>(new QuestionManager(Genre.Random, 30)).Forget());
        buttonRandom50
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<PlayingQuizScene>(new QuestionManager(Genre.Random, 50)).Forget());

        //ハイスコアのセット
        foreach ((string genreKey, Button buttonObject) in genreList)
        {
            InitButtonHighScore(genreKey, buttonObject);
        }

        //戻るボタン対応
        this.backButton
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());

        //androidのバックボタン対応
        this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.Escape)).Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());
    }
}
