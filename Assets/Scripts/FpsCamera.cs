using UnityEngine;
using System.Collections;

public class FpsCamera
    : MonoBehaviour
{
    void Update()
    {
        var look_rate = new Vector2(8.0f, 8.0f) * Time.deltaTime * Mathf.Rad2Deg;
        var look = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        look = Vector2.Scale(look, look_rate);

        var move_rate = new Vector3(1.0f, 1.0f, 1.0f) * Time.deltaTime;
        var move = Vector3.zero;

        var speed = 7.0f;
        var fall = 0.05f;

        var forward = Vector3.Scale(transform.forward, new Vector3(1.0f, 0.0f, 1.0f)).normalized;
        var right = Vector3.Scale(transform.right, new Vector3(1.0f, 0.0f, 1.0f)).normalized;

        if (Input.GetKey(KeyCode.W)) move += forward * +speed;
        if (Input.GetKey(KeyCode.A)) move += right * -speed;
        if (Input.GetKey(KeyCode.S)) move += forward * -speed;
        if (Input.GetKey(KeyCode.D)) move += right * +speed;

        Debug.Log(move);

        move = Vector3.Scale(move, move_rate);

        var rigidBody = gameObject.GetComponent<Rigidbody>();
        var force = move * 75.0f;

        // extra fall because gravity not enough
        rigidBody.AddForce(-Vector3.up * fall, ForceMode.Acceleration);
        rigidBody.AddForce(force, ForceMode.Acceleration);

        //rigidBody.velocity = VectorExtension.ClampMagnitudeXY(rigidBody.velocity, 4.0f);

        var cursorMode = !Input.GetKey(KeyCode.LeftShift);

        Cursor.visible = cursorMode;
        Cursor.lockState = cursorMode ? CursorLockMode.None : CursorLockMode.Locked;

        if (!cursorMode)
        {
            var angles = transform.rotation.eulerAngles;
            var delta = new Vector3(-look.y, look.x, 0.0f);
            var range = 65.0f;

            angles += delta;

            while (angles.x < 0.0f)
                angles.x += 360.0f;
            if (angles.x < 180.0f)
                angles.x = Mathf.Min(angles.x, range);
            if (angles.x > 180.0f)
                angles.x = Mathf.Max(angles.x, 360.0f - range);

            transform.rotation = Quaternion.Euler(angles);
        }

        Camera.main.transform.position = transform.position + Vector3.up * 0.5f;
        Camera.main.transform.rotation = transform.rotation;
    }
}
