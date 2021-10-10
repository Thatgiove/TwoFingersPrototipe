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
        [SerializeField] Canvas CharacterStatsCanvas; //TODO --- lo prendiamo da qua?

        public GameObject weapon;
        Weapon wComponent;

        Animator animator;

        bool isReloading = false;


        public bool HasAttacked;
        public float Health
        {
            get { return _Health; }
        }
        [SerializeField] float Mana;

        [SerializeField] float PhisicalDefenseValue;
        [SerializeField] float PhisicalAttackValue;

        [SerializeField] Character OtherCharacter;
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

            if (CharacterStatsCanvas)
            {
                //TODO --- MMMM
                CharacterStatsCanvas.transform.Find("Health").Find("MaxHealthValue").GetComponent<Text>().text = MaxHealth.ToString();
                _Health = MaxHealth;
            }

        }

        // Update is called once per frame

        void Update()
        {
            if (_isDead)
            {
                print("Destroy()");
            }
            //TODO -?
            if (CharacterStatsCanvas)
            {
                CharacterStatsCanvas.transform.Find("Health").Find("HealthValue").GetComponent<Text>().text = Health.ToString("0.0");
            }

        }

        void UseAbility() { }

        public void TakeDamage(float amount)
        {
            //print(amount);
            if (_Health <= 0) return;

            if (FillBar)
            {
                FillBar.fillAmount -= NormalizedDamage(amount);
            }
            _Health -= amount;
        }


        //In base alla % di difesa del personaggio 
        //determina se posso colpire
        bool CanHit()
        {
            return false;
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
            else
            {
                isReloading = true;
                wComponent.Reload();
                TriggerReloadAnimation();
            }
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
                StartCoroutine(WaitForEndOfReloading());
            }
        }

        //Generica per tutti i tipi di animazione?
        IEnumerator WaitForEndOfReloading()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            isReloading = false;
        }

    }
}