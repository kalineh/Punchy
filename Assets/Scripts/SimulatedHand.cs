using UnityEngine;
using System.Collections;

public class SimulatedHand
    : MonoBehaviour
{
    public GameObject hand;

    private float radius;
    private GameObject shadow;

    private Vector3 currMouse;
    private Vector3 prevMouse;

    public void Start()
    {
        radius = 1.0f;

        prevMouse = Input.mousePosition;
        currMouse = Input.mousePosition;

        hand = transform.Find("Hand").gameObject;

        shadow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shadow.name = name + "Shadow";
        shadow.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 0.1f);
        shadow.GetComponent<Collider>().enabled = false;
        shadow.transform.localScale = Vector3.one * 0.01f;
        shadow.SetActive(false);
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
        var body = hand.GetComponent<Rigidbody>();
        var cam = Camera.main.transform;

        shadow.SetActive(true);
        shadow.transform.position = transform.position;

        while (true)
        {
            var move = (currMouse - prevMouse);
            var scroll = Input.mouseScrollDelta;
            var enlarge = scroll.y * 10.0f * Time.deltaTime;

            radius += enlarge;
            radius = Mathf.Clamp(radius, 0.1f, 1.2f);

            target = cam.position + cam.forward * radius;

            var ofs = target - body.position;
            var dir = ofs.normalized;

            body.AddForce(dir * 7.5f, ForceMode.Acceleration);

            shadow.transform.position = target;

            yield return null;
        }
    }
}

