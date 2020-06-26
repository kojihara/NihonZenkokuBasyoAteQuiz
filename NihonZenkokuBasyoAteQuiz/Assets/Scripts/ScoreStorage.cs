#if UNITY_ANDROID || UNITY_EDITOR // FIREBASE保存はANDROIDのみ
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
#endif
using UnityEngine;
using UniRx;
using UniRx.Async;
using System;
using System.Collections.Generic;

public class ScoreStorage
{
    private static readonly string FirebaseEndPoint = "https://nihonzenkokubasyoatequiz.firebaseio.com/";
    public static readonly string FirebaseRankingScores = "rankingScores";
    private static readonly string FirebaseGenreScore = "individual";
    private static readonly string FirebaseScore = "score";
    private static readonly int FirebaseRankingCount = 50;
    public const int TimeoutSecond = 3;
    private const int On = 1;
    private const int Off = 0;

    private const string Launch1stKey = "Launch1stApp";
    private const string FinishedKey = "FinishedCategory";
    private const string AnswerAllKey = "AnswerAll";
    private const string RegisterdNameKey = "RegisteredName";

#if UNITY_ANDROID || UNITY_EDITOR
    private readonly DatabaseReference dbRef;
    private readonly AuthModel auth;
#endif

    private static readonly ScoreStorage instance = new ScoreStorage();

    public static ScoreStorage Instance()
    {
        return instance;
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    private ScoreStorage()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        //データベースのセットアップ
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(FirebaseEndPoint);
        this.dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        this.auth = new AuthModel();
#endif
    }

    /// <summary>
    /// 新しいスコアを受け取りハイスコアであればセーブする
    /// </summary>
    /// <param name="newScore"></param>
    /// <returns>セーブした場合はtrueを返す</returns>
    public bool SaveScore(QuizScore newScore)
    {
        string key = GenreUtility.Instance().GetKey(newScore.Genre.GenreKey, newScore.QuestionCount);
        int current = GetHighScore(newScore.Genre.GenreKey, newScore.QuestionCount);
        if(current < newScore.Score)
        {
            PlayerPrefs.SetInt(key, newScore.Score);
            // Firebaseに保存
            SaveAsyncIndividualScore(newScore.Genre, newScore.Score);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 個人情報をFirebaseに保存
    /// </summary>
    /// <param name="genre">ジャンル</param>
    /// <param name="score">スコア</param>
    private async void SaveAsyncIndividualScore(Genre genre, int score)
    {
#if UNITY_ANDROID || UNITY_EDITOR
        await this.auth
        .SignInAnonymouslyAsync()
        .ContinueWith(() => SubmitAsyncIndividualScore(genre, score).Timeout(TimeSpan.FromSeconds(TimeoutSecond)));
#endif
    }

    /// <summary>
    /// ランキングにスコアデータを保存
    /// </summary>
    /// <returns></returns>
    private async UniTask SubmitAsyncIndividualScore(Genre genre, int score)
    {
#if UNITY_ANDROID || UNITY_EDITOR
        string json = JsonUtility.ToJson(new GenreScore(genre, score, DateTime.Now));Debug.Log(json);
        string uid = SystemInfo.deviceUniqueIdentifier; //unityで生成されるデバイスごとのUniqueID

        await this.dbRef.Child(FirebaseGenreScore).Child(uid).Child(genre.GenreKey).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            Debug.Log("Task IsCompleted:" + task.IsCompleted + ",IsFaulted:" + task.IsFaulted + ",IsCanceled:" + task.IsCanceled);
        });
#endif
    }

    public int GetHighScore(string genre, int problemCount)
    {
        string key = GenreUtility.Instance().GetKey(genre, problemCount);
        return GetHighScore(key);
    }
    public int GetHighScore(string key)
    {
        int highScore = PlayerPrefs.GetInt(key, 0);
        return highScore;
    }

    /// <summary>
    /// スコアをFirebaseから取得する
    /// </summary>
    /// <returns></returns>
    public async UniTask InitializeScore()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        if (PlayerPrefs.GetInt(Launch1stKey, Off) != Off)
        {
            return;
        }
        Debug.Log("before FetchGenreScoreAsync");
        List<GenreScore> scoreList = await FetchGenreScoreAsync();
        foreach (GenreScore genreScore in scoreList)
        {
            PlayerPrefs.SetInt(genreScore.genre, Math.Max(genreScore.maxScore, PlayerPrefs.GetInt(genreScore.genre, 0)));
        }
        PlayerPrefs.SetInt(Launch1stKey, On);
#endif
    }

#if UNITY_ANDROID || UNITY_EDITOR
    /// <summary>
    /// DBにアクセスし、デシリアライズしてリストにして返す
    /// </summary>
    /// <returns></returns>
    private async UniTask<List<GenreScore>> FetchGenreScoreAsync()
    {
        List<GenreScore> genreScoreList = new List<GenreScore>();
        DataSnapshot snap = await this.dbRef
            .Child(FirebaseGenreScore)
            .Child(SystemInfo.deviceUniqueIdentifier)
            .GetValueAsync();
        foreach (DataSnapshot data in snap.Children)
        {
            string genre = data.Key;
            int maxScore = Convert.ToInt32(data.Child("maxScore").Value);
            string updatedTime = (string)data.Child("updatedTime").Value;
            genreScoreList.Add(new GenreScore(genre, maxScore, updatedTime));
        }
        return genreScoreList;
    }
#endif

    /// <summary>
    /// いずれかのカテゴリでlifeを残したまま最終問題まで行ったことがある
    /// </summary>
    /// <param name="flag"></param>
    public void SetFinishedFlag()
    {
        PlayerPrefs.SetInt(FinishedKey, On);
    }
    /// <summary>
    /// いずれかのカテゴリで全問正解したことがある
    /// </summary>
    /// <param name="flag"></param>
    public void SetAnswerAllFlag()
    {
        PlayerPrefs.SetInt(AnswerAllKey, On);
    }
    public bool GetFinishedFlag()
    {
        return PlayerPrefs.GetInt(FinishedKey, Off) != Off;
    }
    public bool GetAnswerAllFlag()
    {
        return PlayerPrefs.GetInt(AnswerAllKey, Off) != Off;
    }


    /// <summary>
    /// ランキング用スコア登録
    /// ※timeoutSecond内に接続できなければタイムアウト例外をスロー
    /// </summary>
    /// <param name="score"></param>
    /// <param name="name"></param>
    public async void SaveAsyncRankingScore(int score, string name)
    {
#if UNITY_ANDROID || UNITY_EDITOR
        await this.auth
        .SignInAnonymouslyAsync()
        .ContinueWith(() => SubmitAsyncRankingScore(score, name).Timeout(TimeSpan.FromSeconds(TimeoutSecond)));
#endif
    }

    /// <summary>
    /// ランキングにスコアデータを保存
    /// </summary>
    /// <returns></returns>
    private async UniTask SubmitAsyncRankingScore(int score, string name)
    {
#if UNITY_ANDROID || UNITY_EDITOR
        string json = JsonUtility.ToJson(new RankingScore(name, score, DateTime.Now));
        string uid = SystemInfo.deviceUniqueIdentifier; //unityで生成されるデバイスごとのUniqueID

        PlayerPrefs.SetString(RegisterdNameKey, name);

        await this.dbRef.Child(ScoreStorage.FirebaseRankingScores).Child(uid).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            Debug.Log("Task IsCompleted:" + task.IsCompleted + ",IsFaulted:" + task.IsFaulted + ",IsCanceled:" + task.IsCanceled);
        });
#endif
    }

    public string GetRegisterdName()
    {
        return PlayerPrefs.GetString(RegisterdNameKey, "");
    }

    /// <summary>
    /// Google Play 開発者サービスを確認/更新
    /// </summary>
    /// <returns></returns>
    public async UniTask SetupDatabaseAsync()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //   app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                Debug.Log("firebase is ready");
                FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(FirebaseEndPoint);
            }
            else
            {
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
                throw new Exception($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
#endif
    }

#if UNITY_ANDROID || UNITY_EDITOR
    /// <summary>
    /// スコア上位X件取得
    /// </summary>
    /// <returns></returns>
    public async UniTask<List<RankingScore>> LoadScoreAsync()
    {
        await this.auth.SignInAnonymouslyAsync().Timeout(TimeSpan.FromSeconds(TimeoutSecond)); //匿名ログイン
        return await FetchRankingScoreAsync().Timeout(TimeSpan.FromSeconds(TimeoutSecond)); //スコアデータの取得
    }
#endif

#if UNITY_ANDROID || UNITY_EDITOR
    /// <summary>
    /// DBにアクセスし、スコア上位X件をRankingScoreにして返す
    /// </summary>
    /// <returns></returns>
    private async UniTask<List<RankingScore>> FetchRankingScoreAsync()
    {
        List<RankingScore> rankingScoreList = new List<RankingScore>();

        DataSnapshot snap = await this.dbRef
            .Child(FirebaseRankingScores)
            .OrderByChild(FirebaseScore)
            .LimitToLast(FirebaseRankingCount)
            .GetValueAsync();
        foreach (DataSnapshot data in snap.Children)
        {
            string name = (string)data.Child("name").Value;
            int score = Convert.ToInt32(data.Child(FirebaseScore).Value);
            string updatedTime = (string)data.Child("updatedTime").Value;
            bool isCurrentUser = (data.Key == SystemInfo.deviceUniqueIdentifier);

            rankingScoreList.Add(new RankingScore(name, score, updatedTime, isCurrentUser));
        }
        return rankingScoreList;
    }
#endif
}
