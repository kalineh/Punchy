using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public static class Vector3Extensions
{
    public static Vector3 SafeNormalize(this Vector3 v)
    {
        var lsq = v.sqrMagnitude;
        if (lsq < Vector3.kEpsilon)
            return Vector3.up;

        var len = Mathf.Sqrt(lsq);
        var result = v / len;

        return result;
    }

    public static Vector3 SafeNormalizeOr(this Vector3 v, Vector3 fallback)
    {
        var lsq = v.sqrMagnitude;
        if (lsq < Vector3.kEpsilon)
            return fallback;

        var len = Mathf.Sqrt(lsq);
        var result = v / len;

        return result;
    }

    public static float SafeMagnitude(this Vector3 v)
    {
        var lsq = v.sqrMagnitude;
        if (lsq < Vector3.kEpsilon)
            return 0.0f;

        var result = Mathf.Sqrt(lsq);
        return result;
    }
}
