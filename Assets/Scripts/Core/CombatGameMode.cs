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
    [SerializeField] Timer timer;
    [SerializeField] Transform centerOfField;

    MainCanvas canvas;
    public GameObject[] Characters;

    //TODO - cambiare nome e gestire con 'parametro iniziativa'
    List<GameObject> randomizedList = new List<GameObject>();

    //l'interfaccia di comando del Player 
    GameObject[] PlayerPanels;
    GameObject[] LifePanels;

    //l'interfaccia riassuntiva (salute - mana) 
    GameObject LifePanel;
    //L'interfaccia di comando del player
    GameObject playerPanel;


    public GameObject CharacterSelected;
    Enemy enemy;
    //Il personaggio nel campo di battaglia attualmente in turno
    Character CharacterInTheTurn;
    Text enemyTurnText;

    //TODO unificare le code ?
    List<GameObject> CharactersIconList = new List<GameObject>(); //la lista delle icone dei personaggi
    Queue<GameObject> CharacterQueue = new Queue<GameObject>(); //TODO - va trasformata in una lista

    //La lista della turnazione si comporta come un coda, ma ci
    //dà la flessibilità di una lista
    //List<GameObject> TurnQueue = new List<GameObject>();

    float TimeToAttack = 0f; //TODO RIMUOVERE -- stai dando il peggio di te

    Quaternion currentPlayerPanelRotation;
    Vector3 currentPlayerPanelPosition = Vector3.zero;



    void Start()
    {
        //TODO - get by string?
        PlayerPanels = GameObject.FindGameObjectsWithTag("PlayerPanel");
        LifePanels = GameObject.FindGameObjectsWithTag("LifePanel");

        if (PlayerPanels.Length > 0)
        {
            for (int i = 0; i < PlayerPanels.Length; i++)
            {
                PlayerPanels[i].SetActive(false);
            }

        }

        CreateTurn();

        EventManager<OnCharacterSelection>.Register(SelectCharacter);
        EventManager<OnAnimationEnd>.Register(HandleEndOfAnimation);
        EventManager<OnRemoveCharacterFromIconList>.Register(RemoveCharacterFromIconList);
    }

    void Update()
    {

        //Alla fine del turno il valore della tensione del player 
        //aumenta e il tempo del suo turno diminuisce
        if (timer && timer.isTurnOver && CharacterInTheTurn && CharacterInTheTurn.GetComponent<PlayerController>())
        {
            //print(CharacterInTheTurn.gameObject.name + ": turn end");
            CharacterInTheTurn.SetTensionBar(0.05f);
        }

        if (timer && timer.isTurnOver && CharactersIconList.Count > 0 && CharacterQueue.Count > 0)
        {

            CharacterInTheTurn = null;
            
            //distrugge il flag della turnazione precedente
            if (CharactersIconList.Count > 0)
            {
                if(CharactersIconList.First())
                Destroy(CharactersIconList.First().transform.Find("turnIcon").gameObject);
            }
          
            if(CharactersIconList.Count > 0)
            {
                CharactersIconList.RemoveAt(0);
            }
          
            CharacterQueue.Dequeue();

            if (CharactersIconList.Count > 0 && CharacterQueue.Count > 0)
            {
                PutIconAtFirstElementOfQueue();
                HandleCharactersUI();
                HandleInput();
            }
            else
            {
                DestroyTurn();
                CreateTurn();
            }
        }


        //TODO -----------mmm non mi convince per niente
        if (!isPlayerTurn())
        {
            EnemyAttackPlayer();
        }
    }

    //attacca la schermata di comandi a quel player
    void HandleCharactersUI()
    {
        //riattiva i life panel di tutti i personaggi
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
        //disattiva tutti i playerPanel
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            playerPanel = player.transform.Find("PlayerPanel").gameObject;
            if (playerPanel)
            {
                playerPanel.SetActive(false);
            }
        }

        if (isPlayerTurn())
        {
            //disattiva il panel riepilogativo
            var lifePanel = CharacterInTheTurn.transform.Find("LifePanel");
            if (lifePanel)
            {
                lifePanel.gameObject.SetActive(false);
            }

            var relativePoint = Vector3.zero;

            if (centerOfField)
            {
                relativePoint = transform.InverseTransformPoint(centerOfField.position);
            }

            playerPanel = CharacterInTheTurn.transform.Find("PlayerPanel").gameObject;
            if (playerPanel)
            {
                playerPanel.gameObject.SetActive(true);
                playerPanel.transform.position = CharacterInTheTurn.transform.position;
                playerPanel.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                Vector3 newPlayerPanelPosion = new Vector3(relativePoint.x > CharacterInTheTurn.transform.localPosition.x ? -1.5f : 1.5f, 2f, 0);

                playerPanel.transform.position += newPlayerPanelPosion;
                currentPlayerPanelPosition = playerPanel.transform.position;
                currentPlayerPanelRotation = playerPanel.transform.rotation;
            }
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
        if (isPlayerTurn())
        {
            playerPanel.transform.rotation = Quaternion.Euler(currentPlayerPanelRotation.x, Camera.main.transform.eulerAngles.y, currentPlayerPanelRotation.z);
            playerPanel.transform.position = currentPlayerPanelPosition;
        }


    }
    void SetCharacterTurnTime()
    {
        if (!timer) { return; }

        float characterTurnTime = 0;

        //TODO - da rivedere: per il momento il turno del nemico viene sempre
        //settato sulla durata standard
        if (CharacterInTheTurn.GetComponent<PlayerController>())
        {
            characterTurnTime = CharacterInTheTurn.characterTurnTime;
        }
        else if (CharacterInTheTurn.GetComponent<AIController>())
        {
            characterTurnTime = timer.GetStandardTurnTime();
        }

        timer.totalTurnTime = characterTurnTime;
        timer.timeRemaining = timer.totalTurnTime;
        timer.SetSliderToMax();
    }

    void PutIconAtFirstElementOfQueue()
    {
        CharacterInTheTurn = CharacterQueue.Peek().GetComponent<Character>();
        SetCharacterTurnTime();

        //emette evento per CameraManager
        EventManager<OnSetCharacterInTurn>.Trigger?.Invoke(CharacterQueue.Peek());

        //CombatMode di default
        ChangeCombatMode((int)CombatMode.ShootingMode);

        //TODO Gestire diversamente
        HandleCharactersUI();

        HandleInput();

        //Crea il rettangolino rosso del turno
        if (CharactersIconList.Count > 0)
        {
            //TODO si tratta di una gestione provvisoria
            if(CharactersIconList.First()){
                GameObject _TurnIcon = new GameObject("turnIcon");
                Image turnIcon = _TurnIcon.AddComponent<Image>();
                turnIcon.color = Color.red;
                turnIcon.transform.SetParent(CharactersIconList.First().transform);
                turnIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
                turnIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 50);
            }
            else
            {
                EndTurn();
            }
           
        }

    }

    // Prendiamo tutti i personaggi sul campo di battaglia
    // e creiamo un'immagine per ognuno di essi inserendola 
    // nella turnazione
    void CreateTurn()
    {
        //TODO -- qual è il modo migliore per selezionare il canvas?
        canvas = GameObject.FindObjectOfType<MainCanvas>();

        //TODO - Ci indica l'azione del nemico
        enemyTurnText = canvas.transform.Find("EnemyTurnText").GetComponent<Text>();
        enemyTurnText.enabled = false;

        Characters = GameObject.FindGameObjectsWithTag("Character").Where(g => g.GetComponent<Character>().enabled).ToArray();

        randomizedList = Utils.Randomize(Characters.ToList());
        CreateTurnImage();
        PutIconAtFirstElementOfQueue();
    }
    void CreateTurnImage()
    {
        float imgOffset = -450f;
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
                CharactersIconList.Add(imgObject);

                //TODO : creare una sola coda
                CharacterQueue.Enqueue(character);
            }
        }
    }

    //distrugge tutte le icone
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
        if (enemyTurnText)
            enemyTurnText.enabled = !isPlayerTurn();

        if (!isPlayerTurn() && TimeToAttack == 0)
        {
            enemy = CharacterInTheTurn.GetComponent<Enemy>();
            //TODO creare un sistema di calcolo dell'attacco nemico
            TimeToAttack = enemy.CalculateAttackTime(timer.totalTurnTime) + timer.totalTurnTime / 2;

            if (TimeToAttack >= timer.totalTurnTime)
            {
                TimeToAttack = timer.totalTurnTime;
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

    //TODO - questa logica va spostata nell'AI controller
    void EnemyAttackPlayer()
    {
     
        if (timer.timeRemaining <= TimeToAttack  )
        {
         
            TimeToAttack = 0;
            //TODO - non mi convince !Utils.HasComponent<Enemy>
            var playersList = FindObjectsOfType<Character>().Where(ch => ch.enabled && !Utils.HasComponent<Enemy>(ch.gameObject)).ToList();
            
            if (enemy.enabled && enemy.weapon && playersList.Count > 0)
            {
                //TODO -- per il momento seleziona casualmente il player
                //costruire un comportamento per cui la personalità del nemico 
                //selezioni il giocatore da attaccare
                //var weaponDamage = enemy.weapon.GetComponent<Weapon>().damage;
                var playerSelected = playersList[Random.Range(0, playersList.Count)];

                enemy.otherCharacter = playerSelected.gameObject;
                //TODO - mettere nel character
                StartCoroutine(enemy.AttackCharacterAtTheEndOfRotation(2f, enemy, playerSelected));

            }
        }

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
                CharacterInTheTurn.otherCharacter = CharacterSelected;

                if (CharacterInTheTurn.weapon && CharacterInTheTurn.CanHit())
                {
                    StartCoroutine(CharacterInTheTurn.AttackCharacterAtTheEndOfRotation(2f, CharacterInTheTurn, enemy));
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

    void RemoveCharacterFromIconList(GameObject character)
    {
        var characterIcon = canvas.transform.Find(character.name);
        var a = GetComponent<Button>();

        if (characterIcon)
        {
            Destroy(characterIcon.gameObject);
        }

        CharactersIconList.Remove(character);
        
        //rimuovo il personaggio dalle liste
        randomizedList.Remove(character);
       
        
    }

    bool AllPlayersAreDeath()
    {
        foreach (var item in randomizedList)
        {
            if (item.GetComponent<PlayerController>())
            {
                return false;
            }
        }
        return true;
    }

    void GameOver()
    {
        var gameOverText = canvas.transform.Find("gameOver");
        if (gameOverText)
        {
            gameOverText.gameObject.SetActive(true);
        }

        Time.timeScale = 0;
    }
    void HandleEndOfAnimation(string animationName)
    {
        if(animationName == "DeathAnimation")
        {
            if (AllPlayersAreDeath())
            {
                GameOver();
            }
        }
        else
        {
            EndTurn();
        }
    }
    void EndTurn()
    {
        timer.Time_Zero();
    }

}
