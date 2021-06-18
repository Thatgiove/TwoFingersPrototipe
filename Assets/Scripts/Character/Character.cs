using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Character
{
    public enum CombatMode
    {
        ShootingMode = 1,
        DefenseMode = 2,
        MagicMode = 3
    }

    public class Character : MonoBehaviour
    {

        [SerializeField] float MaxHealth;
        [SerializeField] float MaxMana;
        [SerializeField] float _Health;
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
           
        }

        // Update is called once per frame
        void Update()
        {
            if (_isDead)
                print("Destroy()");
        }

        void Attack() { }
        void UseAbility() { }
        public void TakeDamage(float amount) 
        {
            _Health -= amount;
        }
        public float CalculateDamage()
        {
            return 0.2f;
        }

        //In base alla % di difesa del personaggio 
        //determina se posso colpire
        bool CanHit()
        {

            return false;
        }
    }
}