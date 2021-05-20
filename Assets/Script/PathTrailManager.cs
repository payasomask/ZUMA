using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PathTrailManager : MonoBehaviour
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
  public float timer = 0.0f;
  public float interval_time = 3.0f;
  //整個trial的開始淡出的時間
  private float trailtime;
  [SerializeField] 
  private float trial_clear_time = 1.0f;
  [SerializeField]
  private GameObject trail_prefab = null;
  List<Trail> Trail_list = new List<Trail>();

  class Trail {
    public float distanceTravelled;
    public TrailRenderer trail = null;
    public bool clear = false;
  }
  void Start()
  {
    if (pathCreator != null){
      // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
      pathCreator.pathUpdated += OnPathChanged;
    }

    //for(int i = 0; i<Trail_list.Count; i++){
    //  spwanTrail();
    //}

  }

  void Update()
  {

    //if (Input.GetKeyDown(KeyCode.Z)){
    //  trail.Clear();
    //}

    timer += Time.deltaTime;
    if(timer >= interval_time){
      timer = timer % interval_time;
      spwanTrail();
    }

    if (pathCreator != null)
    {

      for(int  i = 0;  i < Trail_list.Count; i++){
        Trail tmp = Trail_list[i];
        tmp.distanceTravelled += speed * Time.deltaTime;
        if (endOfPathInstruction == Mode.Repeat)
        {

          if (tmp.distanceTravelled >= pathCreator.path.length + trailtime * speed && tmp.clear == false)
          {
            tmp.distanceTravelled = pathCreator.path.length;
            tmp.clear = true;
          }

          if (tmp.distanceTravelled >= pathCreator.path.length + trailtime * speed + trial_clear_time * speed)
          {
            TrailRendererReset(tmp);
            return;
          }
        }
        transform.position = pathCreator.path.GetPointAtDistance(tmp.distanceTravelled, EndOfPathInstruction.Stop);
        transform.rotation = pathCreator.path.GetRotationAtDistance(tmp.distanceTravelled, EndOfPathInstruction.Stop);
      }
    }
  }

  // If the path changes during the game, update the distance travelled so that the follower's position on the new path
  // is as close as possible to its position on the old path
  void OnPathChanged()
  {
    for(int i = 0; i < Trail_list.Count; i++){
      Trail tmp = Trail_list[i];
      tmp.distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }
    
  }

  void TrailRendererReset(Trail t){
    t.distanceTravelled = 0.0f;
    transform.position = pathCreator.path.GetPointAtDistance(t.distanceTravelled, EndOfPathInstruction.Stop);
    transform.rotation = pathCreator.path.GetRotationAtDistance(t.distanceTravelled, EndOfPathInstruction.Stop);
    if (t.clear){
      t.trail.time = trailtime;
      t.trail.Clear();
      t.clear = false;
    }
  }

  void spwanTrail(){

    

    GameObject newgo = Instantiate(trail_prefab);
    Trail tmp = new Trail();
    tmp.trail = newgo.GetComponent<TrailRenderer>();
    
    trailtime = tmp.trail.time;

    tmp.clear = true;
    TrailRendererReset(tmp);

    Trail_list.Add(tmp);
  }

  void findIdleTrail(){

  }
}
