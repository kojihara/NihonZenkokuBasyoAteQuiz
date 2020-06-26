using UnityEngine;
using UnityEngine.UI;

public class GenreSelectButtonScript : MonoBehaviour
{
    [SerializeField] GameObject highScorePanel;
    [SerializeField] Text highScoreText;
    public void SetHighScoreText(string score)
    {
        highScorePanel.SetActive(true);
        highScoreText.text = score;
    }
}
