using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ジャンルとスコアを受け取りそれに応じた称号を提供するモデルクラス
/// </summary>
public class TitleModel
{
    // 称号
    public string Title{ get; private set; }

    // 称号リスト
    private static readonly Dictionary<float, string> RightRateAndTitleDictionary =
        new Dictionary<float, string>()
            {
                // 正解率, 称号
                { 1.00f, "パーフェクト" },
                { 0.95f, "マスターオブ地理" },
                { 0.90f, "ゴールドクラス" },
                { 0.85f, "名誉博士" },
                { 0.80f, "大博士" },
                { 0.75f, "博士" },
                { 0.70f, "凄腕" },
                { 0.65f, "猛者" },
                { 0.60f, "免許皆伝+" },
                { 0.55f, "免許皆伝" },
                { 0.50f, "一人前+" },
                { 0.45f, "一人前" },
                { 0.40f, "地理大好き+" },
                { 0.35f, "地理大好き" },
                { 0.30f, "地理好き+" },
                { 0.25f, "地理好き" },
                { 0.20f, "見習い" },
                { 0.15f, "かけだし" },
                { 0.10f, "初学者" },
                { 0.05f, "初心者" },
                { 0.00f, "頑張りましょう" },
            };

    public TitleModel(QuizScore quizScore)
    {
        this.Title = getTitle(quizScore);
    }

    /// <summary>
    /// 正答率に応じた称号取得
    /// </summary>
    /// <param name="quizScore">解答結果</param>
    /// <returns>称号</returns>
    private string getTitle(QuizScore quizScore) {
        float correctAnswerRate = (float)quizScore.RightCount / quizScore.QuestionCount;
        Debug.Log("Correct answer rate: " + correctAnswerRate);
        string result = "";
        foreach (KeyValuePair<float, string> rightRateAndTitle in RightRateAndTitleDictionary)
        {
            if (correctAnswerRate >= rightRateAndTitle.Key)
            {
                result = rightRateAndTitle.Value;
                break;
            }
        }
        // 全問挑戦で全問正解のみ別の称号を表示する
        if (quizScore.Genre == Genre.All && correctAnswerRate >= 1.0f)
        {
            result = "完全制覇";
        }
        return result;
    }
}
