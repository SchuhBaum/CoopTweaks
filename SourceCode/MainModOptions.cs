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

    public static Configurable<bool> artificer_stun = main_mod_options.config.Bind("artificerStun", defaultValue: true, new ConfigurableInfo("When enabled, Artificer's parry does not stun players. But it does knock them back even when JollyCoop's friendly fire setting is turned off.", null, "", "Artificer Stun"));
    public static Configurable<bool> deaf_beep = main_mod_options.config.Bind("deafBeep", defaultValue: true, new ConfigurableInfo("When enabled, mutes the tinnitus beep when near explosions.", null, "", "Deaf Beep"));
    public static Configurable<bool> item_blinking = main_mod_options.config.Bind("itemBlinking", defaultValue: true, new ConfigurableInfo("When enabled, nearby items only blink even you can pick them up.", null, "", "Item Blinking"));
    public static Configurable<bool> release_grasp = main_mod_options.config.Bind("releaseGrasp", defaultValue: true, new ConfigurableInfo("When enabled, other slugcats stop grabbing you when you press jump.", null, "", "Release Grasp"));

    public static Configurable<bool> region_gates = main_mod_options.config.Bind("regionGates", defaultValue: true, new ConfigurableInfo("When enabled, region gates don't wait for players to stand still.", null, "", "Region Gates"));
    public static Configurable<bool> slow_motion = main_mod_options.config.Bind("slowMotion", defaultValue: true, new ConfigurableInfo("When enabled, removes or reduces the slow motion effect in most situations. In addition, the mushroom effect is shared with other players.", null, "", "Slow Motion"));
    public static Configurable<bool> slugcat_collision = main_mod_options.config.Bind("slugcatCollision", defaultValue: true, new ConfigurableInfo("When enabled, players and (most) things that they grab don't collide with each other.", null, "", "Slugcat Collision"));
    public static Configurable<bool> slug_on_back = main_mod_options.config.Bind("slugOnBack", defaultValue: true, new ConfigurableInfo("When enabled, you can only drop slugcats from your back when holding down and grab.", null, "", "SlugOnBack"));

    //
    // parameters
    //

    private readonly float _font_height = 20f;
    private readonly float _spacing = 20f;
    private readonly int _number_of_checkboxes = 3;
    private readonly float _check_box_size = 24f;
    private float CheckBoxWithSpacing => _check_box_size + 0.25f * _spacing;

    //
    // variables
    //

    private Vector2 _margin_x = new();
    private Vector2 _pos = new();
    private readonly List<OpLabel> _text_labels = new();
    private readonly List<float> _box_end_positions = new();

    private readonly List<Configurable<bool>> _check_box_configurables = new();
    private readonly List<OpLabel> _check_boxes_text_labels = new();

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
        AddTextLabel("CoopTweaks Mod", big_text: true);
        DrawTextLabels(ref Tabs[0]);

        // Subtitle
        AddNewLine(0.5f);
        AddTextLabel("Version " + version, FLabelAlignment.Left);
        AddTextLabel("by " + author, FLabelAlignment.Right);
        DrawTextLabels(ref Tabs[0]);

        // Content //
        AddNewLine();
        AddBox();

        AddCheckBox(artificer_stun, (string)artificer_stun.info.Tags[0]);
        AddCheckBox(deaf_beep, (string)deaf_beep.info.Tags[0]);
        AddCheckBox(item_blinking, (string)item_blinking.info.Tags[0]);
        AddCheckBox(region_gates, (string)region_gates.info.Tags[0]);

        AddCheckBox(release_grasp, (string)release_grasp.info.Tags[0]);
        AddCheckBox(slow_motion, (string)slow_motion.info.Tags[0]);
        AddCheckBox(slugcat_collision, (string)slugcat_collision.info.Tags[0]);
        AddCheckBox(slug_on_back, (string)slug_on_back.info.Tags[0]);

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
        _margin_x = new Vector2(50f, 550f);
        _pos = new Vector2(50f, 600f);
    }

    private void AddNewLine(float spacing_modifier = 1f) {
        _pos.x = _margin_x.x; // left margin
        _pos.y -= spacing_modifier * _spacing;
    }

    private void AddBox() {
        _margin_x += new Vector2(_spacing, -_spacing);
        _box_end_positions.Add(_pos.y);
        AddNewLine();
    }

    private void DrawBox(ref OpTab tab) {
        _margin_x += new Vector2(-_spacing, _spacing);
        AddNewLine();

        float box_width = _margin_x.y - _margin_x.x;
        int last_index = _box_end_positions.Count - 1;
        tab.AddItems(new OpRect(_pos, new Vector2(box_width, _box_end_positions[last_index] - _pos.y)));
        _box_end_positions.RemoveAt(last_index);
    }

    private void AddCheckBox(Configurable<bool> configurable, string text) {
        _check_box_configurables.Add(configurable);
        _check_boxes_text_labels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
    }

    private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
    {
        if (_check_box_configurables.Count != _check_boxes_text_labels.Count) return;

        float width = _margin_x.y - _margin_x.x;
        float element_width = (width - (_number_of_checkboxes - 1) * 0.5f * _spacing) / _number_of_checkboxes;
        _pos.y -= _check_box_size;
        float pos_x = _pos.x;

        for (int check_box_index = 0; check_box_index < _check_box_configurables.Count; ++check_box_index) {
            Configurable<bool> configurable = _check_box_configurables[check_box_index];
            OpCheckBox check_box = new(configurable, new Vector2(pos_x, _pos.y)) {
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(check_box);
            pos_x += CheckBoxWithSpacing;

            OpLabel check_box_label = _check_boxes_text_labels[check_box_index];
            check_box_label.pos = new Vector2(pos_x, _pos.y + 2f);
            check_box_label.size = new Vector2(element_width - CheckBoxWithSpacing, _font_height);
            tab.AddItems(check_box_label);

            if (check_box_index < _check_box_configurables.Count - 1) {
                if ((check_box_index + 1) % _number_of_checkboxes == 0) {
                    AddNewLine();
                    _pos.y -= _check_box_size;
                    pos_x = _pos.x;
                } else {
                    pos_x += element_width - CheckBoxWithSpacing + 0.5f * _spacing;
                }
            }
        }

        _check_box_configurables.Clear();
        _check_boxes_text_labels.Clear();
    }

    private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool big_text = false) {
        float text_height = (big_text ? 2f : 1f) * _font_height;
        if (_text_labels.Count == 0) {
            _pos.y -= text_height;
        }

        OpLabel text_label = new(new Vector2(), new Vector2(20f, text_height), text, alignment, big_text) // minimal size.x = 20f
        {
            autoWrap = true
        };
        _text_labels.Add(text_label);
    }

    private void DrawTextLabels(ref OpTab tab) {
        if (_text_labels.Count == 0) {
            return;
        }

        float width = (_margin_x.y - _margin_x.x) / _text_labels.Count;
        foreach (OpLabel text_label in _text_labels) {
            text_label.pos = _pos;
            text_label.size += new Vector2(width - 20f, 0.0f);
            tab.AddItems(text_label);
            _pos.x += width;
        }

        _pos.x = _margin_x.x;
        _text_labels.Clear();
    }
}
