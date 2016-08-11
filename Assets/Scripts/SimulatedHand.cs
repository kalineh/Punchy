using UnityEngine;
using System.Collections;

public class SimulatedHand
    : MonoBehaviour
{
    public GameObject hand;

    private float radius;
    private float rotX, rotY;
    private float panX, panY;

    private Vector3 currMouse;
    private Vector3 prevMouse;

    public void Start()
    {
        radius = 1.0f;

        prevMouse = Input.mousePosition;
        currMouse = Input.mousePosition;

        hand = transform.Find("Hand").gameObject;
    }

    public void FixedUpdate()
    {
        currMouse = prevMouse;
        prevMouse = Input.mousePosition;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StopCoroutine("DoInputCode");
            StartCoroutine("DoInputCode");
        }
    }

    // desired normal per branch
    // branch take hit and push, oscillate lerp back to desire
    // rainbow color puddles
    // absorb sponge, put in leaf
    // pulse synth from root, sound on leaves
    // leave size = db
    // leaf color = sound
    // sounds merge from color merge (vol mixer)
    // max 2 sound/color per leaf
    // leaf type alter pulse rate (/2,*2)
    // 

    public IEnumerator DoInputCode()
    {
        var target = Vector3.zero;

        while (true)
        {
            var move = (currMouse - prevMouse);
            var scroll = Input.mouseScrollDelta;
            var enlarge = scroll.y * 10.0f * Time.deltaTime;

            radius += enlarge;
            radius = Mathf.Clamp(radius, 0.1f, 1.2f);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                panX += move.x;
                panY += move.y;
            }
            else
            {
                rotX += move.x;
                rotY += move.y;
            }

            var rot = Quaternion.Euler(rotY, -rotX, 0.0f);
            var pan = new Vector3(panX, panY, 0.0f);
            var dir = rot * Vector3.forward;

            target = dir * radius + pan;

            hand.transform.localPosition = Vector3.Lerp(hand.transform.localPosition, target, 0.2f);

            yield return null;
        }
    }
}

