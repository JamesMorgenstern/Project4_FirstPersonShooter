using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    [FormerlySerializedAs("shootingSoundM1911")] public AudioSource shootingChannel;
    public AudioSource reloadingSoundM1911;
    public AudioSource emptyMagazineSoundM1911;
    public AudioSource reloadingSoundM4;

    public AudioClip M1911Shot;
    public AudioClip M4Shot;

    public AudioSource throwablesChannel;
    public AudioClip grenadeSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayShootingSound(Weapon.WeaponModel weapon)
    {
        switch (weapon)
        {
            case Weapon.WeaponModel.M1911:
                shootingChannel.PlayOneShot(M1911Shot);
                break;
            case Weapon.WeaponModel.M4:
                shootingChannel.PlayOneShot(M4Shot);
                break;
        }
    }

    public void PlayReloadingSound(Weapon.WeaponModel weapon)
    {
        switch (weapon)
        {
            case Weapon.WeaponModel.M1911:
                reloadingSoundM1911.Play();
                break;
            case Weapon.WeaponModel.M4:
                reloadingSoundM4.Play();
                break;
        }
    }
}
