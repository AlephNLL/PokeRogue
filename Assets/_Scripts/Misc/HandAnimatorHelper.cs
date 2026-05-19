using System;
using System.Collections;
using Unity.Hierarchy;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HandAnimatorHelper : MonoBehaviour
{
    public static HandAnimatorHelper instance;
    public static Action onAnimationEnd;
    [SerializeField] GameObject grabbedObject;
    [SerializeField] GameObject baseFigureJoint;

    public bool isMoving = false;
    Vector3 defaultPosition;

    private void Awake()
    {
        instance = this;
        onAnimationEnd += ResetRotation;
    }
    public void UnparentGrabbedObject()
    {
        grabbedObject.transform.parent = null;
        grabbedObject = null;
    }

    public void ParentGrabbedObject(GameObject GO)
    {
        grabbedObject = GO;
        grabbedObject.transform.parent = baseFigureJoint.transform;
        grabbedObject.transform.localPosition = new Vector3(0, 0, 0);
    }

    void ResetRotation()
    {
        defaultPosition = transform.position + 1.5f * Vector3.up;
        MoveToDefaultPosition(1f);
        StartCoroutine(LerpRotation(Quaternion.Euler(0, 180, 0)));
    }
    IEnumerator LerpRotation(Quaternion rotation)
    {
        Quaternion startRotation = transform.rotation;
        float t = 0;
        float elapsedTime = 0;

        while (t < 1)
        {
            elapsedTime += Time.deltaTime;
            t += elapsedTime * elapsedTime / 2;
            transform.rotation = Quaternion.Lerp(startRotation, rotation, t);
            yield return null;
        }

        transform.rotation = rotation;
    }
    public void OnAnimationEnd()
    {
        onAnimationEnd?.Invoke();
    }

    public void MoveToPosition(Vector3 destination, float duration)
    {
        if (!isMoving)
        {
            StartCoroutine(Move(destination, duration));
            //StartCoroutine(LerpRotation(Quaternion.Euler(90, 180, 0)));
        }
    }
    public void MoveToDefaultPosition(float duration)
    {
        if (!isMoving)
        {
            StartCoroutine(Move(defaultPosition, duration));
            //StartCoroutine(LerpRotation(Quaternion.Euler(0, 180, 0)));
        }
    }
    IEnumerator Move(Vector3 destination, float duration)
    {

        isMoving = true;
        Vector3 startPos = transform.position;
        float t = 0;
        float elapsedTime = 0;

        while (t < duration)
        {
            elapsedTime += Time.deltaTime;
            t += elapsedTime * elapsedTime / 10;
            transform.position = Vector3.Lerp(startPos, destination, t);
            yield return null;
        }

        transform.position = destination;
        isMoving = false;

    }

    public bool IsHandAtXPos(float x)
    {
        if (transform.position.x == x) return true;
        return false;
    }

    public void SetHandBoolParameter(string name, bool value)
    {
        gameObject.GetComponent<Animator>().SetBool(name, value);
    }

    public void SetHandTriggerParameter(string name)
    {
        gameObject.GetComponent<Animator>().SetTrigger(name);
    }

    public void TeleportHandBehindCamera()
    {
        GameObject cameraBrain = GameObject.FindGameObjectWithTag("MainCamera");
        gameObject.transform.position = new Vector3(cameraBrain.transform.position.x, 0.45f, (cameraBrain.transform.position.z)) - new Vector3(3, 0, 1);
    }
}
