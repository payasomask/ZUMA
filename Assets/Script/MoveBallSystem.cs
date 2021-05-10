using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using System;

public class MoveBallSystem : MonoBehaviour
{
  enum Status
  {
    INIT,
    START_MOVE_FIRST_BALL,
    MOVING_FIRST_BALL,
    MOVING_FIRST_BALL_DONE,
    START,
  }
  public static MoveBallSystem moveballSystem = null;
  [SerializeField]
  private PathCreator Path = null;
  //現在正在滾動的
  private List<BallSpwan.Ball> ball_list = null;
  //init------------------------------------------------------------------
  
  //生球的時候第一顆球直接移動到指定的Path長度 normalize 的期望地點(不一定會剛好但是會最接近)
  [SerializeField]
  [Range(0.0f, 1.0f)]
  private float initFirstBallOnPath = 0.3f;
  //生球的時候最後一顆球移動到指定的Path長度 normalize 的絕對地點
  //因為整個球的運動是根據後面的球來決定的...所以這個指定位置會是絕對到達
  [SerializeField]
  [Range(0.0f, 1.0f)]
  private float initLastBallOnPath = 0.0f;

  //整個init運動總耗時
  [SerializeField]
  private float initFirstBalTime = 1.0f;
  //eaing 用
  private float changedis = 0.0f;
  private float startdis = 0.0f;
  //----------------------------------------------------------------------
  private float init_timer = 0.0f;

  [SerializeField]
  private float ball_rotatespeed = 0.5f;

  private float inserttimer = 0.2f;
  //這兩個參數-------------------------------------------
  //必須在inserttime時間內要確保insert_dis_lerp_Factor的強度設定要能盡量接近lerp結束後的數值
  //inserttime 越短 insert_dis_lerp_Factor 理論上要越接近1.0
  //inserttime 理論上也是每顆子彈發射的間隔時間，要確保insert結束後才能在進行一次insert比較保險
  [SerializeField]
  private float inserttime = 0.4f;
  [SerializeField]
  [Range(0.2f, 1.0f)]
  private float insert_dis_lerp_Factor = 0.6f;
  //------------------------------------------------------
  Status mcurrentstatus = Status.INIT;
  // Start is called before the first frame update
  private void Awake() {
    moveballSystem = this;
  }
  void Start(){
    init();
  }

  void init(){
    //finish insert
    inserttimer = inserttime;

    Debug.Log("Path Total Dis :" + Path.path.length);
    //計算出啟動時所有的球的長度
    float getinitTotalBallsDis = getTargetDisOnPath(initFirstBallOnPath);
    float currentTotalBallsDis = 0.0f;
    while(true){
      //要一顆一顆要
      if (currentTotalBallsDis + BallSpwan.ballSpwan.getNextBallSpwanRadius() > getinitTotalBallsDis){
        break;
      }
      BallSpwan.ballSpwan.Spwan();
      currentTotalBallsDis = getCurrentTotalBallsDisOnPath();
    }
    mcurrentstatus = Status.START_MOVE_FIRST_BALL;
  }

  //target 為 normalize value(0~1)
  float getTargetDisOnPath(float target) {
    return Path.path.length * target;
  }

  // Update is called once per frame
  void Update() {

    if (mcurrentstatus == Status.START_MOVE_FIRST_BALL)
    {
      init_timer = 0.0f;
      //計算最後一顆球的目標距離，球是預先先生好了，所以基本上最後一顆球的dis一定是負值
      changedis = getTargetDisOnPath(initLastBallOnPath) -  getCurrentLastBallDisOnPath();
      startdis = getCurrentLastBallDisOnPath();
      ball_rotatespeed = 0.0f;
      mcurrentstatus = Status.MOVING_FIRST_BALL;
    }
    else if (mcurrentstatus == Status.MOVING_FIRST_BALL)
    {
      init_timer += Time.deltaTime;
      if (init_timer >= initFirstBalTime){
        init_timer = initFirstBalTime;
        mcurrentstatus = Status.MOVING_FIRST_BALL_DONE;
      }
      //因為下面的moveball是用+= ，所以這裡每次都要扣掉上一次的Dis
      float preLastBallDisOnPath = getCurrentLastBallDisOnPath();
      float targetdis = (float)CurveUtil.QuadEaseOut(init_timer, startdis, changedis, initFirstBalTime) - preLastBallDisOnPath;
      //距離 = 速度 * 時間
      //速度 = 距離 / 時間
      ball_rotatespeed = targetdis / Time.deltaTime;
    }
    else if (mcurrentstatus == Status.MOVING_FIRST_BALL_DONE){
      ball_rotatespeed = 0.1f;
      mcurrentstatus = Status.START;
      return;
    }

    MoveBall();
    float lastballdisonpath = getCurrentLastBallDisOnPath();
    float lastballradiusonpath = getCurrentLastBallRadiusOnPath();
    float nextballradius = BallSpwan.ballSpwan.getNextBallSpwanRadius();
    if (lastballdisonpath >= 0.0f && lastballdisonpath >= (lastballradiusonpath + nextballradius))
      BallSpwan.ballSpwan.Spwan();

    if (Input.GetKeyDown(KeyCode.R)) {
      removeRandomBallOnPath();
    }
  }


  void removeRandomBallOnPath() {
    if (ball_list.Count == 0)
      return;

    int randomindex = UnityEngine.Random.Range(0, ball_list.Count);
    removeBallOnPath(randomindex);
  }


  void removeBallOnPath(int index) {
    ball_list[index].root_trans.gameObject.SetActive(false);
    Destroy(ball_list[index].root_trans.gameObject);
    ball_list.RemoveAt(index);
  }
  void hideBallOnPath(int index){
    ball_list[index].root_trans.gameObject.SetActive(false);
  }
  void ShowBallOnPath(int index){
    ball_list[index].root_trans.gameObject.SetActive(true);
  }

  public void AddBall(BallSpwan.Ball ball) {
    if (ball_list == null)
      ball_list = new List<BallSpwan.Ball>();

    ball.dis = getCurrentLastBallDisOnPath() - (getCurrentLastBallRadiusOnPath() + ball.ball_trans.lossyScale.x*0.5f);
    ball.canMove = true;
    ball_list.Add(ball);
    //int ballindex = ball_list.Count - 1;
    //if (ball.dis < 0.0f)
    //  hideBallOnPath(ballindex);
    //else
    //  ShowBallOnPath(ballindex);
  }

  enum insertDir {
    Forward,
    Back
  }
  //插入
  public void InsertBall(Bullet bullet, Collider behitball, Vector3 bullethitposition)
  {
    if (ball_list == null)
      ball_list = new List<BallSpwan.Ball>();

    GameObject BallprefabOnBullet = bullet.GetBallPrefab();
    BallColor bulletcolor = bullet.GetBallColor();
    BallSpwan.Ball createball = null;
    BallSpwan.Ball hitedball = null;
    //int currentindex = 0;
    int hitedBallIndex = 0;
    //擊中的目標
    foreach (var v in ball_list) {
      if (v.ball_collider == behitball) {
        createball = BallSpwan.ballSpwan.insertBall(BallprefabOnBullet, bulletcolor, bullethitposition);
        hitedball = v;
        break;
      }
      hitedBallIndex++;
    }

    float dis = Path.path.GetClosestDistanceAlongPath(createball.root_trans.position);
    float sub = dis - hitedball.dis;

    insertDir insertdir = sub >= 0.0f ? insertDir.Forward : insertDir.Back;

    //往前或是往後一個ball直徑的距離
    float targetdis = hitedball.dis + (insertdir == insertDir.Forward ? createball.ball_trans.localScale.x : 0.0f);
    //如果是要插入前面那就維持index，是插入後面就index+1
    hitedBallIndex = insertdir == insertDir.Forward ? hitedBallIndex : hitedBallIndex + 1;


    createball.dis = targetdis;
    createball.root_trans.gameObject.SetActive(true);
    ball_list.Insert(hitedBallIndex, createball);

    inserttimer = 0.0f;
    insertAnimation = new InsertAnimation();
    createball.canMove = false;
    Debug.Log(insertdir);

    Vector3 insertPositionOnPath = Path.path.GetPointAtDistance(createball.dis /*+ ball_rotatespeed * inserttime*/);
    float ball_dis = createball.ball_trans.lossyScale.x * 0.5f + hitedball.ball_trans.lossyScale.x *0.5f;

    hitedball.ball_trans.GetComponent<MeshRenderer>().material.color= new Color(1.0f, 1.0f, 1.0f, 0.5f);
    //我們先固定一個duration，
    //還要加入這期間移動的dis
    insertAnimation.setUp(createball,
      hitedball,
      insertPositionOnPath,
      ball_dis,
      inserttime,
      () => {
        hitedball.ball_trans.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
      }
      );
    //假設撞擊點是0
    //假設disOnpath是1.0
    //也就是說從bullethitposition 面向 insertvector 總長移動長度是insertdis 在 insertdis長度的裡做 easing表演就好

    //要根據球速做一個插入的初速度來算出插入的總時間長度



    EliminateLogic.eliminatelogic.setEliminate(bullet.GetBulletEliminate(), hitedBallIndex);


  }

  BallSpwan.Ball findFirstBallOnPath() {
    float closet = float.MaxValue;
    BallSpwan.Ball cloestFinalBall = null;
    foreach (var v in ball_list) {
      float sub_dis = Path.path.length - v.dis;
      if (sub_dis < closet) {
        closet = sub_dis;
        cloestFinalBall = v;
      }
    }
    return cloestFinalBall;
  }

  public float getCurrentBallMoveSpeed() {
    return ball_rotatespeed;
  }
  InsertAnimation insertAnimation;
  public void MoveBall() {
    if (ball_list == null)
      return;
    if (ball_list.Count == 0)
      return;
    if (Path == null)
      return;

    float instertotaltime = inserttime;

    float move_dis_lerp = 0.9f;//移動的時候使用較慢的lerp減少抖動

    if (inserttimer < instertotaltime) {
      move_dis_lerp = 0.0f;//插入期間球的不能動
      inserttimer += Time.deltaTime;
      if (inserttimer >= instertotaltime){
        inserttimer = instertotaltime;
        EliminateLogic.eliminatelogic.checkEliminate(ball_list);
      }
    }

    InsertBallMove();

    //最後一顆球一定能動
    ball_list[ball_list.Count - 1].canMove = true;

    //從後方的球開始更新
    for (int i = ball_list.Count-1 ; i >= 0; i--) {
      BallSpwan.Ball ball = ball_list[i];

      //雖然是從後面開始更新球的位置
      //但判斷push也還是要從後面判斷
      BallSpwan.Ball behideball = null;
      try
      {
        behideball = ball_list[i + 1];
      }
      catch (ArgumentOutOfRangeException e)
      {
        behideball = null;
      }

      float push_dis_lerp = 0.4f;//移動push的時候使用較慢的的lerp減少抖動

      //只有一種情況是只剩一個球在path上移動
      //STAR後才開始判定push
      if (behideball != null && mcurrentstatus == Status.START)
      {
        //所有的邏輯應該都要以dis為主要判斷邏輯
        //也就是說後面的球的dis距離小於後面的球的直徑就能移動
        float push_dis = ball.ball_trans.lossyScale.x * 0.5f + behideball.ball_trans.lossyScale.x * 0.5f;
        float ball_dis = ball.dis - behideball.dis;
        if (ball_dis > push_dis){
          //表示後面的球的距離超過的兩球半徑和，這個ball不能移動
          ball.canMove = false;
        }
        else {
          //反之可以動
          ball.canMove = true;
          //並且要校兩個球的位置
          float disoffset = push_dis - ball_dis;
          ball.dis += disoffset * (inserttimer < instertotaltime ? insert_dis_lerp_Factor : push_dis_lerp);
        }
      }

      BallSpwan.Ball onInsertingBall = getInsertBall();
      if (onInsertingBall != null)
      {
        onInsertingBall.canMove = false;
      }

      if (ball.canMove == false)
        continue;

      //移動的距離 = 每秒 * 半徑 * 2 * 拍 * ball_rotatespeed
      //但為了保持每一個球(不管大小都要能維持一樣的速度前進)
      //所以球的大小會決定ball_rotatespeed的速度
      //每秒 * 半徑 * 2 * 拍 * ball_rotatespeed / (半徑 * 2)
      //每秒 * 拍 * ball_rotatespeed
      //拍又是一個常數所以就乾脆拿掉
      //當有球需要插入的時候，球的速率*一個降速質
      //ball.dis += Time.deltaTime * ball_rotatespeed * (inserttimer < instertotaltime ? insert_dis_lerp_Factor : move_dis_lerp);
      ball.dis += Time.deltaTime * ball_rotatespeed * move_dis_lerp;

      //Debug.Log(tmp.id + " dis : " + tmp.dis);
      ball.root_trans.position = Path.path.GetPointAtDistance(ball.dis, EndOfPathInstruction.Stop);
      ball.root_trans.rotation = Path.path.GetRotationAtDistance(ball.dis, EndOfPathInstruction.Stop);

      ball.ball_trans.Rotate(ball.root_trans.right.normalized, 360.0f * Time.deltaTime * ball_rotatespeed, Space.World);
      ball.ball_trans.localPosition = Vector3.zero;

      ShowBallOnPath(i);
      if (Path.path.GetClosestTimeOnPath(ball.root_trans.position) <= 0.0f) {
        hideBallOnPath(i);
      }

      if (Path.path.GetClosestTimeOnPath(ball.root_trans.position) >= 1.0f){
        Debug.Log("ball id : " + ball.id + "， was arrivaled end of path");
        removeBallOnPath(i);
      }
    }
  }

  public float getCurrentLastBallDisOnPath(){
    if(ball_list.Count == 0){
      return 0.0f;
    }

    return ball_list[ball_list.Count - 1].dis;
  }

  public float getCurrentFirstBallDisOnPath(){
    if (ball_list.Count == 0){
      return 0.0f;
    }

    return ball_list[0].dis;
  }

  public float getCurrentLastBallRadiusOnPath()
  {
    if (ball_list.Count == 0)
    {
      return 0.0f;
    }

    return ball_list[ball_list.Count - 1].ball_trans.lossyScale.x * 0.5f;
  }

  //包含空隙，包含正在list裡但可能dis是負的球
  public float getCurrentTotalBallsDisOnPath(){
    return getCurrentFirstBallDisOnPath() - getCurrentLastBallDisOnPath();
  }

  public float getInsertTime(){
    return inserttime;
  }

  BallSpwan.Ball getInsertBall(){
    if (insertAnimation != null){
      BallSpwan.Ball onInsertingBall = insertAnimation.getBall();
      if (onInsertingBall != null)
      {
        onInsertingBall.canMove = false;
      }
    }
    return null;
  }

  void InsertBallMove(){
    //insertBallAnimation
    if (insertAnimation != null){
      //insertAnimation.Easing(inserttimer);
      insertAnimation.RadiansEasing(inserttimer);
    }

    return;
  }
}
