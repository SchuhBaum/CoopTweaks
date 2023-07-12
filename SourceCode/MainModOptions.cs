using Menu.Remix.MixedUI;
using System.Collections.Generic;
using UnityEngine;
using static CoopTweaks.MainMod;
using static CoopTweaks.ProcessManagerMod;

namespace CoopTweaks;

public class MainModOptions : OptionInterface {
    public static MainModOptions main_mod_options = new();

    //
    // options
    //

    public static Configurable<bool> artificerStun = main_mod_options.config.Bind("artificerStun", defaultValue: true, new ConfigurableInfo("When enabled, Artificer's parry does not stun players. But it does knock them back even when JollyCoop's friendly fire setting is turned off.", null, "", "Artificer Stun"));
    public static Configurable<bool> deafBeep = main_mod_options.config.Bind("deafBeep", defaultValue: true, new ConfigurableInfo("When enabled, mutes the tinnitus beep when near explosions.", null, "", "Deaf Beep"));
    public static Configurable<bool> itemBlinking = main_mod_options.config.Bind("itemBlinking", defaultValue: true, new ConfigurableInfo("When enabled, nearby items only blink even you can pick them up.", null, "", "Item Blinking"));
    public static Configurable<bool> releaseGrasp = main_mod_options.config.Bind("releaseGrasp", defaultValue: true, new ConfigurableInfo("When enabled, other slugcats stop grabbing you when you press jump.", null, "", "Release Grasp"));

    public static Configurable<bool> regionGates = main_mod_options.config.Bind("regionGates", defaultValue: true, new ConfigurableInfo("When enabled, region gates don't wait for players to stand still.", null, "", "Region Gates"));
    public static Configurable<bool> slowMotion = main_mod_options.config.Bind("slowMotion", defaultValue: true, new ConfigurableInfo("When enabled, removes or reduces the slow motion effect in most situations. In addition, the mushroom effect is shared with other players.", null, "", "Slow Motion"));
    public static Configurable<bool> slugcatCollision = main_mod_options.config.Bind("slugcatCollision", defaultValue: true, new ConfigurableInfo("When enabled, players and (most) things that they grab don't collide with each other.", null, "", "Slugcat Collision"));
    public static Configurable<bool> slugOnBack = main_mod_options.config.Bind("slugOnBack", defaultValue: true, new ConfigurableInfo("When enabled, you can only drop slugcats from your back when holding down and grab.", null, "", "SlugOnBack"));

    //
    // parameters
    //

    private readonly float fontHeight = 20f;
    private readonly float spacing = 20f;
    private readonly int numberOfCheckboxes = 3;
    private readonly float checkBoxSize = 24f;
    private float CheckBoxWithSpacing => checkBoxSize + 0.25f * spacing;

    //
    // variables
    //

    private Vector2 marginX = new();
    private Vector2 pos = new();
    private readonly List<OpLabel> textLabels = new();
    private readonly List<float> boxEndPositions = new();

    private readonly List<Configurable<bool>> checkBoxConfigurables = new();
    private readonly List<OpLabel> checkBoxesTextLabels = new();

    //
    // main
    //

    private MainModOptions() {
        On.OptionInterface._SaveConfigFile -= Save_Config_File;
        On.OptionInterface._SaveConfigFile += Save_Config_File;
    }

    private void Save_Config_File(On.OptionInterface.orig__SaveConfigFile orig, OptionInterface option_interface) {
        // the event OnConfigChange is triggered too often;
        // it is triggered when you click on the mod name in the
        // remix menu;
        // initializing the hooks takes like half a second;
        // I don't want to do that too often;

        orig(option_interface);
        if (option_interface != main_mod_options) return;
        Debug.Log("CoopTweaks: Save_Config_File.");
        Initialize_Option_Specific_Hooks();
    }

    //
    // public
    //

    public override void Initialize() {
        base.Initialize();
        Tabs = new OpTab[1];
        Tabs[0] = new OpTab(this, "Options");
        InitializeMarginAndPos();

        // Title
        AddNewLine();
        AddTextLabel("CoopTweaks Mod", bigText: true);
        DrawTextLabels(ref Tabs[0]);

        // Subtitle
        AddNewLine(0.5f);
        AddTextLabel("Version " + version, FLabelAlignment.Left);
        AddTextLabel("by " + author, FLabelAlignment.Right);
        DrawTextLabels(ref Tabs[0]);

        // Content //
        AddNewLine();
        AddBox();

        AddCheckBox(artificerStun, (string)artificerStun.info.Tags[0]);
        AddCheckBox(deafBeep, (string)deafBeep.info.Tags[0]);
        AddCheckBox(itemBlinking, (string)itemBlinking.info.Tags[0]);
        AddCheckBox(regionGates, (string)regionGates.info.Tags[0]);

        AddCheckBox(releaseGrasp, (string)releaseGrasp.info.Tags[0]);
        AddCheckBox(slowMotion, (string)slowMotion.info.Tags[0]);
        AddCheckBox(slugcatCollision, (string)slugcatCollision.info.Tags[0]);
        AddCheckBox(slugOnBack, (string)slugOnBack.info.Tags[0]);

        DrawCheckBoxes(ref Tabs[0]);

        DrawBox(ref Tabs[0]);
    }

    public void Log_All_Options() {
        Debug.Log("CoopTweaks: Option_DeafBeep " + Option_DeafBeep);
        Debug.Log("CoopTweaks: Option_ArtificerStun " + Option_ArtificerStun);
        Debug.Log("CoopTweaks: Option_ItemBlinking " + Option_ItemBlinking);
        Debug.Log("CoopTweaks: Option_RegionGates " + Option_RegionGates);

        Debug.Log("CoopTweaks: Option_ReleaseGrasp " + Option_ReleaseGrasp);
        Debug.Log("CoopTweaks: Option_SlowMotion " + Option_SlowMotion);
        Debug.Log("CoopTweaks: Option_SlugcatCollision " + Option_SlugcatCollision);
        Debug.Log("CoopTweaks: Option_SlugOnBack " + Option_SlugOnBack);
    }

    //
    // private
    //

    private void InitializeMarginAndPos() {
        marginX = new Vector2(50f, 550f);
        pos = new Vector2(50f, 600f);
    }

    private void AddNewLine(float spacingModifier = 1f) {
        pos.x = marginX.x; // left margin
        pos.y -= spacingModifier * spacing;
    }

    private void AddBox() {
        marginX += new Vector2(spacing, -spacing);
        boxEndPositions.Add(pos.y);
        AddNewLine();
    }

    private void DrawBox(ref OpTab tab) {
        marginX += new Vector2(-spacing, spacing);
        AddNewLine();

        float boxWidth = marginX.y - marginX.x;
        int lastIndex = boxEndPositions.Count - 1;
        tab.AddItems(new OpRect(pos, new Vector2(boxWidth, boxEndPositions[lastIndex] - pos.y)));
        boxEndPositions.RemoveAt(lastIndex);
    }

    private void AddCheckBox(Configurable<bool> configurable, string text) {
        checkBoxConfigurables.Add(configurable);
        checkBoxesTextLabels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
    }

    private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
    {
        if (checkBoxConfigurables.Count != checkBoxesTextLabels.Count) return;

        float width = marginX.y - marginX.x;
        float elementWidth = (width - (numberOfCheckboxes - 1) * 0.5f * spacing) / numberOfCheckboxes;
        pos.y -= checkBoxSize;
        float _posX = pos.x;

        for (int checkBoxIndex = 0; checkBoxIndex < checkBoxConfigurables.Count; ++checkBoxIndex) {
            Configurable<bool> configurable = checkBoxConfigurables[checkBoxIndex];
            OpCheckBox checkBox = new(configurable, new Vector2(_posX, pos.y)) {
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(checkBox);
            _posX += CheckBoxWithSpacing;

            OpLabel checkBoxLabel = checkBoxesTextLabels[checkBoxIndex];
            checkBoxLabel.pos = new Vector2(_posX, pos.y + 2f);
            checkBoxLabel.size = new Vector2(elementWidth - CheckBoxWithSpacing, fontHeight);
            tab.AddItems(checkBoxLabel);

            if (checkBoxIndex < checkBoxConfigurables.Count - 1) {
                if ((checkBoxIndex + 1) % numberOfCheckboxes == 0) {
                    AddNewLine();
                    pos.y -= checkBoxSize;
                    _posX = pos.x;
                } else {
                    _posX += elementWidth - CheckBoxWithSpacing + 0.5f * spacing;
                }
            }
        }

        checkBoxConfigurables.Clear();
        checkBoxesTextLabels.Clear();
    }

    private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false) {
        float textHeight = (bigText ? 2f : 1f) * fontHeight;
        if (textLabels.Count == 0) {
            pos.y -= textHeight;
        }

        OpLabel textLabel = new(new Vector2(), new Vector2(20f, textHeight), text, alignment, bigText) // minimal size.x = 20f
        {
            autoWrap = true
        };
        textLabels.Add(textLabel);
    }

    private void DrawTextLabels(ref OpTab tab) {
        if (textLabels.Count == 0) {
            return;
        }

        float width = (marginX.y - marginX.x) / textLabels.Count;
        foreach (OpLabel textLabel in textLabels) {
            textLabel.pos = pos;
            textLabel.size += new Vector2(width - 20f, 0.0f);
            tab.AddItems(textLabel);
            pos.x += width;
        }

        pos.x = marginX.x;
        textLabels.Clear();
    }
}
