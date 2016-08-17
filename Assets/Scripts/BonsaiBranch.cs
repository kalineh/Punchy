using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(BonsaiBranch))]
public class BonsaiBranchEditor
    : Editor
{
    private bool showSwayTargetLocal = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var bonsai = target as BonsaiBranch;

        if (GUILayout.Button("Grow"))
            bonsai.MakeBranch();
        if (GUILayout.Button("Shake"))
            bonsai.OnHit(Random.onUnitSphere * 0.1f);

        var wasShowSwayTargetLocal = showSwayTargetLocal;
        showSwayTargetLocal = GUILayout.Toggle(showSwayTargetLocal, "Show Sway");
        if (showSwayTargetLocal && !wasShowSwayTargetLocal)
            bonsai.StartDebugDrawSwayTargetLocal();
        if (!showSwayTargetLocal && wasShowSwayTargetLocal)
            bonsai.StopDebugDrawSwayTargetLocal();
    }
}

#endif

public class BonsaiBranch
    : MonoBehaviour
{
    private GameObject branches;
    private GameObject stalk;
    private GameObject leaf;

    private Rigidbody stalkRigidbody;
    private Rigidbody leafRigidbody;

    private Collider stalkCollider;
    private Collider leafCollider;

    private Renderer stalkRenderer;
    private Renderer leafRenderer;

    private int depth = 0;
    private Vector3 swayTargetOriginal;
    private Vector3 swayTargetLocal;

    public void Start()
    {
        OnScriptReload();
    }

    public void OnScriptReload()
    {
        branches = transform.FindChild("Branches").gameObject;
        stalk = transform.FindChild("Stalk").gameObject;
        leaf = transform.FindChild("Leaf").gameObject;

        stalkRigidbody = stalk.GetComponent<Rigidbody>();
        leafRigidbody = leaf.GetComponent<Rigidbody>();

        stalkCollider = stalk.GetComponent<Collider>();
        leafCollider = leaf.GetComponent<Collider>();

        stalkRenderer = stalk.GetComponent<Renderer>();
        leafRenderer = leaf.GetComponent<Renderer>();

        Physics.IgnoreCollision(stalkCollider, leafCollider);
        //Physics.IgnoreCollision(stalkCollider, transform.parent.parent.FindChild("Leaf").GetComponent<Collider>());

        swayTargetOriginal = Vector3.up * 1.0f;
        swayTargetLocal = swayTargetOriginal;

        //StartCoroutine(DoAutoGrow());
        StartCoroutine(DoDebugInput());
        StartCoroutine(DoSway());
        StartCoroutine(DoSwayReturn());
    }

    public Vector3 FindBestGrowDir()
    {
        var pos = transform.position + transform.up * 0.5f;
        var samples = 12;
        var bestDir = Vector3.up;
        var bestCount = 1000;

        for (int i = 0; i < samples; ++i)
        {
            var dir = Vector3.RotateTowards(Vector3.up, Random.onUnitSphere, 1.25f, 0.0f);
            var ofs = dir * 0.5f;
            var closest = leafCollider.ClosestPointOnBounds(ofs);
            var collisions = Physics.OverlapSphere(pos + ofs, 0.10f);
            var count = collisions.Length;

            //Debug.DrawLine(pos, pos + ofs, Color.Lerp(Color.white, Color.blue, Mathf.Clamp01(count * 0.2f)), 1.0f);

            if (count < bestCount)
            {
                //Debug.DrawLine(pos, pos + ofs, Color.red, 1.0f);
                bestDir = dir;
                bestCount = count;
            }
        }

        return bestDir;
    }

    public GameObject MakeBranch()
    {
        var dir = FindBestGrowDir();
        var src = leafCollider.ClosestPointOnBounds(transform.position + dir * (depth + 1));
        var dst = src + dir * (depth + 1);

        return MakeBranchFromTo(src, dst);
    }

    public GameObject MakeBranchFromTo(Vector3 src, Vector3 dst)
    {
        var depthChild = depth + 1;
        if (depthChild > 20)
            return null;

        var prefab = Resources.Load<GameObject>("BonsaiBranch");
        var obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        var ofs = dst - src;
        var dir = ofs.normalized;
        var len = ofs.magnitude;
        var scale = 1.0f - len * 0.125f;

        scale *= Random.Range(0.8f, 1.2f);

        obj.transform.SetParent(branches.transform);
        obj.transform.position = src;

        obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        obj.transform.localScale = Vector3.one * scale;

        var bonsai = obj.GetComponent<BonsaiBranch>();

        bonsai.name = string.Format("Branch{0}", depthChild);
        bonsai.depth = depthChild;

        return obj;
    }

    public IEnumerator DoAutoGrow()
    {
        var targetScale = transform.localScale;

        transform.localScale = Vector3.zero;

        var shake = transform.DOShakePosition(1.5f, 0.015f);
        yield return transform.DOScale(targetScale, depth * 5.0f).WaitForCompletion();
        shake.Kill();

        transform.DOShakePosition(0.5f, 0.01f);

        if (depth > 0)
        {
            stalkRigidbody.isKinematic = false;
            leafRigidbody.isKinematic = false;
        }

        yield return new WaitForSeconds(Random.Range(1.0f, 5.0f) * depth);

        while (true)
        {
            var depthDelay = depth * 1.0f;
            var grownDelay = branches.transform.childCount * (depth + 1) * 5.0f;

            yield return new WaitForSeconds(depthDelay + grownDelay);

            MakeBranch();

            yield return null;
        }
    }

    public IEnumerator DoDebugInput()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.M))
                MakeBranch();

            yield return null;
        }
    }

    public void OnHit(Vector3 force)
    {
        StartCoroutine(DoSwayHit(force));
    }

    public IEnumerator DoSwayHit(Vector3 force)
    {
        while (force.magnitude > 0.01f)
        {
            swayTargetLocal += force;
            force = Vector3.MoveTowards(force, Vector3.zero, 0.25f * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator DoSwayReturn()
    {
        while (true)
        {
            swayTargetLocal = Vector3.Lerp(swayTargetLocal, swayTargetOriginal, 1.0f * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator DoSway()
    {
        var rotFix = Quaternion.Euler(90.0f, 0.0f, 0.0f);

        //obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        while (true)
        {
            // sway dir is upwards, but rotation is forward

            var srcPos = transform.localPosition;
            var dstPos = transform.localPosition + swayTargetLocal;
            var dir = (dstPos - srcPos).normalized;

            dir = rotFix * dir;

            var srcRot = transform.localRotation;
            var dstRot = Quaternion.LookRotation(dir);
            var rot = Quaternion.Lerp(srcRot, dstRot, 0.1f);

            transform.localRotation = rot;

            yield return null;
        }
    }

    public IEnumerator DoSwayRandom()
    {
        var xs = Random.Range(0.5f, 1.0f);
        var zs = Random.Range(0.5f, 1.0f);
        var xm = Random.Range(10.0f, 20.0f);
        var zm = Random.Range(10.0f, 20.0f);

        var src = transform.localRotation;

        while (true)
        {
            var rand = Quaternion.Euler(
                Mathf.Cos(Time.time * xs) * xm,
                0.0f,
                Mathf.Sin(Time.time * zs) * zm
            );

            var wind = Mathf.Abs(Mathf.Sin(Time.time * 0.2f) * 15.0f);
            var dst = Quaternion.RotateTowards(src, rand, wind);

            transform.localRotation = Quaternion.Lerp(src, dst, 0.2f);

            yield return null;
        }
    }

    public void CutBranch()
    {
        if (transform.parent == null)
            return;

        for (int i = 0; i < branches.transform.childCount; ++i)
        {
            var child = branches.transform.GetChild(i);
            var branch = child.GetComponent<BonsaiBranch>();

            branch.CutBranch();
        }

        StartCoroutine(DoFall());
    }

    public IEnumerator DoFall()
    {
        var body = leaf.GetComponent<Rigidbody>();
        var collider = leaf.GetComponent<Collider>();

        var worldPos = leaf.transform.position;
        var worldRot = leaf.transform.rotation;
        var worldScale = leaf.transform.lossyScale;

        collider.isTrigger = false;

        body.isKinematic = false;
        body.useGravity = true;

        transform.SetParent(null, false);

        leaf.transform.position = worldPos;
        leaf.transform.rotation = worldRot;
        leaf.transform.localScale = worldScale;

        body.MovePosition(worldPos);
        body.MoveRotation(worldRot);

        body.AddForce(Random.onUnitSphere * Random.Range(80.0f, 120.0f) + Vector3.up * 50.0f, ForceMode.Acceleration);

        Destroy(stalk);
        Destroy(branches);

        yield return new WaitForSeconds(5.0f);

        body.isKinematic = true;
        collider.enabled = false;

        Destroy(gameObject);
    }

    public void StartDebugDrawSwayTargetLocal()
    {
        StopCoroutine("DoDebugDrawSwayTargetLocal");
        StartCoroutine("DoDebugDrawSwayTargetLocal");
    }

    public void StopDebugDrawSwayTargetLocal()
    {
        StopCoroutine("DoDebugDrawSwayTargetLocal");
    }

    public IEnumerator DoDebugDrawSwayTargetLocal()
    {
        while (true)
        {
            var src = transform.position;
            var dst = transform.position + swayTargetLocal;

            Debug.DrawLine(src, dst, Color.red);
            Debug.DrawLine(Vector3.zero, Vector3.one, Color.blue);

            yield return null;
        }
    }
}
