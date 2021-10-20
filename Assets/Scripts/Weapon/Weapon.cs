using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 0.5f;
    public int ammo = 0;
    [SerializeField]  int numberOfShots = 3;
    [SerializeField]  int maxShots = 3;

    private void Start()
    {
        ammo = SetAmmo();
    }
    public void Shoot()
    {
        if (!isEmpty())
        {
            numberOfShots--;
            ammo -= maxShots;
        }
        else
        {
            Reloading();
        }
       
    }
    public bool isEmpty()
    {
        return numberOfShots <= 0;
    }

    public void Reloading()
    {
        numberOfShots = maxShots;
        ammo = SetAmmo();
    }

    int SetAmmo()
    {
        return numberOfShots * numberOfShots;
    }
}

