using UnityEngine;
using System;
using UniRx;
using UniRx.Async;
using System.Threading.Tasks;
using System.Threading;
#if UNITY_ANDROID || UNITY_EDITOR
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
#endif

/// <summary>
/// FirebaseにSeedデータを与えるためのテスト用スクリプト
/// </summary>
public class DBSeed : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_EDITOR
    [SerializeField] GameObject barrier;
    DatabaseReference dbRef;

    public int NumOfSeeds;

    void Start()
    {
        if(NumOfSeeds == 0)
        {
            Debug.LogError("seed数が0です。");
            return;
        }

        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://nihonzenkokubasyoatequiz.firebaseio.com/");
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        AuthModel auth = new AuthModel();
        auth.SignInAnonymouslyAsync().ContinueWith(() =>
        {
            for (int i = 0; i < NumOfSeeds; i++)
            {
                string uuid = i.ToString();
                SubmitAsync(new ScoreData("Hoge" + i, 10000 * i, DateTime.Now.ToString()), uuid);
            }
        });
    }

    private async Task SubmitAsync(ScoreData data, string uuid)
    {
        barrier.SetActive(true);
        string json = JsonUtility.ToJson(data);

        await dbRef.Child(ScoreStorage.FirebaseRankingScores).Child(uuid).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            Debug.Log("スレッドID: " + Thread.CurrentThread.ManagedThreadId); //別スレッドで実行されている
            if (task.IsCompleted)
            {
                Debug.Log("write data completed");
            }
            else if (task.IsFaulted)
            {
                Debug.Log("write was failed");
            }
            else
            {
                Debug.Log("write was canceled");
            }
        });
        barrier.SetActive(false);
    }

    /// <summary>
    /// ToJSON用クラス
    /// </summary>
    class ScoreData
    {
        public string name;
        public int score;
        public string time;

        public ScoreData(string name, int score, string time)
        {
            this.name = name;
            this.score = score;
            this.time = time;
        }
    }
#endif
}
