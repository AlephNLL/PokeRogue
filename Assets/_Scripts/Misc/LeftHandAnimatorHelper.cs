using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHandAnimatorHelper : MonoBehaviour
{
    public static LeftHandAnimatorHelper instance;
    public static Action onAnimationEnd;
    [SerializeField] GameObject grabbedObject;
    [SerializeField] public GameObject baseFigureJoint;

    public bool isMoving = false;
    Vector3 defaultPosition;

    public List<GameObject> figuresToFling = new List<GameObject>();

    Vector3 startBaseJointPosition;
    Quaternion startBaseJointRotation;


    private void Awake()
    {
        instance = this;
        onAnimationEnd += ResetRotation;
    }
    public void UnparentGrabbedObject()
    {
        if (grabbedObject == null) return;
        grabbedObject.transform.parent = null;
        grabbedObject = null;
    }

    public void ParentGrabbedObject(GameObject GO)
    {
        grabbedObject = GO;
        grabbedObject.transform.parent = baseFigureJoint.transform;
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
        if (transform.position == destination) return;
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

    public void SetDefaultPosition()
    {
        defaultPosition = transform.position;
    }
    IEnumerator Move(Vector3 destination, float duration)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float percent = Mathf.Clamp01(elapsedTime / duration);

            float acceleratedPercent = percent * percent;

            transform.position = Vector3.Lerp(startPos, destination, acceleratedPercent);

            yield return null;
        }

        transform.position = destination;
        isMoving = false;
    }

    public void RaiseAndShake(Vector3 destination, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ExecuteRaiseAndShake(destination, duration));
    }

    IEnumerator ExecuteRaiseAndShake(Vector3 destination, float duration)
    {
        isMoving = true;

        Vector3 originalStartPos = transform.position;

        float raiseDuration = duration * 0.2f;
        float time = 0f;

        while (time < raiseDuration)
        {
            time += Time.deltaTime;
            float percent = Mathf.Clamp01(time / raiseDuration);
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            transform.position = Vector3.Lerp(originalStartPos, destination, smoothPercent);
            yield return null;
        }
        transform.position = destination;

        float shakeDuration = duration * 0.6f;
        time = 0f;
        Vector3 centerPosition = transform.position;

        float maxShakeStrength = 1.5f;
        float shakeSpeed = 4f;

        while (time < shakeDuration)
        {
            time += Time.deltaTime;
            float percent = Mathf.Clamp01(time / shakeDuration);

            float shakeStrength = Mathf.Lerp(maxShakeStrength, 0f, percent);

            float offsetX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeStrength;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeStrength;
            float offsetZ = (Mathf.PerlinNoise(Time.time * shakeSpeed, Time.time * shakeSpeed) - 0.5f) * shakeStrength;

            transform.position = centerPosition + new Vector3(offsetX, offsetY, offsetZ);
            yield return null;
        }
        transform.position = destination;

        float returnDuration = duration * 0.2f;
        time = 0f;

        while (time < returnDuration)
        {
            time += Time.deltaTime;
            float percent = Mathf.Clamp01(time / returnDuration);
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            transform.position = Vector3.Lerp(destination, originalStartPos, smoothPercent);
            yield return null;
        }

        transform.position = originalStartPos;
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

    public Vector3 offsetFromFigureJoint()
    {
        Vector3 handPos = transform.position;
        Vector3 basePosition = baseFigureJoint.transform.position;

        Vector3 offset = handPos - basePosition;

        return offset;
    }

    private void OnDestroy()
    {
        onAnimationEnd -= ResetRotation;
    }

    public void TryFlick(Unit unit)
    {
       startBaseJointPosition = baseFigureJoint.transform.localPosition;
       startBaseJointRotation = baseFigureJoint.transform.localRotation;

        if (figuresToFling.Count > 0)
        {
            figuresToFling.Add(unit.gameObject);
        }
        else
        {
            figuresToFling.Add(unit.gameObject);
            StartCoroutine(Flick());
        }
    }

    IEnumerator Flick()
    {
        baseFigureJoint.transform.SetLocalPositionAndRotation(startBaseJointPosition, startBaseJointRotation);

        MoveToPosition(figuresToFling[0].transform.position + offsetFromFigureJoint(), .8f);
        while (isMoving) yield return null;
        ParentGrabbedObject(figuresToFling[0]);
        SetHandTriggerParameter("flick");
        yield return new WaitForSeconds(.8f);
        UnparentGrabbedObject();
        figuresToFling[0].GetComponent<Unit>().Death();
        figuresToFling.RemoveAt(0);
        if (figuresToFling.Count > 0)
        {
            StartCoroutine(Flick());
            yield break;
        }
        else
        {
            yield return new WaitForSeconds(.8f);
            MoveToDefaultPosition(1f);
        }
    }
}
