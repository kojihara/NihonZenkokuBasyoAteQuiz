using System;
using System.Collections.Generic;
using UnityEngine;

public class Genre
{
    public static Dictionary<string, string> GenreTable { get; private set; }
        = new Dictionary<string, string>();
    public static Dictionary<string, string> ModeTable { get; private set; }
        = new Dictionary<string, string>();

    public static readonly Genre All = new Genre("ALL");
    public static readonly Genre Random = new Genre("RANDOM");

    public string GenreKey { get; }

    /// <summary>
    /// マスターデータCSVからのジャンル設定を取得する
    /// </summary>
    /// <param name="csv">マスターデータCSV</param>
    public static void InitializeMaster(string csv)
    {
        ModeTable.Clear();
        GenreTable.Clear();

        List<string[]> csvLines = new List<string[]>(); // CSVの中身を入れるリスト;
        string[] lines = csv.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            if (i == 0)
            {
                continue;
            }
            csvLines.Add(lines[i].Split(',')); // , 区切りでリストに追加
        }

        foreach (string[] rows in csvLines)
        {
            try
            {
                GenreTable.Add(rows[0], rows[1]);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        foreach(var pair in GenreTable)
        {
            ModeTable.Add(pair.Key, pair.Value);
        }
        ModeTable.Add("ALL", "全て");
        ModeTable.Add("RANDOM", "ランダム");
    }

    public static List<string> EnableKeySet()
    {
        List<string> keySet = new List<string>();
        foreach (string key in ModeTable.Keys)
        {
            List<Question> questionList = QuestionMaster.Instance().GetProblemByGenre(new Genre(key));
            if (questionList != null && questionList.Count > 0)
            {
                keySet.Add(key);
            }
        }
        return keySet;
    }

    public Genre(string name)
    {
        this.GenreKey = name;
    }
}

public class GenreUtility
{
    private static readonly GenreUtility instance = new GenreUtility();

    public static GenreUtility Instance()
    {
        return instance;
    }

    private GenreUtility()
    {
    }

    /// <summary>
    /// 画面表示する
    /// </summary>
    /// <param name="genre"></param>
    /// <param name="count">クイズ数, nullなら制限なし</param>
    /// <returns></returns>
    public string GetDisplayName(string genre, int? count = null)
    {
        if (genre != Genre.Random.GenreKey)
        {
            return Genre.ModeTable[genre];
        }
        else
        {
            return Genre.ModeTable[genre] + count;
        }
    }

    /// <summary>
    /// データ保存のキーを返す
    /// </summary>
    /// <param name="genre"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public string GetKey(string genre, int? count = null)
    {
        if (Genre.Random.GenreKey == genre && count != null)
        {
            return genre + count;
        }
        else
        {
            return genre;
        }
    }
}
