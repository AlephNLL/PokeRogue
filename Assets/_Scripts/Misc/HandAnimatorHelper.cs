using System;
using UnityEngine;

public class HandAnimatorHelper : MonoBehaviour
{
    public static Action onAnimationEnd;
    [SerializeField] GameObject grabbedObject;
    
    public void UnparentGrabbedObject()
    {
        grabbedObject.transform.parent = null;
    }

    public void OnAnimationEnd()
    {
        onAnimationEnd?.Invoke();
    }
}
