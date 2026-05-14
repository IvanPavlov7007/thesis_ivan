using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UMA.CharacterSystem.Examples;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Pixelplacement;
using UniRx;

/// <summary>
/// Sets interactable menu for changing the characteristics of the humanoid avatar
/// For that defines menu states in a state pattern.
/// 
/// This class defines methods to create and destroy UI items such as sliders, buttons etc and data
/// Only they can be used by menu states
/// </summary>
public class HumanoidAvatarCreator : MonoBehaviour
{
    public HumanoidAvatarManager humanoidAvatar;
    [Header("UI prefabs")]
    public GameObject buttonPrefab;
    public GameObject togglePrefab;
    public GameObject sliderPrefab;
    public GameObject DNA_SliderPrefab;
    public GameObject UI_GridPrefab;
    public GameObject WardrobeButtonPrefab;
    public GameObject ColorButtonPrefab;
    public GameObject GroupLabelPrefab;

    [Header("Existing elements")]
    public GameObject GenderDropDown;
    public Transform UI_Menu, UI_Content;

    [Space]
    public string headDNAsRegex = "^face|forehead|chin|ear|cheek|eye|nose|mouth|jaw|lip|ear|mandible";
    public string bodyDNAsRegex = "^(hand|body|forearm|leg|torso|arm|height|head|neck|belly|gluteus|breast|finger|waist)|muscle|weight";
    public string skinDNAsRegex = "^(skin)(?!OlderTexture)";
    public string headWardrobeRegex = "^eye|ear";
    public string hairWardrobeRegex = "^hair|beard|brow";
    public string outfitWardrobeRegex = "*";

    public UMAWardrobeRecipe oldSkinRecipe;

    [Space]
    public SharedColorTable eyesColorOptions;
    public SharedColorTable hairColorOptions;
    public SharedColorTable lipsColorOptions;
    public float sliderChangeCooldown = 0.5f;

    private List<CreatorState> availableStates = new List<CreatorState>();
    private CreatorState currentState;

    private List<GameObject> CreatorStatesButtons = new List<GameObject>();

    // If you read this have a nice day or night :)


    public void Show()
    {
        regenerateCreator(currentGender != Gender.None);
        GenderDropDown.SetActive(true);
    }

    public void Hide()
    {
        regenerateCreator(false);
        GenderDropDown.SetActive(false);
    }

    /// <summary>
    /// Clear or show current menu's settings
    /// </summary>
    /// <param name="active"></param>
    private void regenerateCreator(bool active)
    {
        if (active)
        {
            clearState();
            regeneratePossibleStates();
            clearMenuButtons();
            regenerateMenuButtons();
        }
        else
        {
            clearState();
            clearMenuButtons();
        }
    }


    private void regeneratePossibleStates()
    {
        //Clear previous States
        availableStates.Clear();

        if (humanoidAvatar.gameObject.activeSelf)
        {
            availableStates.AddRange(new List<CreatorState>() { new SkinState(),new BodyState() , new HeadState(), new HairState(), new OutfitState() });
#if UNITY_EDITOR
            availableStates.Add(new DebugAllDNAState());
            availableStates.Add(new DebugSlotsState());
#endif
        }
    }

    private void clearMenuButtons()
    {
        for (int i = 0; i < CreatorStatesButtons.Count; i++)
        {
            var ob = CreatorStatesButtons[i];
            UMAUtils.DestroySceneObject(ob);
        }
        CreatorStatesButtons.Clear();
    }

    private void regenerateMenuButtons()
    {
        foreach (var state in availableStates)
        {
            var ob = Instantiate(buttonPrefab, UI_Menu);

            ob.GetComponentInChildren<Button>().onClick.AddListener(delegate { setState(state); });
            ob.GetComponentInChildren<TextMeshProUGUI>().text = state.buttonText;
            CreatorStatesButtons.Add(ob);
        }
    }

    public void setState(CreatorState state)
    {
        if (state == null)
            throw new UnityException("AvatarCreator - can't switch to null state");
        if (currentState != null)
        {
            currentState.ExitState(this);
        }
        currentState = state;
        currentState.EnterState(this);
    }

    public void clearState()
    {
        if (currentState == null)
            return;

        currentState.ExitState(this);
        currentState = null;
    }

    public void setState(string state)
    {
        var s = availableStates.Find(x => x.GetType().Name == state);
        setState(s);
    }

    #region Gender

    //manages the current gender of the character
    //TODO: it actually belongs to HumanoidAvatarManager. So move it there!

    public enum Gender {None, 
        Male, 
        //Nonbinary, 
        Female}

    public Gender currentGender { get; private set; } = Gender.None;

    public event Action<Gender> onGenderChanged;

    public void triggerChange()
    {
        onGenderChanged?.Invoke(currentGender);
    }

    public void SelectGender(int index) 
    { 
        SelectGender((Gender)index); 
    }
    public void SelectGender(Gender gender)
    {
        switch (gender)
        {
            case Gender.Male:
                humanoidAvatar.ChangeRace("HumanMale");
                humanoidAvatar.SetActive(true);
                break;
            //case Gender.Nonbinary:
            //    humanoidAvatar.ChangeRace("Elf Female");
            //    humanoidAvatar.SetActive(true);
            //    break;
            case Gender.Female:
                humanoidAvatar.ChangeRace("HumanFemale");
                humanoidAvatar.SetActive(true);
                break;
            default:
                humanoidAvatar.SetActive(false);
                break;
        }
        humanoidAvatar.ReapplyWardrobeCollections();
        currentGender = gender;

        regenerateCreator(gender != Gender.None);

        onGenderChanged?.Invoke(gender);
    }

    public void SelectGender(string gender)
    {
        switch (gender)
        {
            case "Male":
                SelectGender(Gender.Male);
                break;
            //case "Nonbinary":
            //    SelectGender(Gender.Nonbinary);
            //    break;
            case "Female":
                SelectGender(Gender.Female);
                break;
            default:
                SelectGender(Gender.None);
                break;
        }
    }

    #endregion


    /// <summary>
    /// Searches for specific DNAHolders, for example to use in DNASliders
    /// </summary>
    /// <param name="regexPattern"></param>
    /// <returns></returns>
    public List<DNAPanel.DNAHolder> getSpecificDNAHolders(string regexPattern)
    {
        Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

        UMADnaBase[] DNA = humanoidAvatar.GetAllDNA();
        List<DNAPanel.DNAHolder> ValidDNA = new List<DNAPanel.DNAHolder>();
        for (int i1 = 0; i1 < DNA.Length; i1++)
        {
            UMADnaBase d = DNA[i1];
            string[] names = d.Names;
            float[] values = d.Values;

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                if (regex.IsMatch(name))
                {
                    ValidDNA.Add(new DNAPanel.DNAHolder(name, values[i], i, d));
                }
            }

        }

        ValidDNA.Sort();
        return ValidDNA;
    }

    public List<DNASlider> instantiateDNASliders(List<DNAPanel.DNAHolder> ValidDNA)
    {
        List<DNASlider> result = new List<DNASlider>();
        for (int i = 0; i < ValidDNA.Count; i++)
        {
            DNAPanel.DNAHolder dna = ValidDNA[i];
            GameObject go = GameObject.Instantiate(DNA_SliderPrefab, UI_Content);
            DNASlider de = go.GetComponentInChildren<DNASlider>();
            de.SetUp(dna.name.BreakupCamelCase(), dna.index, dna.dnaBase, humanoidAvatar, dna.value);
            de.settingCooldown = sliderChangeCooldown;
            go.SetActive(true);
            result.Add(de);
        }
        return result;
    }
    /// <summary>
    /// instantiates a grid of recipe-picking buttons for a given slot
    /// </summary>
    /// <param name="slotName"></param>
    public void instantiateChoicesForSlot(string slotName)
    {
        var grid = Instantiate(UI_GridPrefab, UI_Content.transform);
        // Get the available UMATextRecipes for this slot.
        List<UMATextRecipe> SlotRecipes = humanoidAvatar.AvailableRecipes[slotName];

        if (slotName != "WardrobeCollection")
        {
            addSlotOption("Remove", slotName, grid.transform);
        }

        for (int i = 0; i < SlotRecipes.Count; i++)
        {
            UMATextRecipe utr = SlotRecipes[i];
            string name;
            if (string.IsNullOrEmpty(utr.DisplayValue))
            {
                name = utr.name;
            }
            else
            {
                name = utr.DisplayValue;
            }

            //Empty string, in order to not show the text
            addSlotOption("", slotName, grid.transform, utr);
        }
    }

    void addSlotOption(string name, string slotName, Transform parent, UMATextRecipe utr = null)
    {
        GameObject go = Instantiate(WardrobeButtonPrefab, parent);
        CustomWardrobeHandler wh = go.GetComponent<CustomWardrobeHandler>();
        wh.Setup(humanoidAvatar, utr, slotName, name);
        wh.SetColors();// no Idea why, just copied from an UMA example code
        if (utr != null)
            wh.GetComponentInChildren<Image>().sprite = utr.GetWardrobeRecipeThumbFor(humanoidAvatar.activeRace.name);
    }

    /// <summary>
    /// instantiates a grid of color-picking buttons for a sharedColor(UMA)
    /// </summary>
    /// <param name="sharedColorTable">has all the color choices and defines a sharedColor(UMA)</param>
    public void instantiateChoicesForColor(SharedColorTable sharedColorTable)
    {
        var grid = Instantiate(UI_GridPrefab, UI_Content.transform);

        foreach (var color in sharedColorTable.colors)
        {
            GameObject button = Instantiate(ColorButtonPrefab, grid.transform);
            button.GetComponent<ColorVariantButton>().Setup(sharedColorTable.sharedColorName, color, humanoidAvatar);
        }

    }

    public void createLabel(string text, Transform parent)
    {
        var tmpro = Instantiate(GroupLabelPrefab, parent).GetComponentInChildren<TextMeshProUGUI>();
        tmpro.text = text;
    }

    public Toggle createToggle(string text, Transform parent)
    {
        GameObject obj = Instantiate(togglePrefab, parent);
        var tmpro = obj.GetComponentInChildren<TextMeshProUGUI>();
        tmpro.text = text;

        var toggle = obj.GetComponentInChildren<Toggle>();
        return toggle;
    }

    public Slider createSlider(string text, Transform ui_parent, Action<float> valueChanged)
    {
        GameObject obj = Instantiate(sliderPrefab, ui_parent);
        var tmpro = obj.GetComponentInChildren<TextMeshProUGUI>();
        tmpro.text = text;

        var slider = obj.GetComponentInChildren<Slider>();
        if(valueChanged != null)
            slider.onValueChanged.AddListener(new UnityAction<float>(valueChanged));
        return slider;
    }

    /// <summary>
    /// Destroy all of the created buttons, sliders, toggles etc
    /// </summary>
    /// <param name="ui"></param>
    public void cleanUp_UI_Content(Transform ui)
    {
        if (UI_Content.transform.childCount > 0)
        {
            foreach (Transform t in ui.transform)
            {
                UMAUtils.DestroySceneObject(t.gameObject);
            }
        }
    }
}

/// <summary>
/// Base State
/// </summary>
public abstract class CreatorState
{
    public abstract string buttonText {get;}

    protected HumanoidAvatarCreator context;
    public virtual void EnterState(HumanoidAvatarCreator context)
    {
        this.context = context;
    }

    public virtual void ExitState(HumanoidAvatarCreator context)
    {
        context.cleanUp_UI_Content(context.UI_Content);
        this.context = null;
    }
}

/// <summary>
/// Body characteristics like hight, arms, posture
/// </summary>
public class BodyState : CreatorState
{
    public override string buttonText { get { return "Body"; } }

    public override void EnterState(HumanoidAvatarCreator context)
    {
        base.EnterState(context);
        reloadDNA_Sliders();
    }

    void reloadDNA_Sliders()
    {
        List<DNAPanel.DNAHolder> ValidDNA = context.getSpecificDNAHolders(context.bodyDNAsRegex);
        var sliders = context.instantiateDNASliders(ValidDNA);
    }
}

/// <summary>
/// Head characteristics - everything that has to do with the face, ears, eyes
/// </summary>
public class HeadState : CreatorState
{
    public override string buttonText { get { return "Head"; } }

    public override void EnterState(HumanoidAvatarCreator context)
    {
        base.EnterState(context);
        reloadDNA_Sliders();
        reloadHeadSlots();
        reloadColors();
    }

    void reloadDNA_Sliders()
    {
        List<DNAPanel.DNAHolder> ValidDNA = context.getSpecificDNAHolders(context.headDNAsRegex);
        context.instantiateDNASliders(ValidDNA);
    }

    void reloadHeadSlots()
    {
        Dictionary<string, List<UMATextRecipe>> recipes = context.humanoidAvatar.AvailableRecipes;

        Regex regex = new Regex(context.headWardrobeRegex, RegexOptions.IgnoreCase);
        var outfitSlots = recipes.Where(item => regex.IsMatch(item.Key)).Select(item => item.Key);

        foreach (string s in outfitSlots)
        {
            context.createLabel(s, context.UI_Content);
            context.instantiateChoicesForSlot(s);
        }
    }

    void reloadColors()
    {
        //Color choices for the eyes color
        context.createLabel("Eyes color", context.UI_Content);
        context.instantiateChoicesForColor(context.eyesColorOptions);
        if (context.currentGender == HumanoidAvatarCreator.Gender.Female)
        {
            context.createLabel("Lips color", context.UI_Content);
            context.instantiateChoicesForColor(context.lipsColorOptions);
        }
    }
}

/// <summary>
/// Skin characteristics
/// </summary>
public class SkinState : CreatorState
{
    public override string buttonText { get { return "Skin"; } }

    public override void EnterState(HumanoidAvatarCreator context)
    {
        base.EnterState(context);
        reloadDNA_Sliders();
        bool oldSkinIsOn = context.humanoidAvatar.GetRecepies().Values.Any(x=> x.name == context.oldSkinRecipe.name);

        var toggle = context.createToggle("older skin texture", context.UI_Content);
        // setting the initial value and afterwards binding action for toggle
        toggle.gameObject.AddComponent<ManagedBehaviour>().started +=
            (ManagedBehaviour b) => { toggle.isOn = oldSkinIsOn; toggle.onValueChanged.AddListener(onOldSkinToogled); };
    }

    private void onOldSkinToogled(bool on)
    {

        if (on)
        {
            context.humanoidAvatar.SetSlot(context.oldSkinRecipe);
        }
        else
            context.humanoidAvatar.ClearSlot(context.oldSkinRecipe);
        context.humanoidAvatar.BuildCharacter();
    }

    void reloadDNA_Sliders()
    {
        List<DNAPanel.DNAHolder> ValidDNA = context.getSpecificDNAHolders(context.skinDNAsRegex);
        context.instantiateDNASliders(ValidDNA);
    }
}

/// <summary>
/// Hair characteristics, including hairstyle, beard, colors etc
/// </summary>
public class HairState : CreatorState
{
    public override string buttonText { get { return "Hair"; } }

    public override void EnterState(HumanoidAvatarCreator context)
    {
        base.EnterState(context);
        //Instantiate Wardrobe choices for each hair-related slot
        Dictionary<string, List<UMATextRecipe>> recipes = context.humanoidAvatar.AvailableRecipes;
        Regex regex = new Regex(context.hairWardrobeRegex, RegexOptions.IgnoreCase);
        var hairSlots = recipes.Where(item => regex.IsMatch(item.Key)).Select(item=>item.Key);
        foreach (string s in hairSlots)
        {
            context.createLabel(s, context.UI_Content);
            context.instantiateChoicesForSlot(s);
        }

        //Color choices for the hair color
        context.createLabel("Hair's color", context.UI_Content);
        context.instantiateChoicesForColor(context.hairColorOptions);
        var densitySlider = context.createSlider("Hair density", context.UI_Content, null);

        densitySlider.gameObject.AddComponent<ManagedBehaviour>().started +=
                    (_ => initializeSlider(densitySlider));
    }

    private void initializeSlider(Slider slider)
    {
        //Initialize displayed slider's value
        slider.SetValueWithoutNotify(context.humanoidAvatar.GetColor("Hair").color.a);

        //observable( event-collector) for when value gets changed, with a cooldown, invoking the first
        var immediateChange = slider.OnValueChangedAsObservable().DistinctUntilChanged()
            .ThrottleFirst(TimeSpan.FromSeconds(context.sliderChangeCooldown));

        //observable( event-collector) for when value gets changed, with a cooldown, with last change is always invoked
        var lastChange = slider.OnValueChangedAsObservable().DistinctUntilChanged()
            .Throttle(TimeSpan.FromSeconds(context.sliderChangeCooldown));

        //merging both observables and subscribing an action.
        immediateChange.Merge(lastChange)
            .DistinctUntilChanged()// both observables may collect the same events, so they have to be distinct
            .Subscribe(changeHairDensity).AddTo(slider);
    }
    public void changeHairDensity(float value)
    {
        var data = context.humanoidAvatar.GetColor("Hair");
        var color = data.color;
        color.a = value;
        data.color = color;
        context.humanoidAvatar.SetColorAlbedo("Hair", data);
    }
}

/// <summary>
/// Outfits
/// </summary>
public class OutfitState : CreatorState
{
    public override void EnterState(HumanoidAvatarCreator context)
    {
        base.EnterState(context);
        reloadOutfit_Slots();
    }
    public override string buttonText { get { return "Outfit"; } }
    void reloadOutfit_Slots()
    {
        Dictionary<string, List<UMATextRecipe>> recipes = context.humanoidAvatar.AvailableRecipes;

        Regex regex = new Regex(context.outfitWardrobeRegex, RegexOptions.IgnoreCase);
        var outfitSlots = recipes.Where(item=>regex.IsMatch(item.Key)).Select(item=>item.Key);

        foreach (string s in outfitSlots)
        {
            context.createLabel(s, context.UI_Content);
            context.instantiateChoicesForSlot(s);
        }
    }
}

//Used only in Editor
public class DebugAllDNAState : CreatorState
{
    public override string buttonText { get { return "[DEBUG]All DNAs"; } }

    public override void EnterState(HumanoidAvatarCreator context)
    {
        base.EnterState(context);
        reloadDNA_Sliders();
        context.triggerChange();
    }

    void reloadDNA_Sliders()
    {
        List<DNAPanel.DNAHolder> ValidDNA = context.getSpecificDNAHolders("^.*$");
        context.instantiateDNASliders(ValidDNA);
    }
}
//Used only in Editor
public class DebugSlotsState : CreatorState
{
    public override void EnterState(HumanoidAvatarCreator context)
    {
        base.EnterState(context);
        reloadOutfit_Slots();
        context.triggerChange();
    }
    public override string buttonText { get { return "[DEBUG]All slots"; } }
    void reloadOutfit_Slots()
    {
        Dictionary<string, List<UMATextRecipe>> recipes = context.humanoidAvatar.AvailableRecipes;

        var outfitSlots = recipes.Keys.ToArray();

        foreach (string s in outfitSlots)
        {
            context.createLabel(s, context.UI_Content);
            context.instantiateChoicesForSlot(s);
        }
    }
}