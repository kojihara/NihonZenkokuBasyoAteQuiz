using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 問題一覧
/// </summary>
public class QuestionManager
{
    // ジャンル
    public Genre QuestionGenre { get; private set; }
    // 未出題問題一覧
    private readonly List<Question> readyQuestionList;
    private int questionIndex;

    public QuestionManager(Genre genre, int? num = null)
    {
        this.QuestionGenre = genre;
        this.readyQuestionList = this.getQuestionList(genre, num);
        this.questionIndex = 0;
    }

    /// <summary>
    /// 問題を取得する
    /// </summary>
    /// <param name="genre">ジャンル</param>
    /// <param name="num">取得問題数 nullなら全て</param>
    private List<Question> getQuestionList(Genre genre, int? num = null)
    {
        List<Question> result = QuestionMaster.Instance().GetProblemByGenre(genre);

        var r = new System.Random();
        result = result.OrderBy(a => r.Next(result.Count)).ToList();
        if (num != null)
        {
            result = result.GetRange(0, (int)num);
        }
        return result;
    }

    /// <summary>
    /// 問題総数
    /// </summary>
    /// <returns></returns>
    public int GetTotalNumber()
    {
        return this.readyQuestionList.Count;
    }

    /// <summary>
    /// 未出題の問題の中からランダムで返す
    /// </summary>
    /// <returns>次の問題</returns>
    public Question GetNextQuestion()
    {
        if (this.questionIndex >= this.readyQuestionList.Count)
        {
            return null;
        }
        Question result = this.readyQuestionList[this.questionIndex ++];
        return result;
    }
}
