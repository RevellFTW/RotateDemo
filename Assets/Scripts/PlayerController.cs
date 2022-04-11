using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    Camera cam;
    public bool isControlable;
    private Vector3 screenPoint;
    private Vector3 offset;
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 1f;
    public float ySpeed = 1f;
    public float scrollspeed = 0.007f;
    public float yMinLimit = 0f;
    public float yMaxLimit = 80f;
    public float distanceMin = 40f;
    public float distanceMax = 130f;
    public float smoothTime = 8;
    public float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;

    float velocityX = 0.0f;
    float velocityY = 0.0f;
    float moveDirection = -1;

    Vector2 startPos;
    RaycastHit hitDown = new RaycastHit();
    Ray rayDown = new Ray();
    RaycastHit hitUp = new RaycastHit();
    Ray rayUp;



    public float scrollVar = 1;
    float lastDist = 0;


    //new input vars
    float zoomAmount = 0;
    bool leftMouseDown = false;
    bool leftMouseClicked = false;
    float rotateXAmount = 0;
    float rotateYAmount = 0;
    bool pressed = false;
    Vector2 pointerPosition;

    Vector2 touchPos1;
    Vector2 touchPos2;
    public void SetControllable(bool value)
    {
        isControlable = value;
    }

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        Vector3 angles = transform.eulerAngles;
        rotationYAxis = (rotationYAxis == 0) ? angles.y : rotationYAxis;
        rotationXAxis = angles.x;

        Rigidbody rigidbody = GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (rigidbody)
        {
            rigidbody.freezeRotation = true;
        }
    }

    private void FixedUpdate()
    {
        var offsetRay = Vector3.zero;

        if (leftMouseClicked)
        {
            startPos = pointerPosition;
            rayDown = Camera.main.ScreenPointToRay(pointerPosition);
            Physics.Raycast(rayDown, out hitDown);
            pressed = true;
        }
        if (!leftMouseDown && pressed == true)
        {
            offsetRay = pointerPosition - startPos;
            if (offsetRay.magnitude < 5)
            {
                rayUp = Camera.main.ScreenPointToRay(pointerPosition);
                Physics.Raycast(rayUp, out hitUp);
                if (hitDown.collider != null && hitUp.collider != null && hitDown.collider.name == hitUp.collider.name)
                {
                    if (Physics.Raycast(rayUp, out hitUp) && hitUp.transform.GetComponent<Renderer>() != null)
                    {
                        var objRenderer = hitUp.transform.GetComponent<Renderer>();
                        objRenderer.material.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    }
                }
            }
            pressed = false;
        }

    }

    void LateUpdate()
    {
        if (!leftMouseDown)
        {
            rotateXAmount = 0;
            rotateYAmount = 0;
        }
        if (target)
        {
            if (leftMouseDown && isControlable)
            {

                velocityX += xSpeed * rotateXAmount * 0.02f;
                velocityY += ySpeed * rotateYAmount * 0.02f;
            }

            if (isControlable)
            {
                distance -= zoomAmount * scrollspeed;
                //pinch zoom 
                /* distance -= touchDist * scrollspeed;*/

                if (distance > distanceMax)
                {
                    distance = distanceMax;
                }
                else if (distance < distanceMin)
                {
                    distance = distanceMin;
                }
            }

            rotationYAxis += velocityX;
            rotationXAxis -= velocityY;

            rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);

            Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
            Quaternion rotation = toRotation;

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            transform.rotation = rotation;
            transform.position = Vector3.Lerp(transform.position, rotation * negDistance + target.position, scrollVar);

            velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
            velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);

            screenPoint = cam.WorldToScreenPoint(target.transform.position);
            offset = target.transform.position - cam.ScreenToWorldPoint(new Vector3(moveDirection * pointerPosition.x, moveDirection * pointerPosition.y, screenPoint.z));
        }

    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        zoomAmount = context.ReadValue<Vector2>().y;
    }
    public void OnRotateToggle(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() == 1)
        {
            leftMouseDown = true;
        }
        else
        {
            leftMouseDown = false;
        }
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        if (leftMouseDown)
        {
            rotateXAmount = context.ReadValue<Vector2>().x;
            rotateYAmount = context.ReadValue<Vector2>().y;
        }
        else
        {
            rotateYAmount = 0;
            rotateXAmount = 0;
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() == 1)
        {
            leftMouseClicked = true;
        }
        if (context.ReadValue<float>() != 1)
        {
            leftMouseClicked = false;
        }
    }
    public void OnPosition(InputAction.CallbackContext context)
    {
        pointerPosition = context.ReadValue<Vector2>();
    }

    public void OnPinch1(InputAction.CallbackContext context)
    {
        touchPos1 = context.ReadValue<Vector2>();
    }

    public void OnPinch2(InputAction.CallbackContext context)
    {
        touchPos2 = context.ReadValue<Vector2>();
        lastDist = Vector2.Distance(touchPos1, touchPos2);
        zoomAmount = lastDist;
    }

   
}