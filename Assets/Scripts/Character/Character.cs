using Assets.Scripts.Delegates;
using Assets.Scripts.Utils;
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
        [SerializeField] Image fillBar; //TODO - prendere il playerPanel
        [SerializeField] Image tensionBar; //TODO - prendere il playerPanel
        [SerializeField] Canvas LifePanel; //TODO - lo prendiamo da qua?
        Timer timer;
        public float characterTurnTime;

        //la barra della tensione, ma mano che aumenta il tempo del turno diminuisce
        public float tension = 0f;
        //l'ammontare del turno del personaggio: dipende dalla tension
        
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
        //pu� essere sia nemico che player
        public GameObject otherCharacter;
        public bool _isDead;
        public CombatMode combatMode;
        void Awake()
        {
            timer = FindObjectOfType<Timer>();
            if (timer)
            {
                characterTurnTime = timer.GetStandardTurnTime();
            }
        }
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
        }

        //viene chiamato da un evento segnato nell'animazione Gunplay
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

            if (fillBar)
            {
                fillBar.fillAmount -= NormalizedDamage(amount);
            }
            TriggerHitReactionAnimation();
            _Health -= amount;

        }
        public void SetTensionBar(float tensionAmount)
        {
            if (tensionBar)
            {
                tension += tensionAmount;
                tensionBar.fillAmount = Mathf.Clamp(tension, 0, 1);
             
                if (timer)
                {
                    characterTurnTime = Mathf.Clamp((characterTurnTime -= CalculateTensionTimerPercentage(tensionAmount)), 5, timer.totalTurnTime);
                }
            }
        }

        //la percentuale di tempo sottratta in base alla tensione
        float CalculateTensionTimerPercentage(float tensionAmount)
        {
            return (tensionAmount / timer.GetStandardTurnTime()) * 100f;
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
                StartCoroutine(WaitForEndOfShoot(animator.GetCurrentAnimatorStateInfo(0).speed));
            }
            //Se � ememy ricarica e attacca
            else if (weapon && wComponent && wComponent.isEmpty() && GetComponent<AIController>())
            {
                Reload();
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
        void TriggerHitReactionAnimation()
        {
            if (animator)
            {
                animator.SetTrigger("HitReaction");
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

        //I personaggi si girano verso il nemico 
        //e attaccano alla fine della rotazione
        //alla fine dell'animazione dell'attacco 
        //emette un evento intercettato da combatGameMode
        //che fa passare il turno
        public IEnumerator AttackCharacterAtTheEndOfRotation(float lerpTime,
            Character shooter,
            Character receiver)
        {
            otherCharacter = receiver.gameObject;

            float elapsedTime = 0f;

            while (elapsedTime <= lerpTime)
            {
                shooter.transform.rotation = Quaternion.Slerp(
                    shooter.transform.rotation,
                    Quaternion.LookRotation(receiver.gameObject.transform.position - shooter.transform.position, Vector3.up),
                    elapsedTime / lerpTime);

                elapsedTime += (Time.deltaTime * 10f);
                yield return null;
            }

            shooter.Shoot();
            //receiver.TakeDamage(shooter.weapon.GetComponent<Weapon>().damage);
            //StartCoroutine(WaitForEndOfShoot(animator.GetCurrentAnimatorStateInfo(0).speed));
        }

        IEnumerator WaitForEndOfShoot(float delay)
        {
            yield return new WaitForSeconds(delay);

            otherCharacter.GetComponent<Character>().TakeDamage(wComponent.damage);
            EventManager<OnAnimationEnd>.Trigger?.Invoke();

        }
        IEnumerator WaitForEndOfReloading(float delay)
        {
            yield return new WaitForSeconds(delay);
            isReloading = false;

            //Se � enemy attacca nuovamente il player
            if (GetComponent<AIController>())
            {
                StartCoroutine(AttackCharacterAtTheEndOfRotation(2f, this, otherCharacter.GetComponent<Character>()));
            }
        }
    }
}