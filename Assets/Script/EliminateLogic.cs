using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliminateLogic : MonoBehaviour{
  static public EliminateLogic eliminatelogic = null;
  public class Eliminate{
    public BallColor EliminateColor;
    public int mini;
    public int insertindex = -1;
  }
  Eliminate mEliminate = null;

  private void Awake()
  {
    eliminatelogic = this;
  }

  //同顏色相鄰三個以上，插入結束後
  public void samecolor(List<BallSpwan.Ball> currentBallOnPath){
    if (mEliminate == null)
      return;

    AdjacentBall(currentBallOnPath,mEliminate);
  }

  public void setEliminate(SameColorEliminate bulletEliminate, int insertindex){
    mEliminate = new Eliminate() {  EliminateColor = bulletEliminate.ballcolor, mini = bulletEliminate.mini, insertindex = insertindex };
  }

  public void checkEliminate(List<BallSpwan.Ball> currentballsonpath){
    samecolor(currentballsonpath);
  }

  public void AdjacentBall(List<BallSpwan.Ball> currentBallOnPath, Eliminate insertdata){

  }
}
