using Assets.Scripts.Character;
using Assets.Scripts.Items;
using System;
using UnityEngine;
public enum ItemCategory
{
    offensive, curative
}
public enum ItemType
{
    health, shield, mana, tension, attack, defense, time
}
[CreateAssetMenu]
[Serializable]
public class DisposableItem : IItem
{
    public float value;  //l'ammontare offensivo o curativo
    public ItemCategory category;
    public ItemType itemType;
    public string animation;
    //TODO usa strategy
    //https://refactoring.guru/design-patterns/strategy
    //https://refactoring.guru/design-patterns/strategy/csharp/example
    public override void Use(GameObject c)
    {
        var character = c.GetComponent<Character>();
        if (!character) return;

        switch (itemType)
        {
            case ItemType.health:
                _ = category == ItemCategory.curative ? character.health += value : character.health -= value;
                break;
            case ItemType.shield:
                break;
            case ItemType.mana:
                break;
            case ItemType.tension:
                break;
            case ItemType.attack:
                break;
            case ItemType.defense:
                break;
            default:
                break;
        }
    }
    public override void TriggerAnimation(GameObject c)
    {
        var character = c.GetComponent<Character>();
        if (character)
        {
            character.TriggerAnimation(animation);
        }
    }

}
