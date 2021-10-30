using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Character
{
    public enum CombatMode
    {
        ShootingMode = 1,
        DefenseMode = 2
    }

    public class Character : MonoBehaviour
    {

        [SerializeField] float MinHealth;
        [SerializeField] float MaxHealth;
        [SerializeField] float MaxMana;
        [SerializeField] float _Health;

        [SerializeField] Image FillBar; //TODO -- togliere
        [SerializeField] Canvas LifePanel; //TODO --- lo prendiamo da qua?

        public GameObject weapon;
        Weapon wComponent;

        Animator animator;

        public bool isReloading { get; set; } = false;


        public bool HasAttacked;
        public float Health
        {
            get { return _Health; }
        }
        [SerializeField] float Mana;

        [SerializeField] float PhisicalDefenseValue;
        [SerializeField] float PhisicalAttackValue;

        //può essere sia nemico che player
       public GameObject otherCharacter; 

        public bool _isDead;
        public CombatMode combatMode;

        // Start is called before the first frame update
        void Start()
        {
            animator = gameObject.GetComponent<Animator>();
            if (weapon)
            {
                wComponent = weapon.GetComponent<Weapon>();
            }
           
            combatMode = CombatMode.ShootingMode;

            if (LifePanel)
            {
                //TODO --- MMMM
                LifePanel.transform.Find("Health").Find("MaxHealthValue").GetComponent<Text>().text = MaxHealth.ToString();
                _Health = MaxHealth;
            }

        }

        void Update()
        {
            if (_isDead)
            {
                print("Destroy()");
            }
            //TODO -?
            if (LifePanel)
            {
                LifePanel.transform.Find("Health").Find("HealthValue").GetComponent<Text>().text = Health.ToString("0.0");
            }

            if (otherCharacter)
            {
                
            }

            //if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Reloading"))
            //{
            //    isReloading = false;
            //}
        }

        void PlayShootSound()
        {
            if (wComponent)
            {
                wComponent.PlayShootSound();
            }
        }
        void UseAbility() { }

        public void TakeDamage(float amount)
        {
            if (_Health <= 0) return;

            if (FillBar)
            {
                FillBar.fillAmount -= NormalizedDamage(amount);
            }
            _Health -= amount;
        }


        //In base alla % di difesa del personaggio 
        //determina se posso colpire o se sono in fase di ricarica
        public bool CanHit()
        {
            return wComponent && !wComponent.isEmpty();
        }

        //Il danno convertito nel range 0 - 1
        float NormalizedDamage(float realDamage)
        {
            return (realDamage - MinHealth) / (MaxHealth - MinHealth);
        }

        public void Shoot()
        {
            if (isReloading)
            {
                return;
            }

            if (weapon && wComponent && !wComponent.isEmpty())
            {
                wComponent.Shoot();
                TriggerShootAnimation();
            }
        }
        
        public void Reload()
        {
            isReloading = true;
            wComponent.Reloading();
            TriggerReloadAnimation();
        }
        void TriggerShootAnimation()
        {
            if (animator)
            {
                animator.SetTrigger("Shoot");
            }
        }

        void TriggerReloadAnimation()
        {
            if (animator)
            {
                animator.SetTrigger("Reload");
                StartCoroutine(WaitForEndOfReloading(animator.GetCurrentAnimatorStateInfo(0).speed));
            }
        }

        //Generica per tutti i tipi di animazione?
        IEnumerator WaitForEndOfReloading(float delay)
        {
            yield return new WaitForSeconds(delay);
            isReloading = false;
        }

    }
}