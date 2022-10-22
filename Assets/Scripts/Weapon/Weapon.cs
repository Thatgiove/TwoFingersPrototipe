using Assets.Scripts.Character;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;
public enum ShootType
{
    OneShoot = 1,
    Automatic,
    SemiAutomatic
}
public class Weapon : MonoBehaviour
{
    
    AudioSource audioSource;
    public float damage = 0.5f;
    public AudioClip shootClip;

    public ShootType shootType;
   
    [SerializeField] int totalAmmo = 100; //dipende dagli oggetti
    [SerializeField] int magazine = 5; //caricatore
    public float timeCost = 1; //tempo da sottrarre al timer
    [SerializeField] Canvas WeaponCanvas;

 
    int ammo = 0;

    int positiveModifier = 0; //le armi posso essere incantate o potenziate ed acquisire dei modificatori
    int negativeModifier = 0;
 
    //TODO implementare modalità di fuoco 
    //semiautomatica -> migliora la precisione raffica di 3
    //automicatica svuota tutto il caricatore -- meno precisa 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
            HandleReloading();
        }

        ActivateReloadText();
    }
    void HandleReloading()
    {
        switch (shootType)
        {
            case ShootType.SemiAutomatic:
                ammo = Mathf.Clamp(ammo -= 3, 0, magazine);
                break;
            case ShootType.Automatic:
                ammo = Mathf.Clamp(ammo -= 1, 0, magazine);
                break;
            default:
                ammo = Mathf.Clamp(ammo -= 1, 0, magazine);
                break;
        }
    }
    public bool isEmpty()
    {
        return ammo <= 0;
    }
    public void PlayShootSound()
    {
        if (audioSource && shootClip)
        {
            //audioSource.PlayOneShot(shootClip);
            AudioSource.PlayClipAtPoint(shootClip, gameObject.transform.position);
        }

    }
    void ActivateReloadText()
    {
        //TODO : valutare se rimuovere la classe enemy e utilizzare i tag
        if (isEmpty() && WeaponCanvas && !Utils.HasComponent<Enemy>(gameObject))
        {
            WeaponCanvas.transform.Find("Reload").gameObject.SetActive(true);
        }
    }
    //TODO : rivedere i calcoli
    public void Reloading()
    {
        if (WeaponCanvas)
        {
            WeaponCanvas.transform.Find("Reload").gameObject.SetActive(false);
        }
        
        if (ammo <= 0)
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
    public float CalculateFinalWeaponAttack()
    {
        return damage + positiveModifier - negativeModifier;
    }
}

