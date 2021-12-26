using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARLesson : MonoBehaviour
{
    private ARRaycastManager raycastManager;
    private GameObject spawnedObject;
    private ARSessionState state;  // systemStateChanged callback

    [SerializeField] private GameObject PlaceablePrefab;
    [SerializeField] public GameObject ballPrefab;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    
    [SerializeField] ARSession m_Session;

    [SerializeField] private ARCameraManager arCameraManager;
    private Light mainLight;
    public float? Brightness { get; private set; }
    public float? ColorTemperature { get; private set; }
    public Color? ColorCorrection { get; private set; }

    IEnumerator Start()
    {
        if ((ARSession.state == ARSessionState.None) ||
            (ARSession.state == ARSessionState.CheckingAvailability))
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            // Start some fallback experience for unsupported devices
        }
        else
        {
            // Start the AR session
            m_Session.enabled = true;
        }
    }

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        mainLight = GetComponent<Light>();
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;
    }


    // Update is called once per frame
    void Update()
    {

        if (!TryGetTouchPosition(out Vector2 touchPosition))
        {
            return;
        }

        if (raycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;
            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(PlaceablePrefab, hitPose.position, hitPose.rotation);
            }
            else
            {
                spawnedObject.transform.position = hitPose.position;
                spawnedObject.transform.rotation = hitPose.rotation;
            }
        }
    }


    public void ShootBall()
    {
        GameObject newBall = Instantiate<GameObject>(ballPrefab);
        newBall.transform.position = Camera.main.transform.position;
        Rigidbody rb = newBall.GetComponent<Rigidbody>();
        rb.AddForce(5000 * Camera.main.transform.forward);
    }


    private void OnEnable()
    {
        arCameraManager.frameReceived += FrameUpdated;
    }

    private void OnDisable()
    {
        arCameraManager.frameReceived -= FrameUpdated;
    }

    private void FrameUpdated(ARCameraFrameEventArgs args)
    {
        if (args.lightEstimation.averageBrightness.HasValue)
        {
            Brightness = args.lightEstimation.averageBrightness.Value;
            mainLight.intensity = Brightness.Value;
        }

        if (args.lightEstimation.averageColorTemperature.HasValue)
        {
            ColorTemperature = args.lightEstimation.averageColorTemperature.Value;
            mainLight.colorTemperature = ColorTemperature.Value;
        }

        if (args.lightEstimation.colorCorrection.HasValue)
        {
            ColorCorrection = args.lightEstimation.colorCorrection.Value;
            mainLight.color = ColorCorrection.Value;
        }
    }



}
