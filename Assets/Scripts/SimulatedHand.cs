using UnityEngine;
using System.Collections;

public class SimulatedHand
    : MonoBehaviour
{
    private float radius;
    private GameObject shadow;

    private Vector3 currMouse;
    private Vector3 prevMouse;

    public void Start()
    {
        radius = 1.0f;

        prevMouse = Input.mousePosition;
        currMouse = Input.mousePosition;

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

        if (Input.GetKeyDown(KeyCode.F))
        {
            StopCoroutine("DoPullBranch");
            StartCoroutine("DoPullBranch");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StopCoroutine("DoPunchRay");
            StartCoroutine("DoPunchRay");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StopCoroutine("DoCutBranch");
            StartCoroutine("DoCutBranch");
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            StopAllCoroutines();
        }

        var renderer = GetComponent<Renderer>();
        var touching = GetTouchingLeaf();
        var color = Color.white;

        if (touching != null)
            color = Color.blue;

        renderer.material.color = color;
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

    public GameObject GetTouchingLeaf(float extraRange = 0.25f)
    {
        var touching = Physics.OverlapSphere(shadow.transform.position, extraRange);
        var nearestLen = 10000.0f;
        var nearestObj = (GameObject)null;

        for (int i = 0; i < touching.Length; ++i)
        {
            var obj = touching[i];
            if (!obj.GetComponentInParent<Bonsai4>())
                continue;

            var ofs = obj.transform.position - transform.position;
            var len = ofs.magnitude;

            if (len < nearestLen)
            {
                nearestLen = len;
                nearestObj = obj.gameObject;
            }
        }

        return nearestObj;
    }

    public IEnumerator DoInputCode()
    {
        var target = Vector3.zero;
        var body = GetComponent<Rigidbody>();
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

            body.AddForce(ofs * 20.0f, ForceMode.Acceleration);

            shadow.transform.position = target;

            yield return null;
        }
    }

    public IEnumerator DoPullBranch()
    {
        var nearest = GetTouchingLeaf(0.5f);
        if (nearest == null)
            yield break;

        var pullObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

        pullObj.GetComponent<Collider>().enabled = false;
        pullObj.transform.localScale = Vector3.zero;

        var nearestCollider = nearest.GetComponent<Collider>();
        var nearestPoint = nearestCollider.ClosestPointOnBounds(shadow.transform.position);

        while (true)
        {
            var src = nearestPoint;
            var dst = shadow.transform.position;

            var ofs = (dst - src);
            var dir = ofs.normalized;
            var len = ofs.magnitude;

            pullObj.transform.position = src + ofs * 0.5f;
            pullObj.transform.localScale = new Vector3(0.1f, len * 0.75f, 0.1f);
            pullObj.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0.0f, -90.0f, 90.0f);

            if (!Input.GetKey(KeyCode.F))
                break;

            yield return null;
        }

        var nearestBranch = nearest.GetComponentInParent<Bonsai4>();
        var branchDepth = nearestBranch.depth + 1;

        var settingsT = Mathf.Clamp01(1.0f / 10.0f * (float)branchDepth);
        var settingsSrc = Bonsai4Settings.Get("Bonsai4SettingsSrc");
        var settingsDst = Bonsai4Settings.Get("Bonsai4SettingsDst");
        var settings = Bonsai4Settings.Lerp(settingsSrc, settingsDst, settingsT);

        var branch = Bonsai4.MakeBranch(settings);

        var branchSrc = nearestPoint;
        var branchDst = shadow.transform.position;

        branch.depth = branchDepth;

        Destroy(settingsSrc.gameObject);
        Destroy(settingsDst.gameObject);

        branch.StartCoroutine(branch.DoAttachment(nearestBranch.gameObject, branchSrc, branchDst));

        Destroy(pullObj);

        while (true)
        {
            Debug.DrawLine(branchSrc, branchDst, Color.blue);
            //Debug.DrawLine(shadow.transform.position, nearestPoint, Color.red);
            //Debug.DrawLine(shadow.transform.position, branchDst, Color.green);
            yield return null;
        }
    }

    public IEnumerator DoPunchRay()
    {
        var nearest = GetTouchingLeaf();
        if (nearest == null)
            yield break;

        var body = nearest.GetComponent<Rigidbody>();
        var ofs = (nearest.transform.position - shadow.transform.position);
        var dir = ofs.normalized;
        var force = dir * 10.0f;

        var bonsai = nearest.GetComponentInParent<BonsaiBranch>();

        bonsai.OnHit(force);
    }

    public IEnumerator DoCutBranch()
    {
        var nearest = GetTouchingLeaf();
        if (nearest == null)
            yield break;

        var parent = nearest.transform.parent;
        var bonsai = parent.GetComponent<BonsaiBranch>();

        bonsai.CutBranch();
    }
}

