using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneNavigator : MonoBehaviour
{
    public static SimpleSceneNavigator Instance = null;

    [SerializeField] GameObject goLoadingBarrier;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        Instance = this;
        goLoadingBarrier.SetActive(false);
    }

    public async UniTask GoForwardAsync<T>(object options = null)
        where T : SceneBase
    {
        goLoadingBarrier.SetActive(true);
        

        await SceneManager.LoadSceneAsync(typeof(T).Name);
        goLoadingBarrier.SetActive(false);
        var nextScene = Component.FindObjectOfType<T>();
        if (nextScene == null)
            throw new System.Exception(typeof(T).Name + " is Null");

        nextScene.OnLoad(options);
    }
}