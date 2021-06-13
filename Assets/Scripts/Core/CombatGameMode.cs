using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Character;
using Assets.Scripts.Delegates;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;


public class CombatGameMode : MonoBehaviour
{
    [SerializeField] Timer _Timer;
    MainCanvas canvas;
    GameObject[] Characters;
    //TODO togliere public
    //Il personaggio nel campo di battaglia attualmente in turno
    public GameObject CharacterInTheTurn;
    Text _enemyTurnText;
    //TODO unificare le code
    Queue<GameObject> TurnQueue = new Queue<GameObject>();
    Queue<GameObject> CharacterQueue = new Queue<GameObject>();

    void Start()
    {
        CreateTurn();
        EventManager<OnCharacterSelection>.Register(CharacterSelected);
    }

    void Update()
    {
        ///////////////TODO TMP///////////////////////
        if (Input.GetKeyDown("space") && !isPlayerTurn())
                 _Timer.Time_Zero();
        ////////////////////////////////////////////

        if (_Timer && _Timer.isTurnOver && TurnQueue.Count > 0 && CharacterQueue.Count > 0)
        {
            CharacterInTheTurn = null;
            //distrugge il flag della turnazione precedente
            Destroy(TurnQueue.Peek().transform.Find("_TurnIcon").gameObject); 
            
            TurnQueue.Dequeue();
            CharacterQueue.Dequeue();
          
            if (TurnQueue.Count > 0 && CharacterQueue.Count > 0)
            {
                PutIconAtFirstElementOfQueue();
                CharacterInTheTurn = CharacterQueue.Peek();
                HandleInput();
            }
            else
            {
                DestroyTurn();
                CreateTurn();
            }
        }
    }

    void PutIconAtFirstElementOfQueue()
    {
        CharacterInTheTurn = CharacterQueue.Peek();
        HandleInput();

        GameObject _TurnIcon = new GameObject("_TurnIcon");
        Image turnIcon = _TurnIcon.AddComponent<Image>();
        turnIcon.color = Color.red;
        turnIcon.transform.SetParent(TurnQueue.Peek().transform);
        turnIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
        turnIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 50);
    }

    // Prendiamo tutti i personaggi sul campo di battaglia
    // e creiamo un'immagine per ognuno di essi inserendola 
    // nella turnazione
    void CreateTurn()
    {
        float imgOffset = -300f;
        //TODO -- qual è il modo migliore per selezionare il canvas?
        canvas = GameObject.FindObjectOfType<MainCanvas>();
        _enemyTurnText =  canvas.transform.Find("EnemyTurnText").GetComponent<Text>(); //TODO non prendere by name
        _enemyTurnText.enabled = false;

        if (Characters == null)
            Characters = GameObject.FindGameObjectsWithTag("Character");

        var randomizedList = Utils.Randomize(Characters.ToList());

        foreach (GameObject character in randomizedList)
        {
            if (canvas)
            {
                //genera le immagini dei personaggi nel campo di battaglia
                //e le mette nella coda di turni
                GameObject imgObject = new GameObject(character.name);
                //aggiungiamo una classe custom per tipizzare il gameobject
                imgObject.AddComponent<CharacterIcon>();

                RectTransform trans = imgObject.AddComponent<RectTransform>();
                trans.transform.SetParent(canvas.transform);
                trans.localScale = Vector3.one;
                trans.anchoredPosition = new Vector2(imgOffset, 400f);
                trans.sizeDelta = new Vector2(200, 200);

                Image image = imgObject.AddComponent<Image>();
                imgObject.transform.SetParent(canvas.transform);
                imgOffset += 230;

                GameObject text = new GameObject(character.name);

                Text _text = text.AddComponent<Text>();
                _text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                _text.alignment = TextAnchor.MiddleCenter;
                _text.color = Color.black;
                _text.text = character.name;
                _text.fontSize = 22;
                _text.transform.SetParent(image.transform);

                text.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

                //Mette i personaggi nella coda
                TurnQueue.Enqueue(imgObject);
                //TODO : creare una sola coda
                CharacterQueue.Enqueue(character); 
            }
        }
        PutIconAtFirstElementOfQueue();
    }

    void DestroyTurn()
    {
        var charactersInGame= GameObject.FindObjectsOfType<CharacterIcon>();

        foreach (var ele in charactersInGame)
        {
            Destroy(ele.gameObject);
        }
    }

    void HandleInput()
    {
        if (_enemyTurnText)
            _enemyTurnText.enabled = !isPlayerTurn();

        if (!isPlayerTurn())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    bool isPlayerTurn()
    {
        return CharacterInTheTurn && !Utils.HasComponent<Enemy>(CharacterInTheTurn);
    }
    void CharacterSelected(GameObject g)
    {
        Debug.Log(g.name + " " + "Selected");
    }

}
