using UnityEngine;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;

public class CopyrightScene : SceneBase
{
    public override void OnLoad(object options = null)
    {
        //左矢印ボタン
        base.OnLoad(options);

        //Androidのバックボタン
        this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.Escape)).Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());
    }
}
