using Assets.Scripts.Character;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] Button AttackButton;
    [SerializeField] Button CoverButton;
    CombatGameMode combatGameMode;

    void Start()
    {
        combatGameMode = FindObjectOfType<CombatGameMode>();

        //AttackButton.GetComponent<Button>()?.onClick.AddListener(AttackButtonClick);
        CoverButton.GetComponent<Button>().onClick.AddListener(CoverButtonClick);
    }

    //void AttackButtonClick()
    //{
    //    if (combatGameMode)
    //    {
    //        //imposto la visuale sul campo di battaglia 
    //        combatGameMode.cameraManager?.SetFieldCamera();

    //        //setto lo stato del personaggio in modalità attacco
    //        //se clicco su un personaggio, PlayerController scatenerà l'evento 
    //        //SelectCharacter in CombatGameMode
    //        combatGameMode.CharacterInTheTurn.SetActionType(ActionType.Attack);

    //    }
    //}

    void CancelAttackClick() { }

    void CoverButtonClick()
    {
        combatGameMode?.ChangeCombatMode((int)CombatMode.DefenseMode);
    }
}
