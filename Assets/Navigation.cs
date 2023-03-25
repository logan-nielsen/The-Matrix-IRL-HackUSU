using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

public class Navigation : MonoBehaviour
{
    public static bool canAccelerate = true;

    private bool isValid = false;
    private InputDevice targetDevice;
    public XRNode inputSource;
    private Vector2 inputAxis;
    private CharacterController character;

    public float movementSpeed = 3f;
    public float acceleration = 5f;
    public float jumpSpeed = 3f;
    public float fallingSpeed = -9.81f;

    private XROrigin rig;
    private Vector3 velocity;

    void Start()
    {
        GetDevices();
        character = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
    }

    void Update()
    {
        if (!isValid)
        {
            GetDevices();
        }
    }

    private void FixedUpdate()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);

        float actualAcceleration = 0f;
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
        if (triggerValue > 0.1 && canAccelerate)
        {
            actualAcceleration = triggerValue * acceleration;
        }

        targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue);
        if (primaryButtonValue && velocity.y == 0f)
        {
            velocity.y += jumpSpeed;
        }

        Quaternion headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);
        Vector3 movement = direction * Time.fixedDeltaTime * (movementSpeed + actualAcceleration);

        if (character.isGrounded && velocity.y < 0f)
        {
            velocity.y = 0f;
        }
        else
        {
            velocity.y += fallingSpeed * Time.fixedDeltaTime;
            movement += Time.fixedDeltaTime * velocity.y * Vector3.up;
        }

        character.Move(movement);
    }

    void GetDevices()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;

        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            isValid = true;
            targetDevice = devices[0];
        }
    }
}
