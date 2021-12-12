using System;
using UnityEngine;
public enum ItemCategory
{
    offensive, curative
}
public enum ItemType
{
    health, shield, mana
}
[CreateAssetMenu]
[Serializable]
public class Item : ScriptableObject
{
    public int id;
    public string name;
    public string description;
    public ItemCategory category;
    public ItemType type;
    //l'ammontare offensivo o curativo
    public float value; 

    public float buyValue;
    public float sellValue;

    public int priority;
}
