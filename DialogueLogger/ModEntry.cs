using System;
using System.Collections.Generic;
using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace DialogueLogger
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        // The log of dialogue lines. Tuple is <name, emotion, dialogue line, responses>
        private DialogueQueue<DialogueElement> dialogueList;
        private ModConfig Config; // The mod configuration from the player
        private List<string> responses;
        private string prevAddedDialogue;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            dialogueList = new(Config.LogLimit);
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.MenuChanged += OnMenuChanged;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>The method called after the game is launched. Enables compatibility with Generic Mod Config Menu.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) return;
            // Register mod to Generic Mod Config Menu
            configMenu.Register(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Maximum Dialogue Lines",
                tooltip: () => "Maximum number of dialogue lines to display in the menu.",
                getValue: () => this.Config.LogLimit,
                setValue: value => this.Config.LogLimit = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Recent Messages First",
                tooltip: () => "Enabling this option will cause the menu to start from the bottom, display recent messages first.",
                getValue: () => this.Config.RecentMessagesFirst,
                setValue: value => this.Config.RecentMessagesFirst = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Log Menu Button",
                tooltip: () => "Key to press to open Log.",
                getValue: () => this.Config.LogButton,
                setValue: value => this.Config.LogButton = value
            );

        }

        /// <summary>When loading a save, the dialogue queue is replaced with an empty one.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e) { dialogueList = new(Config.LogLimit); }

        // Handles repeatable dialogue (e.g., repeatedly interacting with objects, some NPC lines, some event lines)
        // by resetting prevAddedDialogue to null
        private void OnMenuChanged(object sender, MenuChangedEventArgs e) { if(e.NewMenu == null) prevAddedDialogue = null; }

        /// <summary>The method invoked when the game updates its state.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(15)) return; // Below code runs every quarter of a second

            // If the currently open menu is a dialogue box
            if (Game1.activeClickableMenu is DialogueBox db)
            {
                // Dialogue boxes with questions
                responses = new();
                if (db.isQuestion)
                {
                    // Converts each response from Response to string, then adds it to responses list variable
                    for (int i = 0; i < db.responses.Count; i++) responses.Add(db.responses[i].responseText);
                    // TODO: Check player's response to question
                    //this.Monitor.Log($"{responseInd}", LogLevel.Debug);
                }

                // Adding dialogue to log
                // In multi-part dialogues, the transitioningBigger check makes sure that incoming dialogue doesn't get logged too early
                if (prevAddedDialogue != db.getCurrentString() && db.transitioningBigger)
                {
                    AddToDialogueList(db.characterDialogue, db.getCurrentString(), responses);
                    prevAddedDialogue = db.getCurrentString();
                }
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Do nothing if no save file has been loaded yet
            if (!Context.IsWorldReady) return;

            // Upon pressing the Log button
            if (e.Button == Config.LogButton)
            {
                // Only open log menu when game is not paused
                if ((Game1.activeClickableMenu == null || Game1.IsMultiplayer) && !Game1.paused)
                {
                    // Set activeClickableMenu to LogMenu, passing the dialogue list
                    Game1.activeClickableMenu = new LogMenu(dialogueList, Config.RecentMessagesFirst);
                    Game1.playSound("bigSelect"); // Play "bloop bleep" sound upon opening menu
                }
                else if(Game1.activeClickableMenu is LogMenu)
                {
                    Game1.activeClickableMenu = null;
                    Game1.playSound("bigDeSelect"); // Play "bleep bloop" sound upon closing menu
                }
            }

            //Check response to an in-game dialogue question upon button click
            //if (Game1.activeClickableMenu is DialogueBox db)
            //{
            //    List<Response> responses = db.responses;
            //    int responseInd = db.selectedResponse;

            //    // Code to check if player pressed button to close dialogue box (??????)
            //    /**
            //     * Pseudocode:
            //     * if (player button == (button to open menu (E or Escape by default)) || no event is playing rn)
            //     *     Player response = responses.count - 1 (usually the leave option)
            //     */

            //    if (responseInd < 0 || responses == null || responseInd > responses.Count || responses[responseInd] == null) return;
            //}
        }

        private void AddToDialogueList(Dialogue charDiag, string dialogue, List<string> responses = null)
        {
            // Replace ^, which represent new line characters in dialogue lines
            dialogue = dialogue.Replace("^", Environment.NewLine);
            if (charDiag is null && dialogue.Split(Environment.NewLine).Length - 1 > 4)
            {
                dialogueList.enqueue(new DialogueElement(charDiag, dialogue[..dialogue.IndexOf(dialogue.Split(Environment.NewLine)[4])]));
                dialogue = dialogue[dialogue.IndexOf(dialogue.Split(Environment.NewLine)[4])..];

            }
            DialogueElement dialogueElement = new(charDiag, dialogue);
            dialogueList.enqueue(dialogueElement);

            if(responses.Count > 0)
            {
                dialogue = "> ";
                dialogue += string.Join($"{Environment.NewLine}> ", responses);
                dialogueList.enqueue(new DialogueElement(null, dialogue));
            }
        }
    }
}