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
  #region MoveBallCore
  public static MoveBallSystem moveballSystem = null;
  [SerializeField]
  private PathCreator Path = null;
  //現在正在滾動的
  private List<BallSpwan.Ball> ball_list = null;

  [SerializeField]
  private float ball_rotatespeed = 0.5f;
  private float default_ball_rotatespeed = 0.5f;
  #endregion

  #region Init
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
  private float init_timer = 0.0f;
  //----------------------------------------------------------------------
  #endregion

  #region InsertBall

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
  #endregion

  #region Goback
  //分段
  class segmentBall
  {
    public int segmentid = 0;
    BallSpwan.Ball firstball;
    BallSpwan.Ball endball;
    int count;
    List<BallSpwan.Ball> balls_onPath_index = new List<BallSpwan.Ball>();
    public float speed
    {
      set
      {
        speed = value;
        for (int i = 0; i < balls_onPath_index.Count; i++)
        {
          balls_onPath_index[i].speed = value;
        }
      }
    }
    float freezytimer = 0.0f;
    public void addtimer(float addvalue)
    {
      freezytimer += addvalue;
    }
  }
  List<segmentBall> segmentball_list = new List<segmentBall>();
  #endregion

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
    default_ball_rotatespeed = ball_rotatespeed;
    StartInit();
  }

  void StartInit(){
    Debug.Log("Path Total Dis :" + Path.path.length);
    //計算出啟動時所有的球的長度
    float getinitTotalBallsDis = getTargetDisOnPath(initFirstBallOnPath);
    float currentTotalBallsDis = 0.0f;
    while (true){
      //要一顆一顆要
      if (currentTotalBallsDis + BallSpwan.ballSpwan.getNextBallSpwanRadius() > getinitTotalBallsDis)
      {
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
      setAllBallSpeed(ball_rotatespeed);
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
      setAllBallSpeed(ball_rotatespeed);
    }
    else if (mcurrentstatus == Status.MOVING_FIRST_BALL_DONE){
      ball_rotatespeed = default_ball_rotatespeed;
      setAllBallSpeed(ball_rotatespeed);
      mcurrentstatus = Status.START;
      return;
    }

    MoveBall();

    InsertBallMove();

    SpwanBall();

    segmentBallOnPath();

    if (Input.GetKeyDown(KeyCode.R)) {
      removeRandomBallOnPath();
    }
  }

  void SpwanBall(){
    float lastballdisonpath = getCurrentLastBallDisOnPath();
    float lastballradiusonpath = getCurrentLastBallRadiusOnPath();
    float nextballradius = BallSpwan.ballSpwan.getNextBallSpwanRadius();

    if (lastballdisonpath >= 0.0f && lastballdisonpath >= (lastballradiusonpath + nextballradius))
      BallSpwan.ballSpwan.Spwan();
  }


  void removeRandomBallOnPath() {
    if (ball_list.Count == 0)
      return;

    int randomindex = UnityEngine.Random.Range(0, ball_list.Count);
    removeBallOnPath(randomindex);
  }


  void removeBallOnPath(int index) {
    ball_list[index].root_trans.gameObject.SetActive(false);
    ball_list[index].remove = true;
    //Destroy(ball_list[index].root_trans.gameObject);
    //ball_list.RemoveAt(index);
  }

  private void DestroyBallOnPath(int index){
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
    ball.speed = ball_rotatespeed;
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
    int hitedBallIndex = 0;
    //找尋擊中的目標
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

    if (targetdis >= Path.path.length)
      targetdis = Path.path.length - 0.01f;

    createball.dis = targetdis;
    createball.root_trans.gameObject.SetActive(true);
    ball_list.Insert(hitedBallIndex, createball);

    inserttimer = 0.0f;
    insertAnimation = new InsertAnimation();
    createball.canMove = false;
    createball.speed = 0.0f;
    createball.segmentid = hitedball.segmentid;
    Debug.Log(insertdir);
    Vector3 insertPositionOnPath = Path.path.GetPointAtDistance(createball.dis /*+ ball_rotatespeed * inserttime*/);
    float ball_dis = createball.ball_trans.lossyScale.x * 0.5f + hitedball.ball_trans.lossyScale.x *0.5f;

    //hitedball.ball_trans.GetComponent<MeshRenderer>().material.color= new Color(1.0f, 1.0f, 1.0f, 0.5f);
    //先固定一個duration，
    insertAnimation.setUp(createball,
      hitedball,
      insertPositionOnPath,
      ball_dis,
      inserttime,
      () => {
        //hitedball.ball_trans.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
      }
      );

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


    //最後一顆球一定能動
    ball_list[ball_list.Count - 1].canMove = true;
    ball_list[ball_list.Count - 1].speed = ball_rotatespeed;

    //從後方的球開始更新
    for (int i = ball_list.Count-1 ; i >= 0;) {
      BallSpwan.Ball ball = null;

      try{
        ball = ball_list[i];
      }
      catch (ArgumentOutOfRangeException){
        i = ball_list.Count - 1;      //可能有球被刪除了直接初始化i
        continue;
      }

      //雖然是從後面開始更新球的位置
      //但判斷push也還是要從後面判斷
      BallSpwan.Ball behideball = findbehideball(i);

      float push_dis_lerp = 0.4f;//移動push的時候使用較慢的的lerp減少抖動


      //STAR後才開始判定push
      if (behideball != null && mcurrentstatus == Status.START)
      {
        //所有的邏輯應該都要以dis為主要判斷邏輯
        //也就是說後面的球的dis距離小於後面的球的直徑就能移動
        float push_dis = ball.ball_trans.lossyScale.x * 0.5f + behideball.ball_trans.lossyScale.x * 0.5f;
        float ball_dis = ball.dis - behideball.dis;
        float pushoffset = 0.005f;//增加少許判斷push的範圍範圍
        if (ball_dis > push_dis + pushoffset){
          //表示後面的球的距離超過的兩球半徑和，這個ball不能移動
          ball.canMove = false;

          //回吸加速
          float backaspeed = 0.2f;
          if (ball.gobacktimer < gobackmaxtime)
            backaspeed = 0.0f;

          if (ball.speed >= 0.0f)
            ball.speed = 0.0f;

          ball.speed -= Time.deltaTime * backaspeed;
          //繼承上一顆球的回吸速度，那怎麼判斷要不要繼承
          if (behideball.canMove == false && behideball.speed<0.0f){
            ball.speed = behideball.speed;
          }

        }
        else {

          //反之可以動
          ball.canMove = true;
          ball.speed = ball_rotatespeed;
          //並且要校兩個球的位置
          float disoffset = push_dis - ball_dis;
          ball.dis += disoffset * (inserttimer < instertotaltime ? insert_dis_lerp_Factor : push_dis_lerp);

        }
      }

      //強制暫停插入球的位置更新(表演球插入畫面)
      freezyInsertBall();

      //為了方便辨識把不能動的球改成黑色球
      //setBallColor(ball);

      //這個也是讓插入球可以表演的關鍵
      //if (ball.canMove == false){
      //  i--;      //部進      
      //  continue;
      //}

      //移動的距離 = 每秒 * 半徑 * 2 * 拍 * ball_rotatespeed
      //但為了保持每一個球(不管大小都要能維持一樣的速度前進)
      //所以球的大小會決定ball_rotatespeed的速度
      //每秒 * 半徑 * 2 * 拍 * ball_rotatespeed / (半徑 * 2)
      //每秒 * 拍 * ball_rotatespeed
      //拍又是一個常數所以就乾脆拿掉
      //當有球需要插入的時候，球的速率*一個降速質
      //ball.dis += Time.deltaTime * ball_rotatespeed * (inserttimer < instertotaltime ? insert_dis_lerp_Factor : move_dis_lerp);
      ball.dis += Time.deltaTime * ball.speed * move_dis_lerp;

      //Debug.Log(tmp.id + " dis : " + tmp.dis);
      ball.root_trans.position = Path.path.GetPointAtDistance(ball.dis, EndOfPathInstruction.Stop);
      ball.root_trans.rotation = Path.path.GetRotationAtDistance(ball.dis, EndOfPathInstruction.Stop);

      ball.ball_trans.Rotate(ball.root_trans.right.normalized, 360.0f * Time.deltaTime * ball.speed, Space.World);
      ball.ball_trans.localPosition = Vector3.zero;



      ShowBallOnPath(i);
      if (Path.path.GetClosestTimeOnPath(ball.root_trans.position) <= 0.0f) {
        hideBallOnPath(i);
      }

      if (Path.path.GetClosestTimeOnPath(ball.root_trans.position) >= 1.0f){
        Debug.Log("ball id : " + ball.id + "， was arrivaled end of path");
        removeBallOnPath(i);
      }

      //球插入期間避開刪除造成的error
      if (inserttimer < inserttime){
        i--;
        continue;
      }


      if (ball.remove == true){
        //因為刪除球會導致整個list裡面的球index.會有變化，所以只有在不刪除的情況才可以繼續檢查下一個球
        //所有其他continue都要額外執行i--
        DestroyBallOnPath(i);
        continue;
      }
      //部進
      i--;
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

  void freezyInsertBall(){
    if (insertAnimation != null){
      BallSpwan.Ball onInsertingBall = insertAnimation.getBall();
      if (onInsertingBall != null){
        onInsertingBall.canMove = false;
        //setBallColor(onInsertingBall);
      }
    }
    return;
  }

  void InsertBallMove(){
    //insertBallAnimation
    if (insertAnimation != null){
      //insertAnimation.Easing(inserttimer);
      insertAnimation.RadiansEasing(inserttimer);
    }

    return;
  }

  public void Eliminate(int[] targets){
    if (targets == null)
      return;
    foreach(var v in targets){
      removeBallOnPath(v);
    }
  }

  BallSpwan.Ball findbehideball(int index) {
    if (ball_list.Count == 0 || ball_list.Count == 1)
      return null;

    int behideindex = index + 1;
    if (behideindex >= ball_list.Count)
      return null;

    return ball_list[behideindex];
  }

  BallSpwan.Ball findfrontball(int index){
    if (ball_list.Count == 0 || ball_list.Count == 1)
      return null;

    int behideindex = index - 1;
    if (behideindex < 0)
      return null;

    return ball_list[behideindex];
  }

  void segmentBallOnPath(){
    int currentsegmentid = 0;
    BallSpwan.Ball segmentfirstball = null;

    //要分段，不過這個分段的邏輯跟消除的時候的分段邏輯感覺要用同一個...不然會導致一些問題..
    for (int i = ball_list.Count -1; i >= 0; i --){
      BallSpwan.Ball frontball = findfrontball(i);
      BallSpwan.Ball ball = ball_list[i];
      if(frontball == null){
        ball.segmentid = currentsegmentid;
        //setBallColor(ball);
        continue;
      }

      //在進行判斷更改前先暫存前面那顆球的segmentid
      //int tempfrontballsegmentid = frontball.segmentid;

      float touch_legnth = frontball.ball_trans.lossyScale.x * 0.5f + ball.ball_trans.lossyScale.x * 0.5f;
      float touch_offset = 0.01f;//小許誤差容忍
      float bal_ldis = frontball.dis - ball.dis;
      if (bal_ldis > touch_legnth + touch_offset){
        //確定是斷開
        ball.segmentid = currentsegmentid;
        currentsegmentid++;
        //找到目前分段後，上一段的最前面一顆球
        segmentfirstball = ball;
      }
      else{
        //相連
        ball.segmentid = currentsegmentid;
        frontball.gobacktimer = 0.0f;

        if (frontball.segmentid - 1 == ball.segmentid){
          //觸發消除檢查
          //Debug.Log("ball index : " + i + " trriger EliminateCheck When canMove change...");
          setBallColor(ball);
          //同時更改屬於frontball的segmentID的所有球
          setBallsegmentid(frontball.segmentid, ball.segmentid);
          //EliminateLogic.eliminatelogic.setEliminate(new SameColorEliminate() { ballcolor = ball.color, mini = 3 }, i);
          //EliminateLogic.eliminatelogic.checkEliminate(ball_list);
        }
      }

      if (segmentfirstball != null){

        frontball.gobacktimer += Time.deltaTime * gobackrate;
        //前段最前面那顆，與後段最後一顆如果同顏色
        if (segmentfirstball.color == frontball.color){
          frontball.gobacktimer = gobackmaxtime;
        }
        //只影響分段後，後面的第一顆球
        segmentfirstball = null;
      }
      //setBallColor(ball);
    }
  }

  float gobackrate = 1.0f;//回吸計時器的計時速率
  float gobackmaxtime = 1.0f;//回吸時最慢等待時間

  void setAllBallSpeed(float speed){
    for(int i = 0; i < ball_list.Count; i++){
      ball_list[i].speed = speed;
    }
  }
  void setBallColor(BallSpwan.Ball ball){
    //canmovedebug用
    //if (ball.canMove == false){
    //  ball.ball_trans.GetComponent<MeshRenderer>().material.color = Color.black;
    //  return;
    //}

    //MeshRenderer MeshRenderer = ball.ball_trans.GetComponent<MeshRenderer>();

    //if (ball.color == BallColor.Blue){
    //  MeshRenderer.material.color = Color.blue;
    //}
    //else if(ball.color == BallColor.Red){
    //  MeshRenderer.material.color = Color.red;
    //}else if(ball.color == BallColor.Yellow){
    //  MeshRenderer.material.color = Color.yellow;
    //}

    //segment用
    //float rgb = (ball.segmentid % 4) * 0.25f;
    //ball.ball_trans.GetComponent<MeshRenderer>().material.color = new Color(rgb, rgb, rgb,1.0f);

    //gobacktime用
    //float rgb = ball.gobacktimer;
    //ball.ball_trans.GetComponent<MeshRenderer>().material.color = new Color(rgb, rgb, rgb, 1.0f);

    //連消用
    ball.ball_trans.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
  }

  void setBallsegmentid(int preid, int currentid){

    for(int i = 0; i < ball_list.Count; i++){
      if (ball_list[i].segmentid == preid)
        ball_list[i].segmentid = currentid;
      else
        continue;
    }
  }
}
