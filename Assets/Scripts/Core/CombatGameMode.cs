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
    GameObject PlayerCanvas;
    public GameObject CharacterSelected;
    Enemy _Enemy; 
    //Il personaggio nel campo di battaglia attualmente in turno
    Character CharacterInTheTurn;
    Text _enemyTurnText;
    //TODO unificare le code
    Queue<GameObject> TurnQueue = new Queue<GameObject>();
    Queue<GameObject> CharacterQueue = new Queue<GameObject>();
    float TimeToAttack; //TODO RIMUOVERE -- stai dando il peggio di te
    bool HasAttacked; //TODO TOGLIERE DA QUA
    void Start()
    { 
        //TODO - non prendere by string
        PlayerCanvas = GameObject.Find("PlayerPanel");
        PlayerCanvas.SetActive(false);
        CreateTurn();
        EventManager<OnCharacterSelection>.Register(SelectCharacter);

       
    }

    void Update()
    {
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
                CharacterInTheTurn = CharacterQueue.Peek().GetComponent<Character>();
                 
                //TODO - check player turn
                if (PlayerCanvas)
                    AttachPlayerCanvas();

                HandleInput();
            }
            else
            {
                DestroyTurn();
                CreateTurn();
            }
        }
        ///////TODO -----------mmm non mi convince per niente
        if (!isPlayerTurn())
            EnemyAttackPlayer();
    }

    //attacca la schermata di comandi a quel player
    void AttachPlayerCanvas()
    {
        if (isPlayerTurn())
        {
            PlayerCanvas.SetActive(true);
            PlayerCanvas.transform.parent = CharacterInTheTurn.gameObject.transform;
            PlayerCanvas.transform.position = CharacterInTheTurn.transform.position;

            Vector3 playerCanvasPos = new Vector3(1.5f, 0, 0);
            
            //TODO -- rotazione del canvas in direzione della camera
            //la camera si pu� muovere
            PlayerCanvas.transform.rotation = transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            PlayerCanvas.transform.position += playerCanvasPos;
        }
        else
            PlayerCanvas.SetActive(false);

    }

    void PutIconAtFirstElementOfQueue()
    {
        CharacterInTheTurn = CharacterQueue.Peek().GetComponent<Character>();
        //TODO Gestire diversamente
        if (PlayerCanvas)
            AttachPlayerCanvas();
       
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
        float imgOffset = -450f;
        //TODO -- qual � il modo migliore per selezionare il canvas?
        canvas = GameObject.FindObjectOfType<MainCanvas>();
        _enemyTurnText = canvas.transform.Find("EnemyTurnText").GetComponent<Text>(); //TODO non prendere by name
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
        var charactersInGame = GameObject.FindObjectsOfType<CharacterIcon>();

        foreach (var ele in charactersInGame)
        {
            Destroy(ele.gameObject);
        }
    }

    //TODO - Cambiare nome tipo handle turn
    void HandleInput()
    {
       
        if (_enemyTurnText)
            _enemyTurnText.enabled = !isPlayerTurn();

        if (!isPlayerTurn())
        {
            _Enemy = CharacterInTheTurn.GetComponent<Enemy>();
            TimeToAttack = _Enemy.CalculateAttackTime(_Timer.totalTurnTime);

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
        return CharacterInTheTurn && !Utils.HasComponent<Enemy>(CharacterInTheTurn.gameObject);
    }

    void EnemyAttackPlayer()
    {
        if (_Timer.timeRemaining <= TimeToAttack)
        {
            _Enemy.Attack();
            _Timer.Time_Zero(); //TODO -- puoi fare di meglio
        }
           
    }

    void SelectCharacter(GameObject charSelected)
    {
        //Il CharacterSelected pu� essere anche un alleato 

        CharacterSelected = charSelected;
        //TODO il combat va tolto da qua
        if (CharacterInTheTurn.combatMode == CombatMode.ShootingMode)
        {
            if (Utils.HasComponent<Enemy>(CharacterSelected.gameObject))
            {
                var _charSelected = CharacterSelected.GetComponent<Enemy>(); //TODO ??????????????
                _charSelected.CalculateDamage();
            }
        }
    }
    //Chiamato dal pulsante in Character Panel
    public void ChangeCombatMode(int combatMode)
    {
        if (CharacterInTheTurn != null)
        {
            CharacterInTheTurn.combatMode = (CombatMode)combatMode;
        }
    }

}
