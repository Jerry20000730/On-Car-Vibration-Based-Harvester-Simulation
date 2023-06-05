using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class CarControllerForPrediction : MonoBehaviour
{
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
    // Camara
    // one in the front and another in the back
    public Camera car_camera;
    public Camera third_person_camera;

    // UI document
    public UIDocument ui;
    private Label _power_display;
    private Label _speed_display;
    private Label _height_display;
    private Label _terrainID_display;

    // arrow image
    public GameObject straight_arrow;
    public GameObject left_arrow;
    public GameObject right_arrow;

    // Configuration of the car (model)
    // rigidbody for the car
    public Rigidbody my_rigidbody;
    // two wheel colliders
    public WheelCollider Wheel_Left_Collider;
    public WheelCollider Wheel_Right_Collider;
    // two wheel model corresponding to four wheel colliders
    public Transform Wheel_Left_Model;
    public Transform Wheel_Right_Model;

    public UniGifImage gifImage;

    // Configuration of the world (car parameter)
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

    // position and direction of the car;
    int loc, rot;

    // isPositionSet indicates whether the user has confirmed the location
    private bool isPositionSet = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        // setting the terrain
        TerrainSetting.getTerrainSetting().setTerrain(Terrain.activeTerrain);
        TerrainSetting.getTerrainSetting().sethmWidth(Terrain.activeTerrain.terrainData.heightmapResolution);
        TerrainSetting.getTerrainSetting().sethmHeight(Terrain.activeTerrain.terrainData.heightmapResolution);
        TerrainSetting.getTerrainSetting().setmResolution(Terrain.activeTerrain.terrainData.heightmapResolution);
        TerrainSetting.getTerrainSetting().setmFlipVertically(false);
        // begin setting the terrain
        TerrainSetting.getTerrainSetting().setHeightmap();

        // configure the car
        speed = 10f;
        my_rigidbody = gameObject.GetComponent<Rigidbody>();
        currentAngle = my_rigidbody.rotation.eulerAngles.y;
        my_rigidbody.isKinematic = true;
        Wheel_Left_Collider.isTrigger = false;
        Wheel_Right_Collider.isTrigger = false;

        // configure the direction
        straight_arrow.GetComponent<CanvasGroup>().alpha = 0f;
        left_arrow.GetComponent<CanvasGroup>().alpha = 0f;
        right_arrow.GetComponent<CanvasGroup>().alpha = 0f;

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
    }

    void Start()
    {
        vc = new VibrationCalculator();
        sp = new DataCollectionSampling(Wheel_Left_Collider, Wheel_Right_Collider);
        BatUtility.runBatProcess("run.bat");
        requester = new Requester();
        requester.Start((float Y_approx) => messageQueue.Enqueue(() =>
        {
            vc.setY(Y_approx);
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

        if (!isPositionSet && Input.GetKeyUp(KeyCode.C))
        {
            loc = UnityEngine.Random.Range(0, 5);
            if (loc == 4)
            {
                rot = UnityEngine.Random.Range(0, 36);
                PosAndDirUtility.changeLocation(my_rigidbody.transform, loccodelist[loc]);
                PosAndDirUtility.changeRotation(my_rigidbody.transform, loccodelist[loc], rotcodelist_center[rot], false);
            } else
            {
                rot = UnityEngine.Random.Range(0, 7);
                PosAndDirUtility.changeLocation(my_rigidbody.transform, loccodelist[loc]);
                PosAndDirUtility.changeRotation(my_rigidbody.transform, loccodelist[loc], rotcodelist_edge[rot], true);
            }
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            StartCoroutine(ViewGifCoroutine());
            straight_arrow.GetComponent<CanvasGroup>().alpha = 0.3f;
            left_arrow.GetComponent<CanvasGroup>().alpha = 0.3f;
            right_arrow.GetComponent<CanvasGroup>().alpha = 0.3f;
            isPositionSet = true;
            if (loc == 4)
            {
                DataCollectionCameraUtility.ScreenShotForPrediction(car_camera, Convert.ToString((int)loccodelist[loc]), Convert.ToString((int)rotcodelist_center[rot]));
            } else
            {
                DataCollectionCameraUtility.ScreenShotForPrediction(car_camera, Convert.ToString((int)loccodelist[loc]), Convert.ToString((int)rotcodelist_edge[rot]));
            }
            Debug.Log("Here!");
        }
        
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
    void OnDestroy()
    {
        requester.Stop();
        BatUtility.runBatProcess("stop.bat");
        BatUtility.KillProjectApplication();
    }

    // calculate the height for current terrain
    // so that the wheel can be put on the terrain
    // without sinking into the terrain
    float currentHeightY(Vector3 pos, WheelCollider wheel)
    {
        return Mathf.Abs(wheel.transform.localPosition.y) + wheel.radius + Terrain.activeTerrain.SampleHeight(pos);
    }

    // calculating the center point (in world space)
    // of two wheel collider
    Vector3 center(WheelCollider left, WheelCollider right)
    {
        return new Vector3((left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).x) / 2,
                           (left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).y) / 2,
                           (left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).z) / 2);
    }

    void WheelModelUpdate(Transform t, WheelCollider wc)
    {
        Quaternion rot = t.rotation;
        Vector3 pos = t.position;
        wc.GetWorldPose(out pos, out rot);
        t.rotation = rot;
        t.position = pos;
    }

    private IEnumerator ViewGifCoroutine()
    {
        yield return StartCoroutine(gifImage.SetGifFromUrlCoroutine(Application.streamingAssetsPath + "\\loading_icon.gif"));
    }
}
