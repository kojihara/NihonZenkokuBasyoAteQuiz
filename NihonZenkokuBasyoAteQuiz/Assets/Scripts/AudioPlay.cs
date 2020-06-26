using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    [SerializeField] AudioClip seRightAnswer;
    [SerializeField] AudioClip seWrongAnswer;
    [SerializeField] AudioClip seWrongTouch;
    [SerializeField] AudioClip seStartProblem;
    private AudioSource audioSource;
    
    void Start()
    {
        //Componentを取得
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 正解音ピンポーンを再生
    /// </summary>
    public void PlaySeRightAnswer()
    {
        Debug.Log("PlaySeRightAnswer");
        audioSource.PlayOneShot(seRightAnswer);
    }

    /// <summary>
    /// 不正解音1ブッブーを再生
    /// </summary>
    public void PlaySeWrongAnswer()
    {
        Debug.Log("PlaySeWrongAnswer");
        audioSource.PlayOneShot(seWrongAnswer);
    }

    /// <summary>
    /// 不正解音2ブッブーを再生
    /// </summary>
    public void PlaySeWrongTouch()
    {
        Debug.Log("PlaySeWrongTouch");
        audioSource.PlayOneShot(seWrongTouch);
    }

    /// <summary>
    /// 問題開始 ピーンを再生
    /// </summary>
    public void PlaySeStartProblem()
    {
        Debug.Log("PlaySeStartProblem");
        audioSource.PlayOneShot(seStartProblem);
    }
}
