using Assets.Scripts.Delegates;
using Assets.Scripts.Items;
using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Assets.Scripts.Character
{
    //TODO Spostare da qua
    [Serializable]
    public class ItemCollection
    {
        public int quantity;
        public IItem item;
    }
    //Il tipo di azione che sta effettuando il personaggio
    public enum ActionType
    {
        None, Attack, Defense, Item, Skill
    }
    public enum AlteredStatus
    {
        None, 
        Hacked, //i personaggi non controllano l'attacco -- curato da tech innest
        Suffering, //la heath bar si riduce progressivamente -- curato da morphine 
        Cursed, //la mana bar si riduce progressivamente -- curato da sacred oil 
        Anxiety, //la tension bar aumenta più velocemente -- curato San-Axs 
        KO,
        blindness //la % di portare a segno un attacco si riduce drasticamente
    }
    public enum CombatMode
    {
        ShootingMode = 1,
        DefenseMode = 2
    }
    public class HitType
    {
        public string hitType;
        public float value;
        public HitType(string _hitType, float _value)
        {
            hitType = _hitType;
            value = _value;
        }
    }
    public class Character : MonoBehaviour
    {
        /*******************
         * STATISTICHE:
         * 
         * DEXTERITY = la capacità del personaggio di manipolare armi avanzate,
         * serve anche a calcolare il valore di iniziativa, e il danno causato dalle armi 
         * 
         * CONSTITUTION = la capacità del personaggio di resistere ai danni, influenza gli HP
         * 
         * ARCANE = la capacità del personaggio di manipolare le forze arcane
         * 
         * TECNOLOGY = la capacità del personaggio di manipolare la tecnologia, armi e automi
         * 
         * LUCK = bonus generico
         ****************/
        [SerializeField] int dexterity;
        [SerializeField] int constitution;
        [SerializeField] int arcane;
        [SerializeField] int technology;
        [SerializeField] int luck;

        [SerializeField] float mana;
        [SerializeField] float maxHealth;
        public float health;

        //UI dei personaggi sul campo di battaglia 
        [SerializeField] Image tensionBar; //TODO - prendere dal playerPanel
        [SerializeField] Canvas lifePanel; //TODO - lo prendiamo da qua? Teoricamente è figlio del gameObject
        
        [SerializeField] GameObject ControlPanel; //TODO - lo prendiamo da qua? Teoricamente è figlio del gameObject
        [SerializeField] GameObject itemButton; //prefab dei bottoni delle combo personaggi - oggetti

        public Sprite characterIcon;

        //[SerializeField] GameObject damageText;

        public CombatMode combatMode;
        public AlteredStatus alteredStatus;
        public ActionType actionType;

        public GameObject weapon;
        public GameObject armor;
        public GameObject otherCharacter;  //può essere sia nemico che player
        public ItemCollection[] inventory;

        public bool isDead;
        public float tension = 0f; //la barra della tensione, ma mano che aumenta il tempo del turno diminuisce
        public float characterTurnTime; //l'ammontare del turno del personaggio: dipende dalla tension
        public bool isReloading { get; set; } = false;

        float physicalAttackValue;
        float physicalDefenseValue; //viene calcolato in base al valore dell'arma e dalla destrezza
        float minHealth;

        float exp;
        int initiative; //insieme alla destrezza stabilisce i turni nella turnazione
        int itemIndex; //index dell'oggetto selezionato

        Timer timer;
        CombatGameMode combatGameMode;

        Weapon weaponComponent;
        Animator animator;
        IItem itemSelected;
        Character[] charactersInCombatField; //TODO - prendere da combatGameMode?
        PlayerController playerController;

        List<Button> itemsBtnList = new List<Button>();

        Transform itemsButton; //bottone toggle itemsScroll
        Transform shotBtn; //il menu dell'attacco nel playerCanvas
        Transform skillsButton; //il menu dell'attacco nel playerCanvas

        Transform itemsScroll; //il menu degli oggetti nel playerCanvas
        Transform charactersScroll; //il menu dei personaggi selezionabili nel playerCanvas
        Transform damageUI;

        string itemDescription = "";
        string hitInfo = "";

        bool itemsMenuOpen = false;
        bool skillsMenuOpen = false;
        bool charactersMenuOpen = false;

        void Awake()
        {
            //TODO - singleton?
            timer = FindObjectOfType<Timer>();
            combatGameMode = FindObjectOfType<CombatGameMode>();

            if (timer)
            {
                characterTurnTime = timer.GetStandardTurnTime();
            }
        }

        void Start()
        {
            playerController = GetComponent<PlayerController>();
            animator = gameObject.GetComponent<Animator>();
            if (weapon)
            {
                weaponComponent = weapon.GetComponent<Weapon>();
            }

            combatMode = CombatMode.ShootingMode; //TODO?

            if (lifePanel)
            {
                //TODO --- MMMM
                lifePanel.transform.Find("Health").Find("MaxHealthValue").GetComponent<Text>().text = maxHealth.ToString();
                health = maxHealth;
            }
            if (ControlPanel)
            {
                charactersScroll = ControlPanel.transform.Find("charactersScroll");
                itemsScroll = ControlPanel.transform.Find("itemsScroll");

                /*Eventi per chiudere le combo dei personaggi, oggetti, abilità, ecc... */
                var characterBtn = charactersScroll?.Find("closeButton").GetComponent<Button>();
                characterBtn?.onClick.AddListener(CloseCharactersDropdown);

                var itemBtn = itemsScroll?.Find("closeButton").GetComponent<Button>();
                itemBtn?.onClick.AddListener(CloseItemsDropdown);


                var buttonGrid = ControlPanel.transform.Find("buttonsGrid");

                shotBtn = buttonGrid.transform.Find("shotBtn");
                itemsButton = buttonGrid?.transform.Find("itemsBtn");
                //skillsButton = buttonGrid?.transform.Find("skillsBtn");
               
                //binding delle action
                shotBtn?.GetComponent<Button>().onClick.AddListener(ToggleCharacterMenu);
                itemsButton?.GetComponent<Button>().onClick.AddListener(ToggleItemsMenu);
                //skillsButton?.GetComponent<Button>().onClick.AddListener(printTHIS);


            }

            damageUI = gameObject.transform.Find("Damage");
        }

        void printTHIS()
        {
            print(this.gameObject.name);
        }


        void Update()
        {
            if (isDead)
            {
                print("DIE");
            }
            //TODO -?
            if (lifePanel)
            {
                lifePanel.transform.Find("Health").Find("HealthValue").GetComponent<Text>().text = health.ToString("0.0");
                lifePanel.transform.Find("Health").GetComponent<Image>().fillAmount = NormalizedHealth(health);
            }
        }

        void CreateItemsDropdown()
        {
            itemsBtnList = new List<Button>();
            if (ControlPanel && itemButton)
            {
                //TODO - fare meglio
                var viewport = itemsScroll.Find("Viewport").Find("Content");
                if (!viewport) return;
                var imgOffset = -15f;

                //Distrugge tutti i bottoni precedenti
                foreach (Transform child in viewport)
                {
                    GameObject.Destroy(child.gameObject);
                }

                if (inventory.Length > 0)
                {

                    for (int i = 0; i < inventory.Length; i++)
                    {
                        if (inventory[i].quantity <= 0) continue;

                        GameObject itBtn = Instantiate(itemButton);
                        itBtn.name = i.ToString();

                        itBtn.transform.SetParent(viewport.transform);
                        var trans = itBtn.GetComponent<RectTransform>();
                        trans.localScale = Vector3.one;
                        trans.localPosition = new Vector3(87, imgOffset, 0);
                        //trans.sizeDelta = new Vector2(162, 44);
                        trans.localRotation = Quaternion.identity;

                        imgOffset -= 30;

                        var button = itBtn.GetComponent<Button>();

                        var _iText = itBtn.transform.Find("itemText");
                        if (_iText)
                        {
                            _iText.GetComponent<TMP_Text>().text = inventory[i].item.name + "= " + inventory[i].quantity;
                        }
                        button.onClick.AddListener(() => SelectItem(itBtn.name));

                        //Il bottone oggetto ha un EventTrigger che ci permette di 
                        //gestire l'evento del mouse enter e valorizzare il testo 
                        //per mostrare la descrizione dell'oggetto
                        var etrigger = itBtn.GetComponent<EventTrigger>();
                        EventTrigger.Entry entry = new EventTrigger.Entry();
                        entry.eventID = EventTriggerType.PointerEnter;
                        entry.callback.AddListener((eventData) => { SetItemDescription(itBtn.name); });
                        etrigger.triggers.Add(entry);

                        itemsBtnList.Add(button);
                    }
                }
            }
        }

        //Crea una lista di bottoni con tutti i personaggi
        void CreateCharactersDropdown(ActionType actionType)
        {
            if (combatGameMode.Characters.Length > 0 && itemsButton)
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
                    //In base all'actionType bindo l'evento per attacco, oggetto e abilità
                    if (actionType == ActionType.Attack)
                    {
                        button.onClick.AddListener(() => AttackCharacter(itBtn.name));
                    }
                    else if (actionType == ActionType.Item)
                    {
                        button.onClick.AddListener(() => UseItemToCharacter(itBtn.name));
                    }

                }
            };
        }
        void SelectItem(string _itemIndex)
        {
            itemIndex = Int32.Parse(_itemIndex);
            itemSelected = inventory[itemIndex].item;

            if (combatGameMode)
            {
                ItemsButtonsEnabled(false);
                charactersScroll.gameObject.SetActive(itemsMenuOpen);
                CreateCharactersDropdown(ActionType.Item);
            }
        }

        void ToggleSkillsMenu()
        {
            skillsMenuOpen = !skillsMenuOpen;
        }
        void ToggleCharacterMenu()
        {
            CloseAllScrollMenu();
            charactersMenuOpen = !charactersMenuOpen;
            charactersScroll.gameObject.SetActive(charactersMenuOpen);
            if (charactersMenuOpen)
            {
                CreateCharactersDropdown(ActionType.Attack);
            }
        }
        void ToggleItemsMenu()
        {
            print("click");
            CloseAllScrollMenu();
            itemsMenuOpen = !itemsMenuOpen;
            itemsScroll.gameObject.SetActive(itemsMenuOpen);
            if (itemsMenuOpen)
            {
                ItemsButtonsEnabled(true);
                CreateItemsDropdown();
            }

        }

        //quando apriamo il menu dei personaggi dopo aver
        //selezionato l'oggetto è opportuno disattivare 
        //i bottoni degli oggetti 
        void ItemsButtonsEnabled(bool isEnabled)
        {
            foreach (var btn in itemsBtnList)
            {
                btn.enabled = isEnabled;
                btn.GetComponent<EventTrigger>().enabled = isEnabled;
            }
        }
        void UseItemToCharacter(string itemIndex)
        {
            if (combatGameMode)
            {
                otherCharacter = combatGameMode.Characters[Int32.Parse(itemIndex)];

                actionType = ActionType.Item;
                StartCoroutine(playerController?.PerformActionAtTheEndOfRotation(2f, this, otherCharacter.GetComponent<Character>(), ActionType.Item));
            }
        }
        public void AttackCharacter(string itemIndex)
        {
            if (combatGameMode)
            {
                otherCharacter = combatGameMode.Characters[Int32.Parse(itemIndex)];
                actionType = ActionType.Attack;
                StartCoroutine(playerController?.PerformActionAtTheEndOfRotation(2f, this, otherCharacter.GetComponent<Character>(), ActionType.Attack));
            }
        }
        public void AttackCaracterTemp(GameObject _otherCharacter)
        {
            if (combatGameMode)
            {
                actionType = ActionType.Attack;
                otherCharacter = _otherCharacter; //TODO rimuovere
                StartCoroutine(playerController?.PerformActionAtTheEndOfRotation(2f, this, otherCharacter.GetComponent<Character>(), ActionType.Attack));
            }
        }

        void RemoveItemFromInventory()
        {
            ItemCollection item = inventory[itemIndex];
            item.quantity -= 1;
        }

        void SetItemDescription(string itemIndex)
        {
            itemDescription = inventory[Int32.Parse(itemIndex)].item.description;
            //TODO solo per test fare meglio
            itemsScroll.Find("itemDescriptionPanel").Find("itemDescriptionText").GetComponent<TMP_Text>().text = itemDescription;
        }
        void CloseItemsDropdown()
        {
            itemsScroll.gameObject.SetActive(false);
        }
        void CloseCharactersDropdown()
        {
            charactersScroll.gameObject.SetActive(false);
            ItemsButtonsEnabled(true);
        }

        //alla fine delle animazioni abbiamo impostato nell'inspector un evento che comunica con 
        //CombatGameMode e causa la fine del turno o altri comportamenti intercettati da HandleEndOfAnimation
        void OnAnimationEnd(string animation = " ")
        {
            //TODO provvisorio
            if (animation == "throw")
            {
                ConsumeItem();
            }
            //evento per CombatGameMode che chiama HandleEndOfAnimation
            EventManager<OnAnimationEnd>.Trigger?.Invoke(animation);
        }
        //viene chiamato da un evento segnato nell'animazione Gunplay
        void PlayShootSound()
        {
            if (weaponComponent)
            {
                weaponComponent.PlayShootSound();
            }
        }
        void UseAbility() { }

        public void CloseAllScrollMenu()
        {
            itemsMenuOpen = false; skillsMenuOpen = false; charactersMenuOpen = false;
            itemsScroll?.gameObject.SetActive(false);
            charactersScroll?.gameObject.SetActive(false);
        }
        public void TakeDamage(float amount)
        {
            health -= amount;
            if (health <= 0)
            {
                Die();
            }
            else if(amount > 0)
            {
                TriggerHitReactionAnimation();
            }
        }
        //La tensionbar è direttamente proporzionale all'avanzare del turno
        public void SetTensionBar(float tensionAmount)
        {
            if (tensionBar)
            {
                tension += tensionAmount;
                tensionBar.fillAmount = Mathf.Clamp(tension, 0, 1);

                if (timer)
                {
                    characterTurnTime = Mathf.Clamp((characterTurnTime -= CalculateTensionTimerPercentage(tensionAmount)), 5, timer.totalTurnTime);
                }
            }
        }

        //la percentuale di tempo sottratta in base alla tensione
        float CalculateTensionTimerPercentage(float tensionAmount)
        {
            return (tensionAmount / timer.GetStandardTurnTime()) * 100f;
        }

        //In base alla % di difesa del personaggio 
        //determina se posso colpire o se sono in fase di ricarica
        public bool CanHit()
        {
            return weaponComponent && !weaponComponent.isEmpty();
        }

        //health normalizzata range 0 - 1
        float NormalizedHealth(float health)
        {
            return (health - minHealth) / (maxHealth - minHealth);
        }

        public void Shoot()
        {
            if (isReloading)
            {
                return;
            }

            if (weapon && weaponComponent && !weaponComponent.isEmpty())
            {
                weaponComponent.Shoot();
                TriggerShootAnimation();
                StartCoroutine(WaitForEndOfShoot(animator.GetCurrentAnimatorStateInfo(0).speed));
            }
            //TODO -- non va bene così Se è ememy ricarica e attacca
            else if (weapon && weaponComponent && weaponComponent.isEmpty() && GetComponent<AIController>())
            {
                Reload();
            }
        }

        public void Reload()
        {
            isReloading = true;
            weaponComponent.Reloading();
            TriggerReloadAnimation();
        }

        void Die()
        {
            TriggerDeathAnimation();

            //TODO - alla morte deve disabilitare il controller, non il Character
            GetComponent<Character>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;

            EventManager<OnRemoveCharacterFromIconList>.Trigger?.Invoke(gameObject);
        }

        //Animations
        void TriggerShootAnimation()
        {
            if (animator)
            {
                animator.SetTrigger("Shoot");
            }
        }
        void TriggerThrowAnimation()
        {
            if (animator)
            {
                animator.SetTrigger("throw");
            }
        }
        void TriggerHitReactionAnimation()
        {
            if (animator)
            {
                animator.SetTrigger("HitReaction");
            }
        }
        void TriggerReloadAnimation()
        {
            if (animator)
            {
                animator.SetTrigger("Reload");
                StartCoroutine(WaitForEndOfReloading(animator.GetCurrentAnimatorStateInfo(0).speed));
            }
        }
        void TriggerDeathAnimation()
        {
            if (animator)
            {
                animator.SetTrigger("Death");
            }
        }
        //TODO - evento chiamato dall'item, non mi convince
        public void TriggerAnimation(string animation)
        {
            if (animation == "hit")
            {
                TriggerHitReactionAnimation();
            }
            else if(animation == "throw")
            {
                TriggerThrowAnimation();
            }
        }

        void ConsumeItem()
        {
            itemSelected.Use(otherCharacter);
            itemSelected.TriggerAnimation(otherCharacter);

            RemoveItemFromInventory();
            itemSelected = null;
            //itemsMenuOpen = false;
            CloseAllScrollMenu();
            //CreateItemsDropdown();
        }
        //TODO gestire meglio e unificare il più possibile le animazioni
        IEnumerator WaitForEndOfShoot(float delay)
        {
            yield return new WaitForSeconds(delay);
            var damage = CalculatePhysicalAttackValue();
            otherCharacter.GetComponent<Character>().TakeDamage(damage);
            otherCharacter.GetComponent<Character>().DisplayDamageAmount(damage, hitInfo);

            EventManager<OnAnimationEnd>.Trigger?.Invoke();
        }

        IEnumerator WaitForEndOfReloading(float delay)
        {
            yield return new WaitForSeconds(delay);
            isReloading = false;

            //Se è enemy attacca nuovamente il player
            if (GetComponent<AIController>())
            {
                StartCoroutine(GetComponent<AIController>().PerformActionAtTheEndOfRotation(2f, this, otherCharacter.GetComponent<Character>(),ActionType.Attack));
            }
        }
        
        float CalculatePhysicalAttackValue()
        {
            hitInfo = "";

            if (!weaponComponent)
            {
                Debug.LogError("WeaponComponent Not found");
                return 0;
            }
            //TODO considerare attributo fortuna
            var hitType = CalculateHitPercentage();

            hitInfo = hitType.hitType;
            if (hitType.hitType == "Miss")
            {
                return 0;
            }
            
            // Attacco dell'arma che tiene conto dei modificatori
            return (weaponComponent.CalculateFinalWeaponAttack() * hitType.value)
                  + ((1f / 3f) * (float)dexterity);
        }

        public ActionType GetActionType()
        {
            return actionType;
        } 
        
        public void SetActionType(ActionType at)
        {
            this.actionType = at;
        }
        public void ResetActionType()
        {
            this.actionType = ActionType.None;
        }


        public void DisplayDamageAmount(float damage, string attackerHitInfo)
        {
            if (damageUI)
            {
                var damageText = damageUI?.GetChild(0).GetChild(0);
                var hitInfo = damageUI?.GetChild(0).GetChild(1);

                if (!damageText || !hitInfo) return;

                //ruotiamo in direzione della camera
                damageUI.gameObject?.SetActive(true);
                damageUI.transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
                damageText.GetComponent<TMP_Text>().text = damage.ToString();
                hitInfo.GetComponent<TMP_Text>().text = attackerHitInfo;
                StartCoroutine(HideInterface());
            }
        }

        IEnumerator HideInterface()
        {
            yield return new WaitForSeconds(1);
            damageUI.gameObject?.SetActive(false);
        }

        HitType CalculateHitPercentage()
        {
            //simula lancio di 2d10
            var t = UnityEngine.Random.Range(0, 10);
            var u = UnityEngine.Random.Range(0, 10);
            var result = (t * 10) + u;

            if (result <= 4)
            {
                return new HitType("Miss", 0);
            }
            else if (result <= 10)
            {
                return new HitType("Near miss", .5f);
            }
            else if (result >= 96 || result == 0)
            {
                return new HitType("Critical Hit", 2f);
            }
           
            else 
            {
                if (result >= 31 && result <= 50)
                {
                    return new HitType("", 1.1f);
                }
                else if (result >= 51 && result <= 85)
                {
                    return new HitType("", 1.15f);
                }
                else if (result >= 86 && result <= 95)
                {
                    return new HitType("Hard Hit", 1.2f);
                }
                else
                {
                    return new HitType("", 1);
                }
             
            }
                       
        }
    }
}