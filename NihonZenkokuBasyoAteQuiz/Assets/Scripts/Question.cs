using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 問題情報
/// </summary>
[Serializable]
public class Question
{
    [SerializeField] public string name { get; } // 問題の地名
    [SerializeField] public float latitude { get; } // 緯度
    [SerializeField] public float longitude { get; }// 経度
    [SerializeField] public float radius { get; } // 中心位置からの半径
    [SerializeField] public string tips { get; } // 地名のTIPS

    // 日本地図画像上での位置
    public Vector2 RealXY;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="name">問題の地名</param>
    /// <param name="latitude">緯度</param>
    /// <param name="longitude">経度</param>
    /// <param name="radius">中心位置からの半径</param>
    /// <param name="tips">地名のTIPS</param>
    public Question(string name, float latitude, float longitude, float radius, string tips)
    {
        this.name = name;
        this.latitude = latitude;
        this.longitude = longitude;
        this.radius = radius;
        this.tips = tips;

        float x = -0.000364f * longitude * longitude + 1.0132f * longitude - 130.740f;
        float y = 0.007325626f * latitude * latitude + 0.614369f * latitude - 31.168263f;
        this.RealXY = new Vector2(x, y);
    }

    /// <summary>
    /// 地図上での位置
    /// </summary>
    /// <returns></returns>
    public Vector2 getPictureXY()
    {
        Debug.Log("name:"+name+",x:"+this.RealXY.x +",y:"+ this.RealXY.y);
        return this.RealXY;
    }
}
