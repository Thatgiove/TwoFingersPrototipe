using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] float MaxHealth;
    [SerializeField] float MaxMana;
    [SerializeField] float Health;
    [SerializeField] float Mana;

    [SerializeField] float PhisicalDefenseValue;
    [SerializeField] float PhisicalAttackValue;

    [SerializeField] Character OtherCharacter;
    bool _isDead;

    // Start is called before the first frame update
    void Start()
    {
        print(gameObject.name + "Health =" + MaxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if(_isDead)
            print("Destroy()");
    }

    void Attack(){}
    void UseAbility(){}
    void TakeDamage(){}

    //In base alla % di difesa del personaggio 
    //determina se posso colpire
    bool CanHit()
    {

        return false;
    }
}
