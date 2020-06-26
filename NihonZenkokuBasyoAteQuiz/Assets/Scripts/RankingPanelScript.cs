using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingPanelScript : MonoBehaviour
{
    [SerializeField] GameObject scoreDataElement;
    private ScoreDataElement reference;
    private const string colorString = "#e74c3c"; // 赤色の16進数文字列

    public void CreateRankingElements(List<RankingScore> data)
    {
        string uid = SystemInfo.deviceUniqueIdentifier;
        int len = data.Count;
        for (int i = len - 1; i >= 0; i--)
        {
            GameObject toInstantiate = Instantiate(scoreDataElement);
            reference = toInstantiate.GetComponent<ScoreDataElement>();
            reference.RankingText = (len - i).ToString();
            reference.NameText = data[i].name;
            reference.ScoreText = data[i].score.ToString();
            reference.TimeText = data[i].updatedTime;

            if (data[i].isCurrentUser)
            {
                Color newColor;
                ColorUtility.TryParseHtmlString(colorString, out newColor); // 新しくColorを作成
                toInstantiate.GetComponent<Image>().color = newColor;
                Debug.Log("current " + toInstantiate.GetComponent<Image>().color.ToString());
            }

            toInstantiate.transform.SetParent(transform);
        }
    }
}
