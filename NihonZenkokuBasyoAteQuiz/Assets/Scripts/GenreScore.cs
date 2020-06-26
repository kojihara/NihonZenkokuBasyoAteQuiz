
using System;
using UnityEngine;
/// <summary>
/// JSONにシリアライズしてDBに保存するジャンルごとのスコアデータ
/// </summary>
public class GenreScore
{
    //JSONの変換に含む
    [SerializeField] [HideInInspector] public string genre;
    [SerializeField] [HideInInspector] public int maxScore;
    [SerializeField] [HideInInspector] public string updatedTime;

    public GenreScore(Genre genre, int maxScore, DateTime updated)
        : this(genre.GenreKey, maxScore, updated.ToString())
    {
    }

    public GenreScore(string genre, int maxScore, string updated)
    {
        this.genre = genre;
        this.maxScore = maxScore;
        this.updatedTime = updated.ToString();
    }
}
