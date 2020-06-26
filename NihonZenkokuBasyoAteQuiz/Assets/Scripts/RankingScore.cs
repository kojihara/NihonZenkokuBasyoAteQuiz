
using System;
using UnityEngine;
/// <summary>
/// JSONにシリアライズしてDBに保存するスコアデータ
/// </summary>
public class RankingScore
{
    //JSONの変換に含む
    [SerializeField] [HideInInspector] public string name;
    [SerializeField] [HideInInspector] public int score;
    [SerializeField] [HideInInspector] public string updatedTime;
    [NonSerialized] [HideInInspector] public bool isCurrentUser;

    public RankingScore(string name, int score, DateTime updated)
        : this(name, score, updated.ToString(), true)
    {
    }

    public RankingScore(string name, int score, string updated, bool isCurrentUser)
    {
        this.name = name;
        this.score = score;
        this.updatedTime = updated;
        this.isCurrentUser = isCurrentUser;
    }
}
