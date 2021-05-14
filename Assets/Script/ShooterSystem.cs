using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ShooterSystem : MonoBehaviour
{
  enum Status{
    Roload,
    Roload_Done
  }
  [SerializeField]
  GameObject mainCamer = null;

  [SerializeField]
  private Vector3 StartPosition = new Vector3(0.11f,-0.08f,0.37f);
  [SerializeField]
  private float fire_Speed = 0.5f;

  [SerializeField]
  private List<GameObject> bulletprefab_list = new List<GameObject>();

  private float reloadtimer =0.0f;
  int currentbulletindex = 0;

  Status mcurrentstatus = Status.Roload;

  Bullet current_bullet = null;
  // Start is called before the first frame update
  void Start(){
    Reload();
    }

    // Update is called once per frame
    void Update(){

    resetbullettransform();

    if (Input.GetKeyDown(KeyCode.Space)){
      fire();
    }

    if(mcurrentstatus == Status.Roload_Done){
      return;
    }

    reloadtimer += Time.deltaTime;
    float reloadtime = MoveBallSystem.moveballSystem.getInsertTime();
    if (reloadtimer >= reloadtime){
      reloadtimer = 0.0f;
      Reload();
    }

    }

  void Reload(){
    if (mainCamer == null)
      return;

    if (mcurrentstatus == Status.Roload_Done)
      return;

    Bullet bullet = SpwanBullet();
    bullet.transform.SetParent(mainCamer.transform);
    bullet.transform.localPosition = StartPosition;
    bullet.transform.localScale *= bullet.getReloadScale();
    bullet.gameObject.SetActive(true);
    current_bullet = bullet;

    mcurrentstatus = Status.Roload_Done;
  }

  Bullet SpwanBullet(){
    BallColor color = (BallColor)(currentbulletindex % bulletprefab_list.Count);
    Transform bullet_root = Instantiate(bulletprefab_list[(int)color]).transform;
    Bullet bullet = bullet_root.gameObject.GetComponent<Bullet>();
    bullet.gameObject.SetActive(false);
    currentbulletindex++;
    return bullet;
  }

  //IEnumerable<Bullet> GetbulletArr(int need)
  //{
  //  for (int index = 0; index < need; index++)
  //  {
  //    Transform bullet_root = Instantiate(bulletprefab_list[currentbulletindex % bulletprefab_list.Count]).transform;
  //    Bullet bullet = bullet_root.gameObject.GetComponent<Bullet>();
  //    bullet.gameObject.SetActive(false);
  //    yield return bullet;
  //    //Task.Delay(1).Wait();
  //  }
  //  yield break;
  //}

  void resetbullettransform(){
    if (current_bullet == null)
      return;

    current_bullet.transform.localPosition = StartPosition;
    //current_bullet.transform.localRotation = new Quaternion();
  }

  void fire(){
    if (mcurrentstatus == Status.Roload)
      return;
    //RaycastHit[] rayhit = new RaycastHit;
    //RaycastHit[] rayhit = Physics.RaycastAll(mainCamer.transform.position, mainCamer.transform.forward,100.0f);

    //Vector3 dir = mainCamer.transform.forward;
    //foreach (var v in rayhit){
    //  if(v.collider.TryGetComponent<PathBallTag>(out PathBallTag pbt)) {

    //    break;
    //  }
    //}

    GameObject aim = CameraPathController.camerpathcontroller.getAim();

    //Physics.Raycast(mainCamer.transform.position, mainCamer.transform.forward, out rayhit);
    //Debug.DrawLine(mainCamer.transform.position, mainCamer.transform.forward * 5, Color.blue, 5.0f);
    
    Vector3 dir = (aim.transform.position- current_bullet.transform.position).normalized;

    Debug.DrawLine(current_bullet.transform.position, current_bullet.transform.position + dir * 5, Color.red, 5.0f);
    current_bullet.transform.SetParent(null);
    current_bullet.setup(dir);
    current_bullet = null;

    mcurrentstatus = Status.Roload;
  }
}
