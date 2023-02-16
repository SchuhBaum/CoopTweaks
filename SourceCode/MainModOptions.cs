using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace CoopTweaks
{
    public class MainModOptions : OptionInterface
    {
        public static MainModOptions instance = new();

        //
        // options
        //

        public static Configurable<bool> deafBeep = instance.config.Bind("deafBeep", defaultValue: true, new ConfigurableInfo("When enabled, mutes the tinnitus beep when near explosions.", null, "", "Deaf Beep"));
        public static Configurable<bool> itemBlinking = instance.config.Bind("itemBlinking", defaultValue: true, new ConfigurableInfo("When enabled, nearby items only blink even you can pick them up.", null, "", "Item Blinking"));
        public static Configurable<bool> releaseGrasp = instance.config.Bind("releaseGrasp", defaultValue: true, new ConfigurableInfo("When enabled, other slugcats stop grabbing you when you press jump.", null, "", "Release Grasp"));

        public static Configurable<bool> regionGates = instance.config.Bind("regionGates", defaultValue: true, new ConfigurableInfo("When enabled, region gates don't wait for players to stand still.", null, "", "Region Gates"));
        public static Configurable<bool> slugcatCollision = instance.config.Bind("slugcatCollision", defaultValue: true, new ConfigurableInfo("When enabled, slugcats don't collide with each other.", null, "", "Slugcat Collision"));
        public static Configurable<bool> slugOnBack = instance.config.Bind("slugOnBack", defaultValue: true, new ConfigurableInfo("When enabled, you can only drop slugcats from your back when holding down and grab.", null, "", "SlugOnBack"));

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

        public MainModOptions()
        {
            // ambiguity error // why? TODO
            // OnConfigChanged += MainModOptions_OnConfigChanged;
        }

        //
        // public
        //

        public override void Initialize()
        {
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
            AddTextLabel("Version " + MainMod.version, FLabelAlignment.Left);
            AddTextLabel("by " + MainMod.author, FLabelAlignment.Right);
            DrawTextLabels(ref Tabs[0]);

            // Content //
            AddNewLine();
            AddBox();

            AddCheckBox(itemBlinking, (string)itemBlinking.info.Tags[0]);
            AddCheckBox(regionGates, (string)regionGates.info.Tags[0]);
            AddCheckBox(releaseGrasp, (string)releaseGrasp.info.Tags[0]);
            AddCheckBox(slugcatCollision, (string)slugcatCollision.info.Tags[0]);
            AddCheckBox(slugOnBack, (string)slugOnBack.info.Tags[0]);
            DrawCheckBoxes(ref Tabs[0]);

            DrawBox(ref Tabs[0]);
        }

        public void MainModOptions_OnConfigChanged()
        {
            Debug.Log("CoopTweaks: Option_ItemBlinking " + MainMod.Option_ItemBlinking);
            Debug.Log("CoopTweaks: Option_RegionGates " + MainMod.Option_RegionGates);
            Debug.Log("CoopTweaks: Option_ReleaseGrasp " + MainMod.Option_ReleaseGrasp);
            Debug.Log("CoopTweaks: Option_SlugcatCollision " + MainMod.Option_SlugcatCollision);
            Debug.Log("CoopTweaks: Option_SlugOnBack " + MainMod.Option_SlugOnBack);
        }

        //
        // private
        //

        private void InitializeMarginAndPos()
        {
            marginX = new Vector2(50f, 550f);
            pos = new Vector2(50f, 600f);
        }

        private void AddNewLine(float spacingModifier = 1f)
        {
            pos.x = marginX.x; // left margin
            pos.y -= spacingModifier * spacing;
        }

        private void AddBox()
        {
            marginX += new Vector2(spacing, -spacing);
            boxEndPositions.Add(pos.y);
            AddNewLine();
        }

        private void DrawBox(ref OpTab tab)
        {
            marginX += new Vector2(-spacing, spacing);
            AddNewLine();

            float boxWidth = marginX.y - marginX.x;
            int lastIndex = boxEndPositions.Count - 1;
            tab.AddItems(new OpRect(pos, new Vector2(boxWidth, boxEndPositions[lastIndex] - pos.y)));
            boxEndPositions.RemoveAt(lastIndex);
        }

        private void AddCheckBox(Configurable<bool> configurable, string text)
        {
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

            for (int checkBoxIndex = 0; checkBoxIndex < checkBoxConfigurables.Count; ++checkBoxIndex)
            {
                Configurable<bool> configurable = checkBoxConfigurables[checkBoxIndex];
                OpCheckBox checkBox = new(configurable, new Vector2(_posX, pos.y))
                {
                    description = configurable.info?.description ?? ""
                };
                tab.AddItems(checkBox);
                _posX += CheckBoxWithSpacing;

                OpLabel checkBoxLabel = checkBoxesTextLabels[checkBoxIndex];
                checkBoxLabel.pos = new Vector2(_posX, pos.y + 2f);
                checkBoxLabel.size = new Vector2(elementWidth - CheckBoxWithSpacing, fontHeight);
                tab.AddItems(checkBoxLabel);

                if (checkBoxIndex < checkBoxConfigurables.Count - 1)
                {
                    if ((checkBoxIndex + 1) % numberOfCheckboxes == 0)
                    {
                        AddNewLine();
                        pos.y -= checkBoxSize;
                        _posX = pos.x;
                    }
                    else
                    {
                        _posX += elementWidth - CheckBoxWithSpacing + 0.5f * spacing;
                    }
                }
            }

            checkBoxConfigurables.Clear();
            checkBoxesTextLabels.Clear();
        }

        private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false)
        {
            float textHeight = (bigText ? 2f : 1f) * fontHeight;
            if (textLabels.Count == 0)
            {
                pos.y -= textHeight;
            }

            OpLabel textLabel = new(new Vector2(), new Vector2(20f, textHeight), text, alignment, bigText) // minimal size.x = 20f
            {
                autoWrap = true
            };
            textLabels.Add(textLabel);
        }

        private void DrawTextLabels(ref OpTab tab)
        {
            if (textLabels.Count == 0)
            {
                return;
            }

            float width = (marginX.y - marginX.x) / textLabels.Count;
            foreach (OpLabel textLabel in textLabels)
            {
                textLabel.pos = pos;
                textLabel.size += new Vector2(width - 20f, 0.0f);
                tab.AddItems(textLabel);
                pos.x += width;
            }

            pos.x = marginX.x;
            textLabels.Clear();
        }
    }
}