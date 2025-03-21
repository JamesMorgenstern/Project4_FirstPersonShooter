using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class NewMonoBehaviourScript : MonoBehaviour
{
    //Bullet Properties
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30f;
    public float bulletPrefabLifeTime = 3f;
    
    //Shooting
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;
    
    //Burst
    public int bulletsPerBurst = 3;
    [FormerlySerializedAs("currentBurst")] public int burstBulletsLeft;
    
    //Spread
    public float spreadIntensity;
    
    public GameObject muzzleFlashPrefab;
    private Animator animator;
    
    //Reloading
    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;
    
    //UI
    public TextMeshProUGUI ammoDisplay;

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
    }
 
    
    void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading)
        {
            Reload();
        }
        
        //auto reload when mag is empty
        //if (readyToShoot && !isShooting && !isReloading && bulletsLeft <= 0)
        //{
        //    Reload();
        //}

        if (readyToShoot && isShooting && bulletsLeft > 0)
        {
            burstBulletsLeft = bulletsPerBurst;
            FireWeapon();
        }

        if (AmmoManager.Instance.ammoDisplay != null)
        {
            AmmoManager.Instance.ammoDisplay.text = $"{bulletsLeft / bulletsPerBurst}/{magazineSize / bulletsPerBurst}";
        }
    }

    private void FireWeapon()
    {
        bulletsLeft--;
        
        muzzleFlashPrefab.GetComponent<ParticleSystem>().Play();
        animator.SetTrigger("Recoil");
        
        SoundManager.Instance.shootingSoundM1911.Play();
        
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
        SoundManager.Instance.reloadingSoundM1911.Play();
        
        isReloading = true;
        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted()
    {
        bulletsLeft = magazineSize;
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
}
