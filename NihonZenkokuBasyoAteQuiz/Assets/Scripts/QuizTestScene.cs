using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UniRx.Async;
using UnityEngine.UI;
using System;

public class QuizTestScene : SceneBase
{
    [SerializeField] GameObject promptPanel;
    [SerializeField] GameObject aichi;
    [SerializeField] Button backButton;
    GameObject clickedGameObject;

    public override void OnLoad(object options = null)
    {
        this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.Escape)).Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());

        //終了確認プロンプトを表示させる
        backButton.OnClickAsObservable().Subscribe(_ => promptPanel.SetActive(true));
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {

            clickedGameObject = null;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit2d = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction);

            if (hit2d)
            {
                clickedGameObject = hit2d.transform.gameObject;
                if (clickedGameObject == aichi)
                {
                    MoveToNext();
                }
            }

            Debug.Log(clickedGameObject);
        }

        if (Input.touchCount > 0)
        {
            GameObject touchedGameObject = null;

            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                Debug.Log("touch position: " + touch.position);
                RaycastHit2D hit2d = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction);

                if (hit2d)
                {
                    touchedGameObject = hit2d.transform.gameObject;
                    if (touchedGameObject == aichi)
                    {
                        MoveToNext();
                    }
                }

                Debug.Log(touchedGameObject);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                // タッチ移動
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // タッチ終了
            }
        }
    }

    /// <summary>
    /// モック用メソッド
    /// </summary>
    private void MoveToNext()
    {
        aichi.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, 255);
        SimpleSceneNavigator.Instance.GoForwardAsync<RankingSubmitScene>().Forget();
    }
}
