using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Weapon : MonoBehaviour
{
    public bool isActiveWeapon;
    
    [Header("Bullet Properties")]
    //Bullet Properties
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30f;
    public float bulletPrefabLifeTime = 3f;
    
    [Header("Shooting")]
    //Shooting
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;
    
    [Header("Burst")]
    //Burst
    public int bulletsPerBurst = 3;
    [FormerlySerializedAs("currentBurst")] public int burstBulletsLeft;
    
    [Header("Spread")]
    //Spread
    public float spreadIntensity;
    public float hipSpreadIntensity;
    public float adsSpreadIntensity;
    
    public GameObject muzzleFlashPrefab;
    internal Animator animator;
    
    [Header("Reloading")]
    //Reloading
    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;
    
    //UI
    public TextMeshProUGUI ammoDisplay;
    
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    private bool isADS;

    public enum WeaponModel
    {
        M1911,
        M4
    }

    public WeaponModel thisWeaponModel;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public ShootingMode CurrentShootingMode;

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();
        
        bulletsLeft = magazineSize;
        
        spreadIntensity = hipSpreadIntensity;
    }
 
    
    void Update()
    {
        if (isActiveWeapon)
        {
            if (Input.GetMouseButtonDown(1))
            {
                EnterADS();
            }
            
            if (Input.GetMouseButtonUp(1))
            {
                ExitADS();
            }
            
            GetComponent<Outline>().enabled = false;
            
            if (bulletsLeft == 0 && isShooting)
            {
                SoundManager.Instance.emptyMagazineSoundM1911.Play();
            }
        
        
            if (CurrentShootingMode == ShootingMode.Auto)
            {
                //Holding down left mouse button
                isShooting = Input.GetKey(KeyCode.Mouse0);
            }
            else if (CurrentShootingMode == ShootingMode.Single || CurrentShootingMode == ShootingMode.Burst)
            {
                //Clicking left mouse button
                isShooting = Input.GetKeyDown(KeyCode.Mouse0);
            }

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading && WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
            {
                Reload();
            }
        
            //auto reload when mag is empty
            //if (readyToShoot && !isShooting && !isReloading && bulletsLeft <= 0)
            //{
            //    Reload();
            //}

            if (readyToShoot && isShooting && bulletsLeft > 0 && !isReloading)
            {
                burstBulletsLeft = bulletsPerBurst;
                FireWeapon();
            }
            
        }
    }

    private void EnterADS()
    {
        animator.SetTrigger("enterADS");
        isADS = true;
        HUDManager.Instance.crosshair.SetActive(false);
        spreadIntensity = adsSpreadIntensity;
    }

    private void ExitADS()
    {
        animator.SetTrigger("exitADS");
        isADS = false;
        HUDManager.Instance.crosshair.SetActive(true);
        spreadIntensity = hipSpreadIntensity;
    }

    private void FireWeapon()
    {
        bulletsLeft--;
        
        muzzleFlashPrefab.GetComponent<ParticleSystem>().Play();

        if (isADS)
        {
            animator.SetTrigger("Recoil_ADS");
        }
        else
        {
            animator.SetTrigger("Recoil");
        }
        
        SoundManager.Instance.PlayShootingSound(thisWeaponModel);
        
        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;
        
        //Instantiate the bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        //Pointing the bullet to face the shooting direction
        bullet.transform.forward = shootingDirection;
        
        //Shoot the bullet
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
        
        //Destroy the bullet after some time
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        //Checking if we are done shooting
        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }
        
        //BurstMode
        if (CurrentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1) //we already shoot once before this check;
        {
            burstBulletsLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private void Reload()
    {
        SoundManager.Instance.PlayReloadingSound(thisWeaponModel);
        
        animator.SetTrigger("Reload");
        
        isReloading = true;
        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted()
    {
        int bulletsNeeded = magazineSize - bulletsLeft;
        int availableAmmo = WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel);
        int bulletsToReload = Math.Min(bulletsNeeded, availableAmmo);

        bulletsLeft += bulletsToReload;

        WeaponManager.Instance.DecreaseTotalAmmo(bulletsToReload, thisWeaponModel);

        isReloading = false;
    }
    
    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        //Shooting from the middle of the screen to check where we are pointing at
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            //Hitting Something
            targetPoint = hit.point;
        }
        else
        {
            //Shooting at the air
            targetPoint = ray.GetPoint(100);
        }
        
        Vector3 direction = targetPoint - bulletSpawn.position;
        
        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        
        //Return the shooting direction and spread
        return direction + new Vector3(x, y, 0f);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    public void SwitchActiveSlot(int slotNumber)
    {
        if (WeaponManager.Instance.activeWeaponSlot.transform.childCount > 0)
        {
            Weapon currentWeapon = WeaponManager.Instance.activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            currentWeapon.isActiveWeapon = false;
        }
        
        WeaponManager.Instance.activeWeaponSlot = WeaponManager.Instance.weaponSlots[slotNumber];

        if (WeaponManager.Instance.activeWeaponSlot.transform.childCount > 0)
        {
            Weapon newWeapon = WeaponManager.Instance.activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            newWeapon.isActiveWeapon = true;
        }
    }
}
