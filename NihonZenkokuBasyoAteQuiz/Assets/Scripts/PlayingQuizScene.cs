using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System;
using UniRx.Async;

public class PlayingQuizScene : SceneBase
{
    private const int Fps = 60;

    // 拡大率最小
    private const float MinOthographicSize = 2f;
    // 拡大率最大
    private const float MaxOthographicSize
        = 6f;
    // 不正解時のマイナススコア
    private const int PenaltyScore = 20000;

    /// <summary>
    /// 地図の右上、左下のワールド座標
    /// </summary>
    private readonly Vector2 MapTopRight = new Vector2(+12.5f, +12.0f);
    private readonly Vector2 MapBottomLeft = new Vector2(-12.5f, -12.5f);

    // View(UI)
    [SerializeField] Text GenreText;
    [SerializeField] Text ProblemNumberText;
    [SerializeField] Text RemainTimeText;
    [SerializeField] Text LifeNumberText;
    [SerializeField] Text ScoreText;
    [SerializeField] Text QNumberText;
    [SerializeField] Text ProblemText;
    [SerializeField] Text NotationText;
    [SerializeField] Button RetireButton;
    [SerializeField] GameObject RightButtonGameObject;
    [SerializeField] Button leftArrowButton; //左矢印
    [SerializeField] GameObject promptPanel; //終了確認プロンプトパネル
    [SerializeField] GameObject NavigationBar;
    [SerializeField] GameObject JapanMap;

    // 演出用画像
    [SerializeField] GameObject MarkMaruGameObject;
    [SerializeField] GameObject MarkBatsuGameObject;
    [SerializeField] GameObject StartTextObject;
    [SerializeField] GameObject FinishTextObject;
    // 音声
    [SerializeField] GameObject audioPlay;

    private AudioPlay audioScript;
    // カメラ
    private Transform camTransform;


    // 残り時間[sec]
    private ReactiveProperty<float> remainTimeCount = new ReactiveProperty<float>();
    // 残りライフ
    private ReactiveProperty<int> lifeNumber = new ReactiveProperty<int>(CommonSetting.MaxLife);
    // 現在のスコア
    private ReactiveProperty<int> score = new ReactiveProperty<int>(0);
    // 現在問題番号
    private ReactiveProperty<int> qNumber = new ReactiveProperty<int>(0);
    // 合計問題数
    private int totalProblemNumber;
    // 総正解数
    private int rightCount;
    // 総不正解数
    private int failCount;
    // 1問解答中のミスタップ数
    private int misstakeCountPerQuestion;
    private bool retireFlag;
    private bool rightAnswerFlag;
    private bool isAnswerTime; // 解答時間中か

    // 問題クラス
    private QuestionManager questionManager;
    // 出題中の問題
    private Question question;

    // 操作時の一時変数
    // ズーム開始時の一時変数
    private DateTime zoomTime;
    // タッチ時のカメラ位置
    private Vector3 touchStartCameraPosition;
    // タッチ時の座標
    private Vector2 touchStartPosition;
    private Vector2 touchEndPosition;
    private ReactiveProperty<bool> tapFlag = new ReactiveProperty<bool>(false);

    // カメラのスクロール速度
    private Vector3 cameraVelocity;

    //直前の2点間の距離.
    private float lastDistance;

    //デバッグ用
    void Awake()
    {
        if (!GameObject.Find("SimpleSceneNavigator"))
        {
            OnLoad(new QuestionManager(Genre.Random, 50));
        }
    }

    void Start()
    {
        Camera.main.orthographicSize = MaxOthographicSize;
    }

    void LateUpdate()
    {
        // タッチ検知
        detectTouch();
        if (this.isAnswerTime)
        {

            // カメラ(地図)ズーム
            bool cameraZoomFlag = cameraZoom();
            // ズーム直後Xミリ秒はスクロールさせない
            if (cameraZoomFlag)
            {
                zoomTime = DateTime.Now;
            }
            else if (zoomTime.AddMilliseconds(300.0) <= DateTime.Now)
            {
                // カメラ(地図)移動
                cameraScroll();
            }
            else
            {
                this.touchStartCameraPosition = this.camTransform.position;
#if UNITY_WEBGL || UNITY_EDITOR
                this.touchStartPosition = Input.mousePosition;
#else
                this.touchStartPosition = Input.GetTouch(0).position;
#endif
                this.cameraVelocity = Vector3.zero;
            }
        }
        fixCameraPosition();
    }

    public override void OnLoad(object options = null)
    {
        this.questionManager = (QuestionManager)options;
        this.Initialize();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        this.camTransform = Camera.main.transform;

        // 音声
        this.audioScript = audioPlay.GetComponent<AudioPlay>();
        var audioSource = audioPlay.GetComponent<AudioSource>();
        audioSource.mute = CommonSetting.IsMute;

        this.qNumber.Value = 0;
        this.ProblemText.text = "";
        GameObject questionInformationGameObject = GameObject.Find("QuestionInfomation");
        Text notationText = questionInformationGameObject.transform.Find("NotationText").gameObject.GetComponent<Text>();
        notationText.text = notationText.text.Replace("タップ", "クリック");
        this.remainTimeCount.Value = CommonSetting.MaxTimeSec;
        this.failCount = 0;
        this.misstakeCountPerQuestion = 0;
        this.isAnswerTime = false;
        this.lastDistance = 1.0f;
        this.GenreText.text = "ジャンル : " +
            GenreUtility.Instance().GetDisplayName(this.questionManager.QuestionGenre.GenreKey, this.questionManager.GetTotalNumber());
        this.totalProblemNumber = questionManager.GetTotalNumber();

        // uniRx登録
        this.setSubscribe();
        // 問題非表示
        this.QNumberText.enabled = false;
        this.ProblemText.enabled = false;
        this.NotationText.enabled = false;
        float startSec = 1.5f;
        // START文字表示
        this.StartTextObject.transform.position = new Vector3(-25f + Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
        this.StartTextObject.SetActive(true);
        this.UpdateAsObservable()
            .Take((int)(startSec * Fps))
            .Subscribe(_ =>
            {
                if (this.StartTextObject.transform.position.x <= Camera.main.transform.position.x)
                {
                    this.StartTextObject.transform.position += new Vector3(45f / Fps / startSec, 0, 0);
                }
            },
            () => {
                // START非表示
                this.StartTextObject.SetActive(false);
                // 問題表示
                this.QNumberText.enabled = true;
                this.ProblemText.enabled = true;
                this.NotationText.enabled = true;
                // 問題スタート
                this.startProblem();
            })
            .AddTo(gameObject);

    }

    /// <summary>
    /// タップ(クリック)感知
    /// </summary>
    private void detectTouch()
    {
        if (promptPanel.activeSelf)
        {
            // 終了しますかのパネルが表示されてるならタップ無効にする
            return;
        }
        this.tapFlag.Value = false;
#if UNITY_WEBGL || UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            // タッチ開始時のカメラ位置、タッチ位置保存
            this.touchStartCameraPosition = this.camTransform.position;
            this.touchStartPosition = Input.mousePosition;
            var wpoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("GetMouseButtonDown" + Input.mousePosition + ",world:(" + wpoint.x.ToString("F5") + ","+ wpoint.y.ToString("F5")+")");
        }
        if (Input.GetMouseButtonUp(0))
        {
            this.touchEndPosition = Input.mousePosition;
            if (Vector2.Distance(this.touchStartPosition, this.touchEndPosition) < 3f)
            {
                this.tapFlag.Value = true;
            }
            var wpoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("GetMouseButtonUp" + Input.mousePosition + ",world:(" + wpoint.x.ToString("F5") + "," + wpoint.y.ToString("F5") + ")");
        }
#else // スマホ用
        if (Input.touchCount == 1) {
            // タッチ情報の取得
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began) {
                this.touchStartCameraPosition = this.camTransform.position;
                this.touchStartPosition = touch.position;
            }

            if (touch.phase == TouchPhase.Ended) {
                this.touchEndPosition = touch.position;
                if (Vector2.Distance(this.touchStartPosition, this.touchEndPosition) < 3f)
                {
                    this.tapFlag.Value = true;
                }
            }
        }
#endif
    }

    /// <summary>
    /// カメラ(地図)スクロール
    /// </summary>
    private void cameraScroll()
    {
        this.camTransform = Camera.main.transform;
        Vector3 nextCameraPosition;
        bool touchFlag = false;
#if UNITY_WEBGL || UNITY_EDITOR
        touchFlag = Input.GetMouseButton(0);
#else
        touchFlag = (Input.touchCount == 1);
#endif
        if (touchFlag)
        {
            // スワイプ分カメラを移動
            Vector2 currentTouchPos;
#if UNITY_WEBGL || UNITY_EDITOR
            currentTouchPos = Input.mousePosition;
#else
            currentTouchPos = Input.GetTouch(0).position;
#endif
            Vector2 diff = currentTouchPos - this.touchStartPosition;
            nextCameraPosition = this.touchStartCameraPosition - (Vector3)(diff * 0.01f);
            // 移動前後の位置の差から速度を求めてvelocityに保存しておく
            this.cameraVelocity = (nextCameraPosition - this.camTransform.position) / Time.deltaTime;
        }
        else
        {
            // スワイプ中でないとき、velocityを減衰させながらカメラを移動する
            this.cameraVelocity *= Mathf.Pow(0.5f, 10.0f * Time.deltaTime);
            nextCameraPosition = this.camTransform.position + cameraVelocity * Time.deltaTime;
        }
        // カメラを実際に移動
        this.camTransform.position = nextCameraPosition;
    }

    /// <summary>
    /// 地図外にはみ出たら端に戻す
    /// </summary>
    private void fixCameraPosition()
    {
        Vector2 screenBottomLeft = this.getScreenBottomLeft();
        Vector2 mapTopRight = this.getMapTopRight();
        Vector2 mapScreenRight = this.getScreenTopRight();
        Vector2 visibleScreenSize = this.getVisibleScreenSize();
        Vector3 fixCameraPosition = this.camTransform.position;
        fixCameraPosition.x = Mathf.Clamp(fixCameraPosition.x, MapBottomLeft.x + visibleScreenSize.x / 2f, MapTopRight.x - visibleScreenSize.x / 2f);
        fixCameraPosition.y = Mathf.Clamp(fixCameraPosition.y, MapBottomLeft.y + visibleScreenSize.y / 2f, MapTopRight.y - visibleScreenSize.y / 2f + mapScreenRight.y - mapTopRight.y);
        this.camTransform.position = fixCameraPosition;
    }

    /// <summary>
    /// カメラ(地図)をズームイン/ズームアウトする
    /// タッチしている位置を変更しないままズームするためカメラも移動してます
    /// カメラのProjection設定はOrthographicにする必要があります
    /// </summary>
    private bool cameraZoom()
    {
        float nextOthographicSize = Camera.main.orthographicSize;
        bool zoomFlag = false;
        Vector2 touchPosition = Vector2.zero;
#if UNITY_WEBGL || UNITY_EDITOR
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // 大:ズームアウト, 小:ズームイン
            nextOthographicSize = Camera.main.orthographicSize - 3f * scroll;
            touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            zoomFlag = true;
        }
#else // スマホ用 ピンチを検出
        
        // マルチタッチかどうか確認
        if (Input.touchCount >= 2)
        {
            // タッチしている２点を取得
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            //2点タッチ開始時の距離を記憶
            if (t2.phase == TouchPhase.Began)
            {
                lastDistance = Vector2.Distance(Camera.main.ScreenToWorldPoint(t1.position), Camera.main.ScreenToWorldPoint(t2.position));
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                // タッチ位置の移動後、長さを再測し、前回の距離からの相対値を取る。
                nextOthographicSize = Camera.main.orthographicSize;
                float newDist = Vector2.Distance(Camera.main.ScreenToWorldPoint(t1.position), Camera.main.ScreenToWorldPoint(t2.position));
                float diff =  (lastDistance - newDist) / 12.0f;
                nextOthographicSize += diff;
        
                touchPosition = (Camera.main.ScreenToWorldPoint(t1.position) + Camera.main.ScreenToWorldPoint(t2.position)) / 2;

                // 限界値をオーバーした際の処理
                nextOthographicSize = Mathf.Clamp(nextOthographicSize, MinOthographicSize, MaxOthographicSize);
                zoomFlag = true;
            }
        }
#endif
        if (zoomFlag)
        {
            Vector2 screenSize = getVisibleScreenSize();
            Vector2 bottomLeftPosition = getScreenBottomLeft();
            float ratiox = (touchPosition.x - bottomLeftPosition.x - screenSize.x / 2f) / (screenSize.x / 2);
            float ratioy = (touchPosition.y - bottomLeftPosition.y - screenSize.y / 2f) / (screenSize.y / 2);
            float othographicRatio = Camera.main.orthographicSize / nextOthographicSize;

            // 実際にズームイン/ズームアウト
            Camera.main.orthographicSize = Mathf.Clamp(nextOthographicSize, MinOthographicSize, MaxOthographicSize);
            Vector3 nextCameraPosition = this.camTransform.position;
            nextCameraPosition.x += (othographicRatio - 1.0f) * (screenSize.x / 2) * ratiox;
            nextCameraPosition.y += (othographicRatio - 1.0f) * (screenSize.y / 2) * ratioy;
            // カメラ移動
            this.camTransform.position = nextCameraPosition;
        }
        return zoomFlag;
    }
    /// <summary>
    /// 画面見える位置で左下ワールド座標を返す
    /// </summary>
    /// <returns>右上ワールド座標</returns>
    private Vector2 getScreenBottomLeft()
    {
        Vector2 buttomLeft = Camera.main.ViewportToWorldPoint(Vector2.zero);
        return buttomLeft;
    }

    /// <summary>
    /// 画面見える位置で右上ワールド座標を返す
    /// </summary>
    /// <returns>右上ワールド座標</returns>
    private Vector2 getScreenTopRight()
    {
        Vector2 topRight = Camera.main.ViewportToWorldPoint(Vector2.one);
        return topRight;
    }
    /// <summary>
    /// 画面見える位置で右上ワールド座標を返す
    /// </summary>
    /// <returns>右上ワールド座標</returns>
    private Vector2 getMapTopRight()
    {
        Vector2 screenTopRight = getScreenTopRight();
        float y = screenTopRight.y - getVisibleScreenSize().y * (NavigationBar.GetComponent<RectTransform>().sizeDelta.y / Screen.currentResolution.height);
        Vector2 mapTopRight = new Vector2(screenTopRight.x, y);
        return mapTopRight;
    }

    private Vector2 getVisibleScreenSize()
    {
        return getScreenTopRight() - getScreenBottomLeft();
    }

    /// <summary>
    /// クリック, 変数変化時の処理
    /// </summary>
    private void setSubscribe()
    {
        this.RetireButton.onClick.AsObservable()
            .Subscribe(_ => {
                Debug.Log("RETIRE CLICK");
                this.retireFlag = true;
                this.rightAnswerFlag = false;
            })
            .AddTo(gameObject);

        this.remainTimeCount
            .Subscribe(val =>
            {
                this.RemainTimeText.text =
                    String.Format("{0:f2}", (remainTimeCount.Value > 0) ? remainTimeCount.Value : 0);
            })
            .AddTo(gameObject);
        this.score
            .Subscribe(val =>
            {
                this.ScoreText.text = String.Format("{0:N0}", this.score);
            })
            .AddTo(gameObject);
        this.lifeNumber
            .Subscribe(val =>
            {
                this.LifeNumberText.text = String.Format("×{0}", this.lifeNumber);
            })
            .AddTo(gameObject);
        this.qNumber
            .Subscribe(val =>
            {
                this.QNumberText.text = String.Format("Q {0}", this.qNumber);
                this.ProblemNumberText.text =
                    String.Format("{0}/{1}", this.qNumber, this.totalProblemNumber);
            })
            .AddTo(gameObject);

        this.tapFlag
            .Where(_ => tapFlag.Value)
            .Subscribe(val =>
            {
                float DISTANCE_PER_PIXEL = 98.42f;
#if UNITY_WEBGL || UNITY_EDITOR
                Vector2 tapPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
#else
                Vector2 tapPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
#endif
                if (tapPosition.y >= this.getMapTopRight().y)
                {
                    // 画面上の黒枠部分はタップ無効
                    return;
                }
                Vector2 pictureXY = this.question.getPictureXY();
                float distance = DISTANCE_PER_PIXEL * Vector2.Distance(pictureXY, tapPosition); //// unity world size -> real km 
                if (distance <= this.question.radius)
                {
                    this.rightAnswerFlag = true;
                }
                else
                {
                    this.audioScript.PlaySeWrongTouch();
                    this.remainTimeCount.Value -= CommonSetting.PenaltySec; // 残り秒数マイナス
                    this.misstakeCountPerQuestion++;
                }
            })
            .AddTo(gameObject);

        //終了確認プロンプトを表示
        leftArrowButton
            .OnClickAsObservable()
            .Subscribe(_ => promptPanel.SetActive(true));

        //androidのバックボタン対応
        this.UpdateAsObservable()
            .Where(_ => Input.GetKey(KeyCode.Escape))
            .Subscribe(_ => promptPanel.SetActive(true));

        //終了確認プロンプトで「はい」が押された時StartSceneに戻る
        promptPanel.GetComponent<PromptPanelScript>().okButton
            .OnClickAsObservable()
            .Subscribe(_ => SimpleSceneNavigator.Instance.GoForwardAsync<StartScene>().Forget());
    }

    /// <summary>
    /// 1問スタート
    /// </summary>
    private void startProblem()
    {
        this.MarkBatsuGameObject.SetActive(false);
        this.MarkMaruGameObject.SetActive(false);
        this.RightButtonGameObject.SetActive(false);

        this.question = questionManager.GetNextQuestion();
        this.qNumber.Value++; // 問題数
        this.ProblemText.text = this.question.name; // 問題名
        this.RemainTimeText.color = Color.white; // 残り時間色
        this.remainTimeCount.Value = CommonSetting.MaxTimeSec; // 残り時間
        this.misstakeCountPerQuestion = 0; // 間違い数
        this.retireFlag = false; // ボタンフラグ
        this.rightAnswerFlag = false; // ボタンフラグ
        this.audioScript.PlaySeStartProblem(); // 音声
        this.isAnswerTime = true;

        Observable.FromMicroCoroutine<float>(observer => displayRemainTime(observer))
            .Subscribe(
            sec =>
            {
                if (sec <= 3f)
                {
                    this.RemainTimeText.color = Color.red;
                }
            },
            () => {
                this.isAnswerTime = false;
                this.finishProblem(rightAnswerFlag);
            })
            .AddTo(gameObject);
    }

    /// <summary>
    /// 1問終了時の処理
    /// <param name="isRight">true:正解, false:不正解</param>
    /// </summary>
    private void finishProblem(bool isRight)
    {
        bool isFinished = (this.qNumber.Value >= this.totalProblemNumber);
        this.RightButtonGameObject.SetActive(true);
        this.RightButtonGameObject.transform.position = new Vector3(this.question.getPictureXY().x, this.question.getPictureXY().y, 89);
        float worldSize = 2 * this.question.radius * 6.5f / 500f;// real km -> unity world size
        this.RightButtonGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(worldSize, worldSize);
        if (isRight)
        {
            // 正解の処理
            Debug.Log("answer is right");
            this.audioScript.PlaySeRightAnswer(); // 音声
            this.MarkMaruGameObject.SetActive(true);
            this.score.Value += getScore(this.remainTimeCount.Value, this.misstakeCountPerQuestion);// スコア加算
            this.rightCount++; // 正解数カウント
            if (!isFinished)
            {
                Observable.Timer(TimeSpan.FromSeconds(1.0f)).Subscribe(_ =>
                {
                    // 次の問題
                    this.startProblem();
                }).AddTo(gameObject);
            }
        }
        else
        {
            // 不正解の処理
            Debug.Log("question failed");
            // ライフ数減, スコア減少,不正解数カウント
            this.lifeNumber.Value--;
            this.score.Value = Math.Max(this.score.Value - PenaltyScore, 0);// スコア減少
            this.failCount++;
            this.audioScript.PlaySeWrongAnswer();
            this.MarkBatsuGameObject.SetActive(true);
            // 正解の位置にカメラ移動
            int zoomCount = Fps;
            Vector2 problemXY = this.question.getPictureXY();
            // 直前のズーム度
            float lastOthographicSize = Camera.main.orthographicSize;
            this.UpdateAsObservable()
                .TakeWhile(_ => zoomCount > 0)
                .Subscribe(_ =>
                {
                    // 正解の位置にスムーズに60frame中にカメラ移動
                    Vector2 direction = (problemXY - (Vector2)this.camTransform.position) / zoomCount;
                    this.camTransform.position += (Vector3)direction;

                    float orthographicDiff = Camera.main.orthographicSize - MinOthographicSize;
                    Camera.main.orthographicSize -= orthographicDiff / zoomCount;
                    zoomCount--;
                }
                , () => {
                    if (lifeNumber.Value <= 0)
                    {
                        // 結果画面に移動
                        transitionSubmitScene();
                        return;
                    }
                    else if(!isFinished)
                    {
                        Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
                        {
                            // 次の問題
                            Camera.main.orthographicSize = lastOthographicSize;
                            this.startProblem();
                        }).AddTo(gameObject);
                    }
                }).AddTo(gameObject);
        }
        if (isFinished)
        {
            // 全問出題したら結果画面に移動
            transitionSubmitScene();
            return;
        }
    }

    /// <summary>
    /// 結果画面に遷移する
    /// </summary>
    private void transitionSubmitScene()
    {
        // FINISH文字表示
        this.FinishTextObject.transform.position = new Vector3(-5f + Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
        this.FinishTextObject.SetActive(true);
        this.UpdateAsObservable()
            .Take((int)(2f * Fps))
            .Subscribe(_ =>
            {
                if (this.FinishTextObject.transform.position.x <= Camera.main.transform.position.x-0.2f)
                {
                    this.FinishTextObject.transform.position += new Vector3(50f / Fps / 1.5f, 0, 0);
                }
            },
            () => {
                this.FinishTextObject.SetActive(false);
                // スコア保存
                QuizScore quizScore = new QuizScore(this.questionManager.QuestionGenre, this.score.Value,
                    this.questionManager.GetTotalNumber(), this.rightCount, this.failCount, this.lifeNumber.Value);
                SimpleSceneNavigator.Instance.GoForwardAsync<ScoreDisplayScene>(quizScore).Forget();
            })
            .AddTo(gameObject);
    }

    /// <summary>
    /// スコア計算 基本スコア+タイムボーナス+ノーミスボーナス
    /// </summary>
    /// <param name="remainSec">残り時間[sec]</param>
    /// <param name="misstakeCountPer1Problem">1問中に間違えた回数</param>
    /// <returns></returns>
    private int getScore(float remainSec, int misstakeCountPer1Problem)
    {
        int baseScore = 1000;
        int timeBonus = (int)(5000 * (remainSec / CommonSetting.MaxTimeSec));
        int noMissBonus = (misstakeCountPer1Problem == 0) ? 5000 : 0;
        return baseScore + timeBonus + noMissBonus;
    }

    /// <summary>
    /// 残り時間のカウント
    /// </summary>
    private IEnumerator displayRemainTime(IObserver<float> observer)
    {
        while (true)
        {
            yield return null;
            this.remainTimeCount.Value -= Time.deltaTime;
            // 残り時間がゼロ, リタイア, 正解をタップすれば終了
            if (this.remainTimeCount.Value <= 0 || retireFlag || rightAnswerFlag)
            {
                break;
            }
            observer.OnNext(this.remainTimeCount.Value);
        }
        observer.OnCompleted();
        yield break;
    }
}
