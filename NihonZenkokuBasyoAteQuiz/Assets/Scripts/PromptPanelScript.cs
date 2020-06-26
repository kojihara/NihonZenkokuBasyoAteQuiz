using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class PromptPanelScript : MonoBehaviour
{
    [SerializeField] public Button okButton;
    [SerializeField] Button cancelButton;

    private void Awake()
    {
        //UniRxのEventTriggerコンポーネントをアタッチ
        var eventTrigger = gameObject.AddComponent<ObservableEventTrigger>();
        //このパネルが押された場合、パネルを非アクティブにする
        eventTrigger.OnPointerClickAsObservable().Subscribe(_ => gameObject.SetActive(false)).AddTo(this);
        //キャンセルボタンが押された場合、パネルを非アクティブに
        cancelButton.OnClickAsObservable().Subscribe(_ => gameObject.SetActive(false)).AddTo(this);
    }
    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#elif UNITY_ANDROID
        Application.Quit();
#endif
    }
}
