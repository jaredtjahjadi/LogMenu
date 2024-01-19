using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;

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
        private IClickableMenu prevMenu;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            dialogueList = new(Config.LogLimit);
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        /*********
        ** Private methods
        *********/
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
            // Do nothing if no save file has been loaded yet or if the game is currently paused
            if (!Context.IsWorldReady || Game1.paused) return;

            // Upon pressing the Log button
            if (e.Button == this.Config.LogButton)
            {
                // Only open log menu when game is not paused
                if ((Game1.activeClickableMenu == null || Game1.IsMultiplayer) && !Game1.paused)
                {
                    prevMenu = Game1.activeClickableMenu;
                    // Set activeClickableMenu to LogMenu, passing the dialogue list
                    Game1.activeClickableMenu = new LogMenu(this.dialogueList);
                    Game1.playSound("bigSelect"); // Play "bloop bleep" sound upon opening menu
                }
                else if(Game1.activeClickableMenu is LogMenu)
                {
                    Game1.activeClickableMenu = prevMenu is DialogueBox ? prevMenu : null;
                    Game1.playSound("bigDeSelect"); // Play "bleep bloop" sound upon closing menu
                }
            }

            // Check response to an in-game dialogue question upon button click
            //if(Game1.activeClickableMenu is DialogueBox db)
            //{
            //    responseInd = db.selectedResponse;
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