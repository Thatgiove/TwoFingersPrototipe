using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public float damage = 0.5f;
    int ammo = 0;
    int totalAmmo = 100; //dipende dagli oggetti
    int magazine = 10; //caricatore
    
    [SerializeField] Canvas WeaponCanvas;

    //TODO implementare modalità di fuoco 
    //semiautomatica -> migliora la precisione raffica di 3
    //automicatica svuota tutto il caricatore -- meno precisa 

    void Start()
    {
        ammo = magazine;
    }
    void Update()
    {
        if (WeaponCanvas)
        {
            WeaponCanvas.transform.Find("Ammo").GetComponent<Text>().text = ammo.ToString("000");
            WeaponCanvas.transform.Find("TotalAmmo").GetComponent<Text>().text = totalAmmo.ToString("000");
        }
    }
    public void Shoot()
    {
        if (!isEmpty())
        {
            ammo -= 3;
        }

    }
    public bool isEmpty()
    {
        return ammo <= 0;
    }

    public void Reloading()
    {
        if(ammo <= 0)
        {
            totalAmmo -= magazine;
        }
        else
        {
            var difference = (magazine - ammo);
            totalAmmo -= difference;
        }

        ammo = magazine;

    }
}

