using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Bonsai4Settings))]
public class Bonsai4SettingsEditor
    : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

#endif

public class Bonsai4Settings
    : MonoBehaviour
{
    public static Bonsai4Settings Get(string name)
    {
        var resource = Resources.Load<GameObject>(name);
        var obj = GameObject.Instantiate(resource);
        return obj.GetComponent<Bonsai4Settings>();
    }

    public static Bonsai4Settings Lerp(Bonsai4Settings src, Bonsai4Settings dst, float t)
    {
        var obj = new GameObject("Bonsai4Settings");
        var result = obj.AddComponent<Bonsai4Settings>();

        result.LimitForce = Mathf.Lerp(src.LimitForce, dst.LimitForce, t);
        result.LimitTorque = Mathf.Lerp(src.LimitTorque, dst.LimitTorque, t);

        result.MoveForce = Mathf.Lerp(src.MoveForce, dst.MoveForce, t);
        result.MovePower = Mathf.Lerp(src.MovePower, dst.MovePower, t);
        result.MoveLerp = Mathf.Lerp(src.MoveLerp, dst.MoveLerp, t);
        result.TorqueForce = Mathf.Lerp(src.TorqueForce, dst.TorqueForce, t);
        result.TorqueLerp = Mathf.Lerp(src.TorqueLerp, dst.TorqueLerp, t);

        result.BackMoveForce = Mathf.Lerp(src.BackMoveForce, dst.BackMoveForce, t);
        result.BackTorque = Mathf.Lerp(src.BackTorque, dst.BackTorque, t);

        result.Mass = Mathf.Lerp(src.Mass, dst.Mass, t);
        result.Drag = Mathf.Lerp(src.Drag, dst.Drag, t);
        result.AngularDrag = Mathf.Lerp(src.AngularDrag, dst.AngularDrag, t);

        return result;
    }

    public float LimitForce;
    public float LimitTorque;

    public float MoveForce;
    public float MovePower;
    public float MoveLerp;
    public float TorqueForce;
    public float TorqueLerp;

    public float BackMoveForce;
    public float BackTorque;

    public float Mass;
    public float Drag;
    public float AngularDrag;
}
