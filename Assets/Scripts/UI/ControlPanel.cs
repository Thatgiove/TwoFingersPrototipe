using Assets.Scripts.Character;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{

    public Character character;
    [SerializeField] GameObject itemButton; //prefab dei bottoni delle combo personaggi - oggetti
    CombatGameMode combatGameMode;

    Transform skillsButton; //il menu dell'attacco nel playerCanvas
    Transform shotBtn; //il menu dell'attacco nel playerCanvas
    Transform charactersScroll; //il menu dei personaggi selezionabili nel playerCanvas

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetCharactersBinding(CombatGameMode cgm) //TODO ????
    {
        combatGameMode = cgm;
        var buttonGrid = transform.Find("buttonsGrid");

        charactersScroll = transform.Find("charactersScroll");
        shotBtn = buttonGrid.transform.Find("shotBtn");
        skillsButton = buttonGrid?.transform.Find("skillsBtn");

        shotBtn?.GetComponent<Button>().onClick.AddListener(ToggleCharacterMenu);
        skillsButton?.GetComponent<Button>().onClick.AddListener(printTHIS);
    }

    void printTHIS()
    {
        print(character.gameObject.name);
    }

    public void SetPlayerImage()
    {
        var pi = GameObject.Find("playerImage");
        if (!pi) return;
        pi.GetComponent<Image>().sprite = character.characterIcon;
    }

    void ToggleCharacterMenu()
    {
        if (combatGameMode.Characters.Length > 0)
        {
            var viewport = charactersScroll.Find("Viewport").Find("Content");
            if (!viewport) return;
            var imgOffset = -12f;

            //Distrugge tutti i bottoni precedenti
            foreach (Transform child in viewport)
            {
                GameObject.Destroy(child.gameObject);
            }

            for (int i = 0; i < combatGameMode.Characters.Length; i++)
            {
                //Prendo solo i nemici 
                if (combatGameMode.Characters[i].GetComponent<AIController>() == null)
                {
                    continue;
                }

                GameObject itBtn = Instantiate(itemButton);
                itBtn.name = i.ToString();
                itBtn.transform.SetParent(viewport.transform);

                var trans = itBtn.GetComponent<RectTransform>();
                trans.localScale = Vector3.one;
                trans.localPosition = new Vector3(87, imgOffset, 0);
                //trans.sizeDelta = new Vector2(100, 24);
                trans.localRotation = Quaternion.identity;

                imgOffset -= 30;//lo abbasso sempre di un po'

                //Imposto il colore il testo dei bottoni 
                var button = itBtn.GetComponent<Button>();
                button.GetComponent<Image>().color = Color.red;
                var itemText = itBtn.transform.Find("itemText");
                if (itemText)
                {
                    itemText.GetComponent<TMP_Text>().text = combatGameMode.Characters[i].name;
                    itemText.GetComponent<TMP_Text>().color = Color.black;

                }

                button.onClick.AddListener(() => character.AttackCharacter(itBtn.name));
            }
        }
    }
}

