using UnityEngine;
using System.Collections;

public class AxisHelper
    : MonoBehaviour
{
    public static GameObject Create()
    {
        var resource = Resources.Load<GameObject>("AxisHelper");
        var obj = GameObject.Instantiate(resource);

        return obj;
    }
}
