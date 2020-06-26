using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDataElement : MonoBehaviour
{
    [SerializeField] Text rankingText;
    [SerializeField] Text nameText;
    [SerializeField] Text scoreText;
    [SerializeField] Text timeText;

    public string RankingText
    {
        get { return rankingText.text; }
        set { rankingText.text = value; }
    }

    public string NameText
    {
        get { return nameText.text; }
        set { nameText.text = value; }
    }
    public string ScoreText
    {
        get { return scoreText.text; }
        set { scoreText.text = value; }
    }
    public string TimeText
    {
        get { return timeText.text; }
        set { timeText.text = value; }
    }
}
