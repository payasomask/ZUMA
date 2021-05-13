using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Cinemachine;

public class CameraPathController : MonoBehaviour
{
  public static CameraPathController camerpathcontroller = null;
  // Start is called before the first frame update
  //[SerializeField]
  //private CinemachineBrain BrainCamObj = null;
  [SerializeField]
  private CinemachineFreeLook FreeLookCam = null;
  //[SerializeField]
  //private CinemachineVirtualCamera BackCamObj = null;
  float pathdistance = 0.0f;
  [SerializeField]
  private PathCreation.PathCreator Path = null;
  private float aim_speed = 1.0f;
  GameObject Camera_Aim = null;
  [SerializeField]
  private float rotationpower = 0.5f;
  Cinemachine3rdPersonFollow PersonFollow = null;

  [SerializeField]
  [Range(40,80)]
  private float LookDownMaxAngle = 60.0f;//不能大於180
  [SerializeField]
  [Range(260, 320)]
  private float LookUpMaxAngle = 300.0f;//不能小於180

  private void Awake()
  {
    camerpathcontroller = this;
  }
  //[SerializeField]
  //[Range(0.0f, 5.0f)]
  //private float camera_Xoffset_Factor = 0.01f;
  void Start()
    {
    init();
    }

    // Update is called once per frame
    void Update()
    {
    moveAimOnPath();
    //rotateCamera();
  }

  public void init(){

    if (Camera_Aim == null){
      Camera_Aim = new GameObject("Camera_Aim");
      Camera_Aim.transform.rotation *= Quaternion.AngleAxis(-180, Vector3.right);
    }

    //CinemachineVirtualCamera vc = FrontCamObj.GetComponent<CinemachineVirtualCamera>();
    //vc.Follow = Camera_Aim.transform;

    //AimConstraint camera_aim = CamObj.GetComponent<AimConstraint>();
    //camera_aim.SetSource(0, new ConstraintSource() { sourceTransform = Camera_Aim.transform, weight = 1.0f });

    //just lockAt
    //PersonFollow = FrontCamObj.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
    //vc.LookAt = Camera_Aim.transform;

    //follow aim
    //PersonFollow.CameraDistance = 1.0f;
    //PersonFollow.Damping = new Vector3(0.1f, 0.5f, 0.3f);
    //cineTransposer.m_FollowOffset.z = 1.0f;
    //cineTransposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetNoRoll;

    //just lockAt
    //vc = BackCamObj.GetComponent<CinemachineVirtualCamera>();
    //cineTransposer = BackCamObj.GetCinemachineComponent<CinemachineTransposer>();
    //vc.LookAt = Camera_Aim.transform;

    //follow aim
    //vc.Follow = Camera_Aim.transform;
    //cineTransposer.m_FollowOffset.z = -1.0f;

    if(FreeLookCam != null){
      FreeLookCam.GetRig(0).m_LookAt = Camera_Aim.transform;
      FreeLookCam.GetRig(1).m_LookAt = Camera_Aim.transform;
      FreeLookCam.GetRig(2).m_LookAt = Camera_Aim.transform;
    }

    moveAimOnPath();
  }

  void moveAimOnPath(){
    //if (CamObj == null)
    //  return;
    if (Path == null)
      return;

    //攝影機在切換的時候不能移動aim
    //if (BrainCamObj.IsBlending)
    //  return;


    if (Input.GetKey(KeyCode.E))
    {
      pathdistance += Time.deltaTime * aim_speed;
    }
    else if (Input.GetKey(KeyCode.Q))
    {
      pathdistance -= Time.deltaTime * aim_speed;
    }

    if (pathdistance >= Path.path.length)
      pathdistance = Path.path.length-0.01f;

    if(pathdistance <= 0.0f)
      pathdistance = 0.0f;

    Camera_Aim.transform.position = Path.path.GetPointAtDistance(pathdistance, PathCreation.EndOfPathInstruction.Loop);
  }

  //Quaternion nextRotate;
  float rotateLerp = 0.8f;
  void rotateCamera(){
    //if (FrontCamObj == null)
    //  return;

    float horizomtal = Input.GetAxisRaw("Horizontal");
    float vertical = Input.GetAxisRaw("Vertical");
    Vector3 dir = new Vector3(horizomtal, vertical, 0).normalized;

    //var rotatevector = new Vector3(rotationpower * Input.GetAxis("Mouse X"), rotationpower * Input.GetAxis("Mouse Y"), 0);

    Camera_Aim.transform.rotation *= Quaternion.AngleAxis(dir.x * rotationpower, Vector3.up);
    Camera_Aim.transform.rotation *= Quaternion.AngleAxis(dir.y* rotationpower, Vector3.right);

    var angles = Camera_Aim.transform.localEulerAngles;
    angles.z = 0;
    var angle = Camera_Aim.transform.localEulerAngles.x;

    if (angle > 180 && angle < LookUpMaxAngle)
    {
      angles.x = LookUpMaxAngle;
    }
    else if (angle < 180 && angle > LookDownMaxAngle)
    {
      angles.x = LookDownMaxAngle;
    }

    Camera_Aim.transform.localEulerAngles = angles;

    //nextRotate = Quaternion.Lerp(Camera_Aim.transform.rotation, nextRotate, Time.deltaTime * rotateLerp);

    //Camera_Aim.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
  }

  public GameObject getAim(){
    return Camera_Aim;
  }


}
