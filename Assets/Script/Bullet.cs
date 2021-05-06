using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
  Vector3 dir;
  [SerializeField]
  private float ReloadScale = 0.5f;
  [SerializeField]
  float speed = 10.0f;
  [SerializeField]
  float death_time = 5.0f;
  [SerializeField]
  private GameObject PathBall = null;
  bool befired = false;
  [SerializeField]
  public BallColor color = BallColor.Blue;
  [SerializeField]
  public SameColorEliminate mEliminate = null;

    public void setup(Vector3 dir){
    this.dir = dir;
    befired = true;
  }

  private void Update(){
    transform.position += dir * Time.deltaTime * speed;
    if (befired){
      death_time -= Time.deltaTime;
      if(death_time <= 0.0f){
        OnDeath();
      }
    }
  }

  public GameObject GetBallPrefab(){
    return PathBall;
  }

  public BallColor GetBallColor(){
    return color;
  }

  public SameColorEliminate GetBulletEliminate()
  {
    return mEliminate;
  }

  private void OnTriggerEnter(Collider other){
    if(other.gameObject.TryGetComponent<PathBallTag>(out PathBallTag ball)){
      Vector3 hitposition = other.ClosestPoint(gameObject.transform.position);
      //HIT
      MoveBallSystem.moveballSystem.InsertBall(this, other,hitposition);

      OnDeath();
    }
  }

  public float getReloadScale(){
    return ReloadScale;
  }

  void OnDeath(){
    gameObject.SetActive(false);
    Destroy(gameObject,1);
  }
}
