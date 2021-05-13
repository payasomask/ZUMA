using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public enum BallColor{
  Red,
  Blue,
  Yellow,
  SZ
}
public enum BallPrefabName{
  Red_ball,
  Blue_ball,
  Yellow_ball,
  SZ
}


public class BallSpwan : MonoBehaviour
{
  public static BallSpwan ballSpwan = null;

  public class Ball {
    public int id { get; set; }
    public Transform root_trans { get; set; }
    public float dis = 0.0f;
    public bool canMove { get; set; }
    public bool isSpwan { get; set; }
    public bool remove = false;
    public Transform ball_trans = null;
    public Collider ball_collider { get; set; }
    public BallColor color { get; set; }
    public float speed { get; set; }
    public int segmentid { get; set; }
    public float gobacktimer = 0.0f;
  }

  [SerializeField]
  public MoveBallSystem moveSystem = null;
  [SerializeField]
  private List<GameObject> BallPrefab = new List<GameObject>();

  //private Ball[] ball_arr = null;
  private int currentballindex = 0;
    
    // Start is called before the first frame update
    void Start(){
      ballSpwan = this;
    }


  public void Spwan(){
    //還要修改球生出來的邏輯
    //改成用最後一個正在滾動的球的dis距離原點的差要>=下一個球的半徑 + 最後一個正在滾動的球的半徑

    if (moveSystem == null){
      Debug.Log("Ball Spwan : " + gameObject.name + " movesystem is null ,set it before spwan");
      return;
    }

    //if (ball_arr.Length == 0){
    //  Debug.Log("Ball Spwan : " + gameObject.name + " ball_arr.Count == 0 ,init it before spwan");
    //  return;
    //}

    BallColor ballcolor = (BallColor)Random.Range(0, (int)BallColor.SZ);
    //ballcolor = BallColor.Red;
    moveSystem.AddBall(InstantiateBall(ballcolor));
      
  }


  public float getNextBallSpwanRadius(){
    int next_ballindex = currentballindex + 1;
    float nextball_radius = BallPrefab[next_ballindex % BallPrefab.Count].transform.Find("ball").lossyScale.x*0.5f;
    return nextball_radius;
  }

  
  Ball InstantiateBall(BallColor ballColor){
    Transform ball_root = Instantiate(BallPrefab[((int)ballColor)]).transform;

    Transform ball_trans = ball_root.Find("ball");
    ball_trans.gameObject.AddComponent<PathBallTag>();
    ball_root.gameObject.SetActive(false);
    Ball tmp_ball = new Ball()
    {
      id = currentballindex,
      root_trans = ball_root,
      ball_trans = ball_trans,
      isSpwan = false,
      ball_collider = ball_trans.GetComponent<Collider>(),
      dis = 0.0f,
      color = ballColor,
    };
    currentballindex++;

    return tmp_ball;
  }

  //IEnumerable<Ball> GetBallArr(int need)
  //{
  //  for (int index = 0; index < need; index++)
  //  {
  //    Transform ball_root = Instantiate(BallPrefab[currentballindex % BallPrefab.Count]).transform;

  //    Transform ball_trans = ball_root.Find("ball");
  //    ball_trans.gameObject.AddComponent<PathBallTag>();
  //    yield return new Ball()
  //    {
  //      id = index,
  //      root_trans = ball_root,
  //      ball_trans = ball_trans,
  //      canMove = false,
  //      isSpwan = false,
  //      ball_collider = ball_trans.GetComponent<Collider>(),
  //      dis = 0.0f,
  //    };
  //  }
  //  yield break;
  //}


  //一次產生設定數量的球
  //IEnumerable<Ball> GetBallArr(int maxballcount)
  //{
  //  for (int index = 0; index < needballcount; index++){
  //    Transform ball_root = Instantiate(BallPrefab[index % BallPrefab.Count]).transform;

  //    //GameObject collider_go = new GameObject("ball_collider");
  //    //collider_go.transform.SetParent(ball_root.transform);
  //    //collider_go.transform.localPosition = Vector3.zero;
  //    //BoxCollider ball_collider = collider_go.AddComponent<BoxCollider>();

  //    Transform ball_trans = ball_root.Find("ball");
  //    ball_trans.gameObject.AddComponent<PathBallTag>();
  //    yield return new Ball() {
  //      id = index,
  //      root_trans = ball_root,
  //      ball_trans = ball_trans,
  //      canMove = false,
  //      isSpwan = false,
  //      //球的生成時間會被球的移動速度影響 才會黏在一起
  //      //球的半徑是scale * 0.5，但實際上兩球緊密距離是裡兩個半徑(在每一ㄎ球的大小都一樣的情況下)
  //      //距離除以速度 = 時間
  //      spwanTime = ball_trans.localScale.x / moveSystem.getCurrentBallMoveSpeed(),
  //      ball_collider = ball_trans.GetComponent<Collider>(),
  //      dis = 0.0f,
  //    };
  //    //Task.Delay(1).Wait();
  //  }
  //  yield break;
  //}

  public Ball insertBall(GameObject ball_g,BallColor color, Vector3 hitposition){

    GameObject pathball = Instantiate(ball_g, hitposition, Quaternion.identity);
    Transform ball_root = pathball.transform;
    Transform ball_trans = ball_root.Find("ball");
    ball_trans.gameObject.AddComponent<PathBallTag>();
    Ball tmpball = new Ball() {
      id = -1,
      root_trans = ball_root,
      ball_trans = ball_trans,
      isSpwan = false,
      ball_collider = ball_trans.GetComponent<Collider>(),
      dis = 0.0f,
      color = color,
    };
    return tmpball;
  }

}
