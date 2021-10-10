using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 0.3f;
    [SerializeField]  int ammo = 3;
    [SerializeField]  int maxAmmo = 3;

    public void Shoot()
    {
        if (!isEmpty())
        {
            ammo--;
        }
        else
        {
            Reload();
        }
       
    }
    public bool isEmpty()
    {
        return ammo <= 0;
    }

    public void Reload()
    {
        ammo = maxAmmo;
    }
}

