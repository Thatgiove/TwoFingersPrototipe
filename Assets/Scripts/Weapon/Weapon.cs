using Assets.Scripts.Character;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    AudioSource audioSource;
    public float damage = 0.5f;
    int ammo = 0;
    int totalAmmo = 100; //dipende dagli oggetti
    int magazine = 5; //caricatore
    public AudioClip shootClip;
    [SerializeField] Canvas WeaponCanvas;

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
           
            ammo = Mathf.Clamp(ammo -= 3, 0, magazine);
        }

        ActivateReloadText();
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
        WeaponCanvas.transform.Find("Reload").gameObject.SetActive(false);
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
}

