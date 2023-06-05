using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class DataCollectionCarController : MonoBehaviour
{
    // 1. automation sequence
    // loccodelist: location code for up_left, up_right, down_left, down_right and center
    // rotcodelist_edge: when located on the edge, the rotation code
    // rotcodelist_center: when located on the center, the rotation code
    private static CarPosition[] loccodelist = { CarPosition.UP_LEFT, CarPosition.UP_RIGHT, CarPosition.DOWN_LEFT, CarPosition.DOWN_RIGHT, CarPosition.CENTER };
    private static CarDirection[] rotcodelist_edge = { CarDirection.STRAIGHT, CarDirection.STRAIGHT_LEFT_10, CarDirection.STRAIGHT_LEFT_20, CarDirection.STRAIGHT_LEFT_30, CarDirection.STRAIGHT_RIGHT_10, CarDirection.STRAIGHT_RIGHT_20, CarDirection.STRAIGHT_RIGHT_30 };
    private static CarDirection[] rotcodelist_center = { CarDirection.STRAIGHT, CarDirection.STRAIGHT_LEFT_10, CarDirection.STRAIGHT_LEFT_20,
                                                         CarDirection.STRAIGHT_LEFT_30, CarDirection.STRAIGHT_LEFT_40, CarDirection.STRAIGHT_LEFT_50, 
                                                         CarDirection.STRAIGHT_LEFT_60, CarDirection.STRAIGHT_LEFT_70, CarDirection.STRAIGHT_LEFT_80,
                                                         CarDirection.STRAIGHT_LEFT_90, CarDirection.STRAIGHT_LEFT_100, CarDirection.STRAIGHT_LEFT_110,
                                                         CarDirection.STRAIGHT_LEFT_120, CarDirection.STRAIGHT_LEFT_130, CarDirection.STRAIGHT_LEFT_140,
                                                         CarDirection.STRAIGHT_LEFT_150, CarDirection.STRAIGHT_LEFT_160, CarDirection.STRAIGHT_LEFT_170,
                                                         CarDirection.STRAIGHT_RIGHT_10, CarDirection.STRAIGHT_RIGHT_20, CarDirection.STRAIGHT_RIGHT_30,
                                                         CarDirection.STRAIGHT_RIGHT_40, CarDirection.STRAIGHT_RIGHT_50, CarDirection.STRAIGHT_RIGHT_60,
                                                         CarDirection.STRAIGHT_RIGHT_70, CarDirection.STRAIGHT_RIGHT_80, CarDirection.STRAIGHT_RIGHT_90,
                                                         CarDirection.STRAIGHT_RIGHT_100, CarDirection.STRAIGHT_RIGHT_110, CarDirection.STRAIGHT_RIGHT_120,
                                                         CarDirection.STRAIGHT_RIGHT_130, CarDirection.STRAIGHT_RIGHT_140, CarDirection.STRAIGHT_RIGHT_150,
                                                         CarDirection.STRAIGHT_RIGHT_160, CarDirection.STRAIGHT_RIGHT_170, CarDirection.STRAIGHT_RIGHT_180 };
    // 1. Camara
    // one in the front and another in the back
    public Camera front_camera_perspective;
    public Camera front_camera_orthographic;
    public Camera third_person_camera;

    // 2. configuration of the car (model)
    // rigidbody for the car
    public Rigidbody my_rigidbody;
    // two wheel colliders
    public WheelCollider Wheel_Left_Collider;
    public WheelCollider Wheel_Right_Collider;
    // two wheel model corresponding to four wheel colliders
    public Transform Wheel_Left_Model;
    public Transform Wheel_Right_Model;

    // 3. configuration of the world (car parameter)
    // speed of the car
    private float speed;
    // max angle
    private float maxAngle = 45f;
    // current angle
    private float currentAngle;
    // angle speed
    private float angleSpeed = 30f;
    // omega of the wheel
    private float omega = 0.0f;
    // ratio (unity unit to real life)
    private float ratio_to_reality = 1.0f / 96.0f;

    // 4. UI document
    public UIDocument ui;
    private Label _power_display;
    private Label _speed_display;
    private Label _height_display;
    private Label _terrainID_display;

    // 5. utility for calculating powers
    // calculator
    private VibrationCalculator vc;
    // sampling utility
    private DataCollectionSampling sp;
    // requester: for communication between unity and python server
    private Requester requester;
    // listening on this queue, to see if there is an update
    // if so, update in main ui thread
    private readonly ConcurrentQueue<Action> messageQueue = new ConcurrentQueue<Action>();

    // list is locked or not
    private bool islocked = false;
    // direction is changing on the edge (may cause inaccuracy if the sampling point takes one process to another process)?
    private bool isChanging = true;
    // list to store all power and calculate average
    private List<float> powerlist = new List<float>();
    // data collector
    private DataCollector dc;
    // terrain ID
    private string terrainID;
    // projection of the camera;
    private string projection;

    // Start is called before the first frame update
    void OnEnable()
    {
        // initialize configuration
        ReadConfig.initializeConfig();

        // setting the terrain
        TerrainSetting.getTerrainSetting().setTerrain(Terrain.activeTerrain);
        TerrainSetting.getTerrainSetting().sethmWidth(Terrain.activeTerrain.terrainData.heightmapResolution);
        TerrainSetting.getTerrainSetting().sethmHeight(Terrain.activeTerrain.terrainData.heightmapResolution);
        TerrainSetting.getTerrainSetting().setmResolution(Terrain.activeTerrain.terrainData.heightmapResolution);
        TerrainSetting.getTerrainSetting().setmFlipVertically(false);
        // begin setting the terrain
        TerrainSetting.getTerrainSetting().setHeightmap();
        // setting the terrainID
        ReadConfig.setTerrainID(Path.GetFileNameWithoutExtension(TerrainSetting.getTerrainSetting().getFilePath()));

        // configure the car
        speed = 10f;
        my_rigidbody = gameObject.GetComponent<Rigidbody>();
        currentAngle = my_rigidbody.rotation.eulerAngles.y;
        my_rigidbody.isKinematic = true;
        Wheel_Left_Collider.isTrigger = false;
        Wheel_Right_Collider.isTrigger = false;

        // configure the location and direction of the car
        PosAndDirUtility.UP_LEFT_POS.y = currentHeightY(PosAndDirUtility.UP_LEFT_POS, Wheel_Left_Collider);
        PosAndDirUtility.UP_RIGHT_POS.y = currentHeightY(PosAndDirUtility.UP_RIGHT_POS, Wheel_Left_Collider);
        PosAndDirUtility.DOWN_LEFT_POS.y = currentHeightY(PosAndDirUtility.DOWN_LEFT_POS, Wheel_Left_Collider);
        PosAndDirUtility.DOWN_RIGHT_POS.y = currentHeightY(PosAndDirUtility.DOWN_RIGHT_POS, Wheel_Left_Collider);
        PosAndDirUtility.CENTER_POS.y = currentHeightY(PosAndDirUtility.CENTER_POS, Wheel_Left_Collider);

        // initialize all the text for displaying purposes
        _power_display = ui.rootVisualElement.Q<GroupBox>("information").Q<Label>("power_indicator");
        _speed_display = ui.rootVisualElement.Q<GroupBox>("information").Q<Label>("speed_indicator");
        _height_display = ui.rootVisualElement.Q<GroupBox>("information").Q<Label>("height_indicator");
        _terrainID_display = ui.rootVisualElement.Q<GroupBox>("information").Q<Label>("terrainID_indicator");
        _power_display.text = "Power: 0.0mW";
        _speed_display.text = "Speed: " + String.Format("{0:N1}", my_rigidbody.velocity.magnitude * ratio_to_reality) + "m/s";
        _height_display.text = "Height: " + center(Wheel_Left_Collider, Wheel_Right_Collider) + "(wheel)";
        _terrainID_display.text = "TerrainID: " + Path.GetFileName(TerrainSetting.getTerrainSetting().getFilePath());

        // open the camera on car
        projection = ReadConfig.getProjection();
        if (projection == "perspective")
        {
            front_camera_perspective.enabled = true;
            front_camera_orthographic.enabled = false;
        }
        else if (projection == "orthographic")
        {
            front_camera_perspective.enabled = false;
            front_camera_orthographic.enabled = true;
        }
        third_person_camera.enabled = false;

        // initial location is in upper left
        PosAndDirUtility.initLocation(my_rigidbody.transform);

        terrainID = ReadConfig.getTerrainID();
    }

    private void Start()
    {
        vc = new VibrationCalculator();
        sp = new DataCollectionSampling(Wheel_Left_Collider, Wheel_Right_Collider);
        dc = new DataCollector();
        BatUtility.runBatProcess("run.bat");
        requester = new Requester();
        requester.Start((float Y_approx) => messageQueue.Enqueue(() =>
        {
            vc.setY(Y_approx);
            addPowerItems(vc.Calculate() * 1000);
        }
        ));
    }

    // Update is called once per frame
    void Update()
    {
        // update the display
        _speed_display.text = "Speed: " + String.Format("{0:N2}", my_rigidbody.velocity.magnitude * ratio_to_reality) + "m/s";
        _power_display.text = "Power: " + String.Format("{0:N2}", vc.Calculate() * 1000) + "mW";
        _height_display.text = "Height: " + center(Wheel_Left_Collider, Wheel_Right_Collider) + "(wheel)";

        if (Input.GetKeyUp(KeyCode.E))
        {
            StartCoroutine(saveImage(terrainID));
        }

        if (isChanging)
        {
            sp.Reset();
        } else
        {
            if (!sp.isEnd())
            {
                sp.onSampling();
                // update the current speed of the car
                // which further update the omega of the car
                omega = my_rigidbody.velocity.magnitude / Wheel_Left_Collider.radius;
                vc.setOmega(omega);
            }
            else
            {
                requester.setSamplingData(sp.getJSON());
                sp.Reset();
            }
        }

        if (!messageQueue.IsEmpty)
        {
            Action action;
            while (messageQueue.TryDequeue(out action))
            {
                action.Invoke();
            }
        }

        // WheelRotate();
        WheelModelUpdate(Wheel_Left_Model, Wheel_Left_Collider);
        WheelModelUpdate(Wheel_Right_Model, Wheel_Right_Collider);
    }

    private void OnDestroy()
    {
        requester.Stop();
        BatUtility.runBatProcess("stop.bat");
        BatUtility.KillProjectApplication();
    }

    IEnumerator saveImage(string terrainID)
    {
        // open the camera on car
        Camera front_camera = projection == "perspective" ? front_camera_perspective : front_camera_orthographic ;
        third_person_camera.enabled = false;
        // time for each round
        float timeForEachIteration = ReadConfig.getRuntimeForEachIteration();

        for (int i = 0; i < loccodelist.Length; i++)
        {
            if (i == loccodelist.Length - 1)
            {
                for (int j = 0; j < rotcodelist_center.Length; j++)
                {
                    PosAndDirUtility.changeLocation(my_rigidbody.transform, loccodelist[i]);
                    PosAndDirUtility.changeRotation(my_rigidbody.transform, loccodelist[i], rotcodelist_center[j], false);
                    // adjust the angle for the camera
                    // initialize the angle for the camera
                    front_camera.transform.localEulerAngles = new Vector3(0, 0, 0);
                    Vector3 dst_pos = getDestPos(my_rigidbody.transform, timeForEachIteration * speed);
                    front_camera.transform.localEulerAngles = new Vector3(front_camera.transform.localEulerAngles.x + getCameraAdjustAngle(front_camera.transform.position, dst_pos, timeForEachIteration * speed),
                                                                          front_camera.transform.localEulerAngles.y,
                                                                          front_camera.transform.localEulerAngles.z);
                    DataCollectionCameraUtility.startScreenshot(front_camera, terrainID, Convert.ToString((int)loccodelist[i]), Convert.ToString((int)rotcodelist_center[j]));
                    yield return new WaitForSeconds(0.3f);
                }
            }
            else
            {
                for (int j = 0; j < rotcodelist_edge.Length; j++)
                {
                    PosAndDirUtility.changeLocation(my_rigidbody.transform, loccodelist[i]);
                    PosAndDirUtility.changeRotation(my_rigidbody.transform, loccodelist[i], rotcodelist_edge[j], true);
                    DataCollectionCameraUtility.startScreenshot(front_camera, terrainID, Convert.ToString((int)loccodelist[i]), Convert.ToString((int)rotcodelist_edge[j]));
                    // adjust the angle for the camera
                    // initialize the angle for the camera
                    front_camera.transform.localEulerAngles = new Vector3(0, 0, 0);
                    Vector3 dst_pos = getDestPos(my_rigidbody.transform, timeForEachIteration * speed);
                    front_camera.transform.localEulerAngles = new Vector3(front_camera.transform.localEulerAngles.x + getCameraAdjustAngle(front_camera.transform.position, dst_pos, timeForEachIteration * speed),
                                                                          front_camera.transform.localEulerAngles.y,
                                                                          front_camera.transform.localEulerAngles.z);
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }

        yield return StartCoroutine(saveAveragePower(terrainID));
    }

    IEnumerator saveAveragePower(string terrainID)
    {
        // open the third-party camera
        front_camera_perspective.enabled = false;
        front_camera_orthographic.enabled = false;
        third_person_camera.enabled = true;
        float timeForEachIteration = ReadConfig.getRuntimeForEachIteration();
        string datafile = ReadConfig.getDataFileName();

        for (int i = 0; i<loccodelist.Length; i++)
        {
            if (i == loccodelist.Length-1)
            {
                for (int j=0; j<rotcodelist_center.Length; j++)
                {
                    powerlist.Clear();
                    PosAndDirUtility.changeLocation(my_rigidbody.transform, loccodelist[i]);
                    PosAndDirUtility.changeRotation(my_rigidbody.transform, loccodelist[i], rotcodelist_center[j], false);
                    float sumTime = timeForEachIteration;
                    my_rigidbody.isKinematic = false;
                    my_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    my_rigidbody.velocity = my_rigidbody.transform.forward * this.speed;
                    isChanging = false;
                    while (sumTime > 0)
                    {
                        sumTime--;
                        if (sumTime > 0)
                        {
                            constantSpeed();
                            PosAndDirUtility.remainRotation(my_rigidbody.transform, loccodelist[i], rotcodelist_center[j], false);
                            yield return new WaitForSeconds(1);
                            // addPowerItems(vc.Calculate() * 1000);
                        }
                        else
                        {
                            my_rigidbody.isKinematic = true;
                            my_rigidbody.velocity = my_rigidbody.transform.forward * 0;
                            islocked = true;
                            powerlist.RemoveAll(o => { return o >= 1000.0f; });
                            dc.record(terrainID, i, j, powerlist.Average());
                            islocked = false;
                            isChanging = true;
                            yield return new WaitForSeconds(1);
                        }
                    }
                }
            } else
            {
                for (int j = 0; j<rotcodelist_edge.Length; j++)
                {
                    powerlist.Clear();
                    PosAndDirUtility.changeLocation(my_rigidbody.transform, loccodelist[i]);
                    PosAndDirUtility.changeRotation(my_rigidbody.transform, loccodelist[i], rotcodelist_edge[j], true);
                    float sumTime = timeForEachIteration;
                    my_rigidbody.isKinematic = false;
                    my_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    my_rigidbody.velocity = my_rigidbody.transform.forward * this.speed;
                    isChanging = false;
                    while (sumTime > 0)
                    {
                        sumTime--;
                        if (sumTime > 0)
                        {
                            constantSpeed();
                            PosAndDirUtility.remainRotation(my_rigidbody.transform, loccodelist[i], rotcodelist_edge[j], true);
                            yield return new WaitForSeconds(1);
                            // addPowerItems(vc.Calculate() * 1000);
                        }
                        else
                        {
                            my_rigidbody.isKinematic = true;
                            my_rigidbody.velocity = my_rigidbody.transform.forward * 0;
                            islocked = true;
                            powerlist.RemoveAll(o => { return o >= 1000.0f; });
                            dc.record(terrainID, i, j, powerlist.Average());
                            islocked = false;
                            isChanging = true;
                            yield return new WaitForSeconds(1);
                        }
                    }
                }
            }
        }
        string filePath = Application.streamingAssetsPath + "\\" + datafile;
        dc.SaveCSV(filePath);
    }

    public void addPowerItems(float power)
    {
        if (!islocked)
        {
            Debug.Log("Added: " + power);
            powerlist.Add(power);
        }
    }

    // calculate the height for current terrain
    // so that the wheel can be put on the terrain
    // without sinking into the terrain
    float currentHeightY(Vector3 pos, WheelCollider wheel)
    {
        return Mathf.Abs(wheel.transform.localPosition.y) + wheel.radius + Terrain.activeTerrain.SampleHeight(pos);
    }

    Vector3 getDestPos(Transform car, float distance)
    {
        Vector3 dest_pos;
        dest_pos.x = car.position.x + distance * Mathf.Sin(car.localEulerAngles.y * Mathf.Deg2Rad);
        dest_pos.y = 46.0f;
        dest_pos.z = car.position.z + distance * Mathf.Cos(car.localEulerAngles.y * Mathf.Deg2Rad);

        return new Vector3(dest_pos.x, Terrain.activeTerrain.SampleHeight(dest_pos), dest_pos.z);
    }

    // calculate the height for the lowest point along the way
    // so that the camera can shoot all things
    Vector3 getLowestPos(Transform car, float distance)
    {
        Vector3 dest_pos = car.transform.position;
        float min_height = 256;
        Vector3 final_dest_pos = car.transform.position;
        
        for (int fdis=2; fdis<(int)distance; fdis++)
        {
            
            if (Terrain.activeTerrain.SampleHeight(dest_pos) < min_height)
            {
                min_height = Terrain.activeTerrain.SampleHeight(dest_pos);
                final_dest_pos = new Vector3(dest_pos.x, min_height, dest_pos.z);
            }
        }
        
        return final_dest_pos;
    }

    float getCameraAdjustAngle(Vector3 cur_pos, Vector3 dest_pos, float distance)
    {
        return Mathf.Rad2Deg * Mathf.Atan((cur_pos.y - dest_pos.y) / distance);
    }

    // calculating the center point (in world space)
    // of two wheel collider
    Vector3 center(WheelCollider left, WheelCollider right)
    {
        return new Vector3((left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).x) / 2,
                           (left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).y) / 2,
                           (left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).z) / 2);
    }

    /*void attachGround()
    {
        RaycastHit hit;

        // ground layer
        int Rmask = LayerMask.GetMask("Ground");

        Vector3 Point_dir = transform.TransformDirection(Vector3.down);

        if (Physics.Raycast(transform.position, Point_dir, out hit, 50.0f, Rmask))
        {
            Quaternion NextRot = Quaternion.LookRotation(Vector3.Cross(hit.normal, Vector3.Cross(transform.forward, hit.normal)), hit.normal);
            GetComponent<Rigidbody>().MoveRotation(Quaternion.Lerp(transform.rotation, NextRot, 0.1f));
        }
    }*/
    
    void constantSpeed()
    {
        if (my_rigidbody.velocity.magnitude < this.speed && my_rigidbody.isKinematic == false)
        {
            my_rigidbody.velocity = my_rigidbody.transform.forward * this.speed;
        }
    }

    void WheelRotate()
    {
        // logic of turning left
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            Wheel_Left_Collider.steerAngle -= angleSpeed * Time.deltaTime;
            Wheel_Right_Collider.steerAngle -= angleSpeed * Time.deltaTime;
            currentAngle -= angleSpeed * Time.deltaTime;
            my_rigidbody.transform.localEulerAngles = new Vector3(0, currentAngle, 0);

            // if the steerAngle is bigger than the max angle, set the constraints on angle.
            if (Wheel_Left_Collider.steerAngle < -maxAngle || Wheel_Right_Collider.steerAngle < -maxAngle)
            {
                Wheel_Left_Collider.steerAngle = Wheel_Right_Collider.steerAngle = -maxAngle;
            }
        }

        // logic of turning right
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            Wheel_Left_Collider.steerAngle += angleSpeed * Time.deltaTime;
            Wheel_Right_Collider.steerAngle += angleSpeed * Time.deltaTime;
            currentAngle += angleSpeed * Time.deltaTime;
            my_rigidbody.transform.localEulerAngles = new Vector3(0, currentAngle, 0);

            // if the steerAngle is bigger than the max angle, set the constraints on angle.
            if (Wheel_Left_Collider.steerAngle > maxAngle || Wheel_Right_Collider.steerAngle > maxAngle)
            {
                Wheel_Left_Collider.steerAngle = Wheel_Right_Collider.steerAngle = maxAngle;
            }
        }

        // logic of releasing the turning left or right button
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            Wheel_Left_Collider.steerAngle = Wheel_Right_Collider.steerAngle = 0;
        }
    }

    void WheelModelUpdate(Transform t, WheelCollider wc)
    {
        Quaternion rot = t.rotation;
        Vector3 pos = t.position;
        wc.GetWorldPose(out pos, out rot);
        t.rotation = rot;
        t.position = pos;
    }

    public void updateSpeed(float updateSpeed)
    {
        speed = updateSpeed;
    }

    public void updatelock(bool islocked)
    {
        this.islocked = islocked;
    }
}
