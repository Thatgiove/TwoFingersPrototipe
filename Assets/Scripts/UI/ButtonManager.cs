using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] Button AttackButton;
    [SerializeField] Button SelectWeaponButton;
    [SerializeField] Button ItemsButton;
    [SerializeField] Button MagicButton;
    [SerializeField] Button CoverButton;

    void Start()
    {
        AttackButton.GetComponent<Button>().onClick.AddListener(AttackButtonClick);
        SelectWeaponButton.GetComponent<Button>().onClick.AddListener(SelectWeaponButtonClick);
        ItemsButton.GetComponent<Button>().onClick.AddListener(ItemsButtonClick);
        MagicButton.GetComponent<Button>().onClick.AddListener(MagicButtonClick);
        CoverButton.GetComponent<Button>().onClick.AddListener(CoverButtonClick);
    }

    void AttackButtonClick()
    {
        Debug.Log("AttackButtonClick!");
    }
    void SelectWeaponButtonClick()
    {
        Debug.Log("SelectWeaponButtonClick");
    }
    void ItemsButtonClick()
    {
        Debug.Log("ItemsButtonClick!");
    } 
    void MagicButtonClick()
    {
        Debug.Log("MagicButtonClick!");
    } void CoverButtonClick()
    {
        Debug.Log("CoverButtonClick!");
    }

}
