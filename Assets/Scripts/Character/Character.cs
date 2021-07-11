using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Weapon;
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

        CharacterWeapon _Weapon;
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
            combatMode = CombatMode.ShootingMode;

            _Weapon = new CharacterWeapon();
            _Weapon.Damage = 0.2f;

            if(CharacterStatsCanvas) //TODO --- MMMM
               CharacterStatsCanvas.transform.Find("Health").Find("MaxHealthValue").GetComponent<Text>().text = MaxHealth.ToString();
        }

        // Update is called once per frame

        void Update()
        {
            if (_isDead)
                print("Destroy()");
        }

        public void Attack()
        {
            print("Attack()");
        }
        void UseAbility() { }
        public void TakeDamage(float amount)
        {
            if (_Health <= 0) return;

            if (FillBar)
                FillBar.fillAmount -= NormalizedDamage(_Weapon.Damage);
            
                _Health -= amount;
        }
        public void CalculateDamage()
        {
            TakeDamage(_Weapon.Damage);
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
    }
}