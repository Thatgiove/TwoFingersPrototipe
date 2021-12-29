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
        
        //La UI dei personaggi sul campo di battaglia 
        [SerializeField] Image tensionBar; //TODO - prendere dal playerPanel
        [SerializeField] Canvas lifePanel; //TODO - lo prendiamo da qua? Teoricamente è figlio del gameObject
        [SerializeField] Canvas playerPanel; //TODO - lo prendiamo da qua? Teoricamente è figlio del gameObject
        
        [SerializeField] GameObject itemButton;
        [SerializeField] GameObject characterButton; //I prefab che ci servono per la creazione delle combo oggetti e personaggi
        
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
        List<Button> itemsBtnList = new List<Button>();

        Transform itemsButton; //bottone toggle itemsScroll
        Transform itemsScroll; //il menu degli oggetti nel playerCanvas
        Transform characterScroll; //il menu dei personaggi selezionabili nel playerCanvas

        string itemDescription = "";
       
        bool itemsMenuOpen = false;
        bool skillsMenuOpen = false;

        void Awake()
        {
            timer = FindObjectOfType<Timer>();
            combatGameMode = FindObjectOfType<CombatGameMode>();

            if (timer)
            {
                characterTurnTime = timer.GetStandardTurnTime();
            }
        }

        void Start()
        {
           
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
            if (playerPanel)
            {
                characterScroll = playerPanel.transform.Find("characterScroll");
                itemsScroll = playerPanel.transform.Find("itemsScroll");
                itemsButton = playerPanel.transform.Find("itemsButton");
               
                if (itemsButton)
                {
                    itemsButton.GetComponent<Button>().onClick.AddListener(ToggleItemsMenu);
                }
            }
            //TODO: qui abbiamo codice ripetuto. Trovare un modo per unificare la creazione di bottoni
           
            CreateItemsDropdown();
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
            if (playerPanel && itemButton)
            {
                //TODO - fare meglio
                var viewport = itemsScroll.Find("Viewport").Find("Content");
                if (!viewport) return;
                var imgOffset = -12f;

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
                        trans.localPosition = new Vector3(50, imgOffset, 0);
                        trans.sizeDelta = new Vector2(100, 24);
                        trans.localRotation = Quaternion.identity;

                        imgOffset -= 20;
                        
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
                        entry.callback.AddListener((eventData) => { SetItemDescription(itBtn.name);});
                        etrigger.triggers.Add(entry);

                        itemsBtnList.Add(button);
                    }
                }
            }
        }  
        void CreateCharactersDropdown()
        {

            if (combatGameMode.Characters.Length > 0 && characterButton)
            {
                var viewport = characterScroll.Find("Viewport").Find("Content");
                if (!viewport) return;
                var imgOffset = -12f;

                //Distrugge tutti i bottoni precedenti
                foreach (Transform child in viewport)
                {
                    GameObject.Destroy(child.gameObject);
                }

                for (int i = 0; i < combatGameMode.Characters.Length; i++)
                {
                    GameObject itBtn = Instantiate(characterButton);


                    itBtn.name = i.ToString();
                    itBtn.transform.SetParent(viewport.transform);
                    

                    var trans = itBtn.GetComponent<RectTransform>();
                    trans.localScale = Vector3.one;
                    trans.localPosition = new Vector3(50, imgOffset, 0);
                    trans.sizeDelta = new Vector2(100, 24);
                    trans.localRotation = Quaternion.identity;
   
                    
                    imgOffset -= 20;

                    var button = itBtn.GetComponent<Button>();

                    var _iText = itBtn.transform.Find("itemText");
                    if (_iText)
                    {
                        _iText.GetComponent<TMP_Text>().text = combatGameMode.Characters[i].name;
                        //se è enemy il colore della scritta è rosso
                        if (combatGameMode.Characters[i].GetComponent<AIController>())
                        {
                            _iText.GetComponent<TMP_Text>().color = Color.red;
                        }
                        else
                        {
                            _iText.GetComponent<TMP_Text>().color = Color.green;
                        }
                       
                    }
                    //TODO deve essere generico per attacco, oggetto e abilità
                    button.onClick.AddListener(() => UseItemToCharacter(itBtn.name));
                }
            };
        }
        void SelectItem(string _itemIndex)
        {
            itemIndex = Int32.Parse(_itemIndex);
            itemSelected = inventory[itemIndex].item;
            
            if (combatGameMode)
            {
                ToggleItemsButtons(false);
                characterScroll.gameObject.SetActive(itemsMenuOpen);
                CreateCharactersDropdown();
            }
        }

        void ToggleSkillsMenu()
        {
            skillsMenuOpen = !skillsMenuOpen;
            CloseCharactersComboOnToggle(skillsMenuOpen);
        }

        void ToggleItemsMenu()
        {
            itemsMenuOpen = !itemsMenuOpen;
            itemsScroll.gameObject.SetActive(itemsMenuOpen);
            CloseCharactersComboOnToggle(itemsMenuOpen);
            ToggleItemsButtons(true);
        }
        //quando apriamo il menu dei personaggi dopo aver
        //selezionato l'oggetto è opportuno disattivare 
        //i bottoni degli oggetti 
        void ToggleItemsButtons(bool isEnabled)
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
                StartCoroutine(AttackCharacterAtTheEndOfRotation(2f, this, otherCharacter.GetComponent<Character>()));
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

 

        //se chiudo i menu delle skill o degli oggetti,
        //ma quella dei personaggi è ancora aperta allora la chiudo
        void CloseCharactersComboOnToggle(bool isMenuOpen)
        {
            if(!isMenuOpen && characterScroll)
            {
                characterScroll.gameObject.SetActive(isMenuOpen);
            }
        }

        //alla fine delle animazioni abbiamo impostato nell'inspector un evento che comunica con 
        //CombatGameMode e causa la fine del turno o altri comportamenti intercettati da HandleEndOfAnimation
        void OnAnimationEnd(string animation = " ")
        {
            //TODO provvisorio
            if(animation == "throw")
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
            itemsMenuOpen = false; skillsMenuOpen = false;
            itemsScroll.gameObject.SetActive(false);
            characterScroll.gameObject.SetActive(false);
        }
        public void TakeDamage(float amount)
        {
            health -= amount;
            if (health <= 0)
            {
                Die();
            }
            else
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
            if(animation == "hit")
            {
                TriggerHitReactionAnimation();
            }
        }

        //TODO - Questo va nel controller così alla morte posso disattivarlo
        //TODO - va reso generico per tutti i tipi di azioni
        //I personaggi si girano verso il nemico 
        //e attaccano alla fine della rotazione
        //alla fine dell'animazione dell'attacco 
        //emette un evento intercettato da combatGameMode
        //che fa passare il turno
        public IEnumerator AttackCharacterAtTheEndOfRotation(
            float lerpTime,
            Character shooter,
            Character receiver) 
        {
            otherCharacter = receiver.gameObject; //TODO assegnazione obsoleta, rimuovere

            float elapsedTime = 0f;

            while (elapsedTime <= lerpTime)
            {
                if(receiver.gameObject.transform.position != shooter.transform.position)
                {
                    shooter.transform.rotation = Quaternion.Slerp(
                                     shooter.transform.rotation,
                                     Quaternion.LookRotation(receiver.gameObject.transform.position - shooter.transform.position, Vector3.up),
                                     elapsedTime / lerpTime);
                }
             
                elapsedTime += (Time.deltaTime * 10f);
                yield return null;
            }

            //TODO -- la rotazione non avviene solo durante l'attacco , anche se uso oggetti o abilità
            if(actionType == ActionType.Item)
            {
                actionType = ActionType.None;
                TriggerThrowAnimation();
                
     
            }
            else
            {
                shooter.Shoot();
            }
            
        }
        void ConsumeItem()
        {
            itemSelected.Use(otherCharacter);
            itemSelected.TriggerAnimation(otherCharacter);

            RemoveItemFromInventory();
            itemSelected = null;
            itemsMenuOpen = false;
            CloseAllScrollMenu();
            CreateItemsDropdown();
        }
        //TODO gestire meglio e unificare il più possibile le animazioni
        IEnumerator WaitForEndOfShoot(float delay)
        {
            yield return new WaitForSeconds(delay);

            otherCharacter.GetComponent<Character>().TakeDamage(CalculatePhysicalAttackValue());
            EventManager<OnAnimationEnd>.Trigger?.Invoke();
        } 
        
        IEnumerator WaitForEndOfReloading(float delay)
        {
            yield return new WaitForSeconds(delay);
            isReloading = false;

            //Se è enemy attacca nuovamente il player
            if (GetComponent<AIController>())
            {
                StartCoroutine(AttackCharacterAtTheEndOfRotation(2f, this, otherCharacter.GetComponent<Character>()));
            }
        }
        float CalculatePhysicalAttackValue()
        {

            if (!weaponComponent)
            {
                Debug.LogError("WeaponComponent Not found");
                return 0;
            }
            // Attacco dell'arma che tiene conto dei modificatori
            return weaponComponent.CalculateFinalWeaponAttack() +
                 //TODO aggiungere lancio di 2 D10 per calcolare tipo di danno (e se abbiamo un miss o un critical)
                 //e considerare attributo fortuna
                 ((1f / 3f) * (float)dexterity);
        }
    }
}