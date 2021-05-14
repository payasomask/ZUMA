using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliminateLogic : MonoBehaviour {
  static public EliminateLogic eliminatelogic = null;
  public class Eliminate {
    public BallColor EliminateColor;
    public int miniAdjacent;
    public int insertindex = -1;
  }
  Eliminate mEliminate = null;

  public class EliminateBall{
    public  int indexOnPath;
    public BallSpwan.Ball ball;
  }


  private void Awake()
  {
    eliminatelogic = this;
  }

  //同顏色相鄰三個以上，插入結束後
  EliminateBall[] samecolor(EliminateBall[] adjacent_arr, Eliminate insertdata){
    if (mEliminate == null)
      return null;

    //如何判斷球色
    //
    int insertindex = 0;
    int miniAdjacent = insertdata.miniAdjacent;
    BallColor insertcolor = insertdata.EliminateColor;
    EliminateBall insertball = null;

    foreach (var v in adjacent_arr){
      if (v.indexOnPath == insertdata.insertindex){
        insertball = v;
        break;
      }
      insertindex++;
    }

    if(insertball == null){
      Debug.Log("978 - 同色檢查裡 並未包含insertdata.index的球，請檢查AdjacentBall()回傳是否有加入插入的球");
      return null;
    }

    //插入的球是被放在最後一個index

    List<EliminateBall> samecolor_list = new List<EliminateBall>();

    samecolor_list.Add(insertball);

    //往後找
    for (int i = insertindex + 1; i < adjacent_arr.Length; i++)
    {
      EliminateBall nextBall = adjacent_arr[i];
      if (nextBall.ball.color == insertcolor)
      {
        //同色
        samecolor_list.Add(nextBall);
      }
      else{
        break;
      }
    }


    //往前找
    for (int i = insertindex - 1; i >= 0; i--)
    {
      EliminateBall nextBall = adjacent_arr[i];
      if (nextBall.ball.color == insertcolor){
        //同色
        samecolor_list.Add(nextBall);
      }
      else{
        break;
      }
    }

    if (samecolor_list.Count >= miniAdjacent)
      return samecolor_list.ToArray();

    return null;
  }

  public void setEliminate(SameColorEliminate bulletEliminate, int insertindex){
    mEliminate = new Eliminate() {  EliminateColor = bulletEliminate.ballcolor, miniAdjacent = bulletEliminate.mini, insertindex = insertindex };
  }

  public void checkEliminate(List<BallSpwan.Ball> currentballsonpath){
    if (mEliminate == null){
      Debug.Log("998 - set mEliminate first before checkEliminate...");
      return;
    }
    //檢查相連的球
    var adjacent_arr = AdjacentBall(currentballsonpath, mEliminate);
    if (adjacent_arr == null){
      Debug.Log("998 - adjacent_arr == null...");
      return;
    }
    for(int i = 0; i < adjacent_arr.Length; i++){
      Debug.Log("998 - adjacent_arr : " + adjacent_arr[i].indexOnPath);
    }
    var samecolor_arr = samecolor(adjacent_arr, mEliminate);
    if (samecolor_arr == null){
      Debug.Log("998 - samecolor_arr == null...");
      return;
    }
      
    var eliminateTarget =  processBallToindexOnpath(samecolor_arr);

    //最後的目的是要告訴movesystem要刪除那些ball
    MoveBallSystem.moveballSystem.Eliminate(eliminateTarget);
    mEliminate = null;
  }

  int[] processBallToindexOnpath(EliminateBall[] source)
  {
    List<int> tmp = new List<int>(source.Length);
    foreach(var v in source){
      Debug.Log("969 - prepare to EliminateBallIndexOnPath : " + v.indexOnPath);
      tmp.Add(v.indexOnPath);
    }

    return tmp.ToArray();
  }

  EliminateBall[] AdjacentBall(List<BallSpwan.Ball> currentBallOnPath, Eliminate insertdata){

    if (insertdata == null)
      return null;

    //如何判斷相連的球色
    //
    int insertindex = insertdata.insertindex;
    int miniAdjacent = insertdata.miniAdjacent;

    //目前最簡單的方式應該是直接用距離判斷
    BallSpwan.Ball insertball = currentBallOnPath[insertindex];

    float currentcheck_dis_onpath = insertball.dis;
    float currentcheckradius = insertball.ball_trans.lossyScale.x * 0.5f;
    List<EliminateBall> adjacent_list = new List<EliminateBall>();

    adjacent_list.Add(new EliminateBall() { ball = insertball, indexOnPath = insertindex });

    //往後找
    for (int i = insertindex +1; i < currentBallOnPath.Count; i++){
      BallSpwan.Ball nextBall = currentBallOnPath[i];
      float nextballradius = nextBall.ball_trans.lossyScale.x * 0.5f;
      float dis_offset = 0.01f;//增加可以接受少許的誤差範圍
      float dis_to_nextball = (currentcheck_dis_onpath - nextBall.dis);
      if (dis_to_nextball < nextballradius + currentcheckradius + dis_offset)
      {
        //相連
        adjacent_list.Add(new EliminateBall() { ball = nextBall, indexOnPath = i } );
      }
      else{
        break;
      }
      currentcheck_dis_onpath = nextBall.dis;
      currentcheckradius = nextballradius;
    }

    //重製條件
     currentcheck_dis_onpath = insertball.dis;
     currentcheckradius = insertball.ball_trans.lossyScale.x * 0.5f;

    //往前找
    for (int i = insertindex - 1; i >= 0; i--){
      BallSpwan.Ball nextBall = currentBallOnPath[i];
      float nextballradius = nextBall.ball_trans.lossyScale.x * 0.5f;
      float dis_offset = 0.01f;//增加可以接受少許的誤差範圍
      float dis_to_nextball = (nextBall.dis - currentcheck_dis_onpath);
      if (dis_to_nextball < nextballradius + currentcheckradius + dis_offset)
      {
        //相連
        adjacent_list.Insert(0, new EliminateBall() { ball = nextBall, indexOnPath = i });
      }
      else
      {
        break;
      }
      currentcheck_dis_onpath = nextBall.dis;
      currentcheckradius = nextballradius;
    }

    if (adjacent_list.Count >= miniAdjacent)
      return adjacent_list.ToArray();

    return null;
  }
}
