using System.IO;
using UnityEngine;

public class CopyrightWebView : SceneBase
{
    // HTML ファイル名 \Assets\StreamingAssets\に置かれる
    private readonly string filename = "nihonzenkokubashoatequiz_copyright.html";

    void Start()
    {
        WebViewObject webViewObject = this.transform.gameObject.AddComponent<WebViewObject>();
        webViewObject.Init();
#if UNITY_ANDROID
        webViewObject.LoadURL("file:///android_asset/" + filename);
#else
#endif
        webViewObject.SetVisibility(true);
    }
}
