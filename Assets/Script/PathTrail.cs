using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PathTrail : MonoBehaviour
{

  public enum Mode{
    //PathDefault---
    Loop, 
    Reverse, 
    Stop,
    //CustomMode---
    Repeat
  }
  public PathCreator pathCreator;
  public Mode endOfPathInstruction;
  public float speed = 5;
  float distanceTravelled;

  //整個trial的開始淡出的時間
  private float trailtime;
  private TrailRenderer trail = null;
  [SerializeField]
  private float trial_clear_time = 1.0f;
  private bool clear = false;

  void Start()
  {
    if (pathCreator != null){
      // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
      pathCreator.pathUpdated += OnPathChanged;
    }

    trail = gameObject.GetComponent<TrailRenderer>();
    trailtime = trail.time;
  }

  void Update()
  {

    if (Input.GetKeyDown(KeyCode.Z)){
      trail.Clear();
    }

    if (pathCreator != null)
    {
      distanceTravelled += speed * Time.deltaTime;
      if(endOfPathInstruction == Mode.Repeat){

        if(distanceTravelled >= pathCreator.path.length + trailtime * speed && clear == false){
          distanceTravelled = pathCreator.path.length;
          clear = true;
        }

        if (distanceTravelled >= pathCreator.path.length + trailtime * speed + trial_clear_time * speed){
          distanceTravelled = 0.0f;
          transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
          transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
          TrailRendererReset();
          return;
        }
      }
      transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
      transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
    }
  }

  // If the path changes during the game, update the distance travelled so that the follower's position on the new path
  // is as close as possible to its position on the old path
  void OnPathChanged()
  {
    distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
  }

  void TrailRendererReset(){
    if (clear){
      trail.time = trailtime;
      trail.Clear();
      clear = false;
    }
  }
}
