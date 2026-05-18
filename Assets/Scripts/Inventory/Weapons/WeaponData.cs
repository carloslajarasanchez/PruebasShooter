using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    [Header("Weapon Stats")]
    public int MaxAmmo = 15;
    public float FireRate = 0.2f;
    public float ReloadTime = 1.5f;
    public float Range = 50f;
    public float Damage = 10f;
    public bool IsAutomatic = false;
    public float HitForce = 5f;

    [Header("Shotgun")]
    public int PelletCount = 1; // mas de 1 para escopetas
    public float SpreadAngle = 0f; // �ngulo de dispersi�n para escopetas
    public WeaponTypeEnum WeaponType;

    [Header("VFX")]
    public GameObject CasingPrefab;
    public Transform CasingEjectPoint;

    [Header("Recoil")]
    public float CameraRecoil = 2f;
    public float RecoilRecoverySpeed = 8f;

}
