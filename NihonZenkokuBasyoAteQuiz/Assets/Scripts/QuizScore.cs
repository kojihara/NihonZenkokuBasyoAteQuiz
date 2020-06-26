using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizScore
{
    // ジャンル
    public Genre Genre { get; }
    // スコア
    public int Score { get; }
    // 総問題数
    public int QuestionCount { get; }
    /// 正解数
    public int RightCount { get; }
    // 不正解数
    public int FailCount { get; }
    // 残りライフ
    public int LifeCount { get; }

    public QuizScore(Genre genre, int score, int questionCount, int rightCount, int failCount, int lifeCount)
    {
        this.Genre = genre;
        this.Score = score;
        this.QuestionCount = questionCount;
        this.RightCount = rightCount;
        this.FailCount = failCount;
        this.LifeCount = lifeCount;
    }
}
