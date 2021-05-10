using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void Action();
public class InsertAnimation {
  BallSpwan.Ball ball;

  float ball_dis;
  //子彈撞擊BALLONPATH當時的位置
  Vector3 BulletHiedBallOnPathPosition;
  Vector3 targetDir;
  float targetlength;
  BallSpwan.Ball hitBall;
  float duration;
  Action OnEasinDone = null;

  public void RadiansEasing(float time){
    if(time >= duration){
      time = duration;
      OnEasinDone?.Invoke();
    }
    if (ball == null)
      return;
    //Debug.Log("Easining!!");
    float currentlength = (float)CurveUtil.Linear(time, 0.0f, targetlength, duration);
    Vector3 currentPosition = BulletHiedBallOnPathPosition + targetDir * currentlength;
    Vector3 current_vector_after_move = (currentPosition - hitBall.root_trans.position);
    float ball_dis_after_move = current_vector_after_move.magnitude;
    Vector3 pushDir = current_vector_after_move.normalized;

    if (ball_dis_after_move < ball_dis){
      float pushlength = ball_dis - ball_dis_after_move;
      currentPosition = currentPosition + pushDir * pushlength;
    }
    ball.root_trans.position = currentPosition;

    if (time >= duration)
      ball = null;
  }


  public void setUp(BallSpwan.Ball ball, BallSpwan.Ball Hitball,Vector3 FinalPoint, float radius, float duration, Action onEasingDone)
  {
    this.duration = duration;
    this.ball = ball;
    this.hitBall = Hitball;
    Vector3 finalPoint = FinalPoint;
    OnEasinDone = onEasingDone;    
    BulletHiedBallOnPathPosition = ball.root_trans.position;
    Vector3 targetvector = (FinalPoint - ball.root_trans.position);
    targetDir = targetvector.normalized;
    targetlength = targetvector.magnitude;
    ball_dis = radius;
    Debug.DrawLine(ball.ball_trans.position, ball.ball_trans.position + (targetDir * targetlength), Color.black, this.duration);
    //Debug.DrawLine(ball.ball_trans.position, ball.ball_trans.position + (StartDir * targetlength), Color.blue, this.duration);
    //Debug.DrawLine(ball.ball_trans.position, ball.ball_trans.position + (EndDir * targetlength), Color.red, this.duration);

  }


  public BallSpwan.Ball getBall(){
    return ball;
  }
}
