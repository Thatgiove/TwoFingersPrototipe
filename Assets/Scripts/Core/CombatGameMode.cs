using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Character;
using Assets.Scripts.Delegates;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

/************************
 * CombatGameMode gestisce la turnazione
 * durante la fase di combattimento
 */
public class CombatGameMode : MonoBehaviour
{
    [SerializeField] Timer _Timer;
    [SerializeField] Transform centerOfField;

    MainCanvas canvas;
    GameObject[] Characters;

    //l'interfaccia di comando del Player 
    GameObject PlayerPanel;
    GameObject[] LifePanels;

    //l'interfaccia riassuntiva (salute - mana) 
    GameObject LifePanel;
    public GameObject CharacterSelected;
    Enemy enemy;
    //Il personaggio nel campo di battaglia attualmente in turno
    Character CharacterInTheTurn;
    Text _enemyTurnText;
    //TODO unificare le code
    Queue<GameObject> TurnQueue = new Queue<GameObject>();
    Queue<GameObject> CharacterQueue = new Queue<GameObject>();
    float TimeToAttack = 0f; //TODO RIMUOVERE -- stai dando il peggio di te

    Quaternion currentPlayerPanelRotation;
    Vector3 currentPlayerPanelPosition = Vector3.zero;
    void Start()
    {
        //TODO - get by string?
        PlayerPanel = GameObject.Find("PlayerPanel");
        LifePanels = GameObject.FindGameObjectsWithTag("LifePanel");

        if (PlayerPanel != null)
        {
            PlayerPanel.SetActive(false);
        }

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

                //TODO - check player turn
                if (PlayerPanel)
                {
                    AttachPlayerCanvas();
                }
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
        {
            EnemyAttackPlayer();
        }
    }

    //attacca la schermata di comandi a quel player
    void AttachPlayerCanvas()
    {
        //riattiva i life panel di tutti i personaggi
        //TODO rifare
        //TODO il get character può tornare utile in altre occasioni
        foreach (var obj in FindObjectsOfType<Character>().Where(ch => !Utils.HasComponent<Enemy>(ch.gameObject)))
        {
            LifePanel = obj.transform.Find("LifePanel").gameObject;
            if (LifePanel)
            {
                LifePanel.SetActive(true);
            }

            //controlla se il character è in defense mode
            //in tal caso attiva il Text
            Transform current = LifePanel.transform.GetChild(0);
            current = current.GetChild(0);
            current.gameObject.SetActive(obj.combatMode == CombatMode.DefenseMode);
        }

        if (isPlayerTurn())
        {
            var lifePanel = CharacterInTheTurn.transform.Find("LifePanel");
            if (lifePanel)
            {
                lifePanel.gameObject.SetActive(false);
            }

            PlayerPanel.SetActive(true);
            PlayerPanel.transform.parent = CharacterInTheTurn.gameObject.transform;
            PlayerPanel.transform.position = CharacterInTheTurn.transform.position;

            var relativePoint = Vector3.zero;

            if (centerOfField)
            {
                relativePoint = transform.InverseTransformPoint(centerOfField.position);
            }

            //TODO : rivedere
            Vector3 playerPanelPosion = new Vector3(relativePoint.x > CharacterInTheTurn.transform.localPosition.x ? -1.5f : 1.5f, 2.5f, 0);


            PlayerPanel.transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            PlayerPanel.transform.position += playerPanelPosion;

            currentPlayerPanelPosition = PlayerPanel.transform.position;
            currentPlayerPanelRotation = PlayerPanel.transform.rotation;
        }
        else
        {
            PlayerPanel.SetActive(false);
        }
    }
    void LateUpdate()
    {
        //rotazione dei canvas in direzione della camera
        //ruota tutti i life panel
        if (LifePanels.Length > 0)
        {
            for (int i = 0; i < LifePanels.Length; i++)
            {
                LifePanels[i].transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            }
        }
        //Il playerPanel non cambia la position e rotation
        if (PlayerPanel && isPlayerTurn())
        {
            PlayerPanel.transform.rotation = Quaternion.Euler(currentPlayerPanelRotation.x, Camera.main.transform.eulerAngles.y, currentPlayerPanelRotation.z);
            PlayerPanel.transform.position = currentPlayerPanelPosition;
        }
    }

    void PutIconAtFirstElementOfQueue()
    {
        CharacterInTheTurn = CharacterQueue.Peek().GetComponent<Character>();
        
        //emette evento per CameraManager
        EventManager<OnSetCharacterInTurn>.Trigger?.Invoke(CharacterQueue.Peek());

        //CombatMode di default
        ChangeCombatMode((int)CombatMode.ShootingMode);

        //TODO Gestire diversamente
        if (PlayerPanel)
        {
            AttachPlayerCanvas();
        }

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
        //TODO -- qual è il modo migliore per selezionare il canvas?
        canvas = GameObject.FindObjectOfType<MainCanvas>();
        _enemyTurnText = canvas.transform.Find("EnemyTurnText").GetComponent<Text>();
        _enemyTurnText.enabled = false;

        if (Characters == null)
        {
            Characters = GameObject.FindGameObjectsWithTag("Character");
        }

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
        //TODO - rimuovere
        if (_enemyTurnText)
            _enemyTurnText.enabled = !isPlayerTurn();

        if (!isPlayerTurn() && TimeToAttack == 0)
        {
            enemy = CharacterInTheTurn.GetComponent<Enemy>();
            //TODO creare un sistema di calcolo dell'attacco nemico
            TimeToAttack = enemy.CalculateAttackTime(_Timer.totalTurnTime) + _Timer.totalTurnTime / 2;

            if (TimeToAttack >= _Timer.totalTurnTime)
            {
                TimeToAttack = _Timer.totalTurnTime;
                TimeToAttack -= 3;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        TogglePlayerController();
    }
    //Attiva la rotazione solo di chi è in turno
    void TogglePlayerController()
    {
        foreach (var character in FindObjectsOfType<Character>())
        {
            if (character.GetComponent<PlayerController>())
            {
                character.gameObject.GetComponent<PlayerController>().enabled = false;
            }
        }
        //TODO - si può togliere ?
        if (isPlayerTurn())
        {
            CharacterInTheTurn.gameObject.GetComponent<PlayerController>().enabled = true;
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
            TimeToAttack = 0;
            var playersList = FindObjectsOfType<Character>().Where(ch => !Utils.HasComponent<Enemy>(ch.gameObject)).ToList();
            if (enemy.weapon)
            {
                //TODO -- per il momento seleziona casualmente il player
                //costruire un comportamento per cui la personalità del nemico 
                //selezioni il giocatore da attaccare
                //var weaponDamage = enemy.weapon.GetComponent<Weapon>().damage;
                var playerSelected = playersList[Random.Range(0, playersList.Count)];
                
                enemy.otherCharacter = playerSelected.gameObject;

               //playerSelected.TakeDamage(weaponDamage);
               //TODO - mettere nel character
               StartCoroutine(WaitEndOfRotation(2f, enemy, playerSelected));

            }
            //EndTurn();
        }

    }
    //I personaggi si girano verso il nemico 
    //e attaccano alla fine della rotazione
    IEnumerator WaitEndOfRotation(float lerpTime,
        Character shooter, 
        Character receiver)
    {
        float elapsedTime = 0f;

        while (elapsedTime <= lerpTime)
        {
            shooter.transform.rotation = Quaternion.Slerp(
                shooter.transform.rotation,
                Quaternion.LookRotation(receiver.gameObject.transform.position - shooter.transform.position, Vector3.up), 
                elapsedTime / lerpTime);

            elapsedTime += (Time.deltaTime * 10f);
            yield return null;
        }
        shooter.Shoot();
        receiver.TakeDamage(shooter.weapon.GetComponent<Weapon>().damage);
       
        //TODO Provvisorio
        //if (shooter == enemy)
        //{
        //    EndTurn();
        //}

        EndTurn();
    }

    void SelectCharacter(GameObject charSelected)
    {
        //Il CharacterSelected può essere anche un alleato 

        CharacterSelected = charSelected;
        //TODO il combat va tolto da qua
        if (CharacterInTheTurn.combatMode == CombatMode.ShootingMode)
        {
            if (Utils.HasComponent<Enemy>(CharacterSelected.gameObject))
            {
                var enemy = CharacterSelected.GetComponent<Enemy>();

                if (CharacterInTheTurn.weapon && CharacterInTheTurn.CanHit())
                {
                    StartCoroutine(WaitEndOfRotation(2f, CharacterInTheTurn, enemy));
                }

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
        if (combatMode == (int)CombatMode.DefenseMode)
        {
            EndTurn();
        }
    }

    void EndTurn()
    {
        _Timer.Time_Zero();
    }

}
