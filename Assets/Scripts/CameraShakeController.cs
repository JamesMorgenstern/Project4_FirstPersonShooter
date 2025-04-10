using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShakeController : MonoBehaviour
{
    private CinemachineCamera cam;
    private CinemachineBasicMultiChannelPerlin perlinNoise;

    private void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        perlinNoise = cam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        ResetIntensity();
    }

    public void ShakeCamera(float intensity, float shakeTime)
    {
        perlinNoise.AmplitudeGain = intensity;
        StartCoroutine(WaitTime(shakeTime));
    }

    IEnumerator WaitTime(float shakeTime)
    {
        yield return new WaitForSeconds(shakeTime);
        ResetIntensity();
    }

    void ResetIntensity()
    {
        perlinNoise.AmplitudeGain = 0f;
    }

}
