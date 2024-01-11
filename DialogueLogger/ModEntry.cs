using System;
using System.Collections.Generic;
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
        private DialogueQueue<Tuple<string, string, string, List<string>>> dialogueList; // The log of dialogue lines. Tuple is <name, emotion, dialogue line>
        // private Dictionary<Tuple<string, string, string>, Boolean> dialogueInList;
        private ModConfig Config; // The mod configuration from the player
        private string prevLoggedDialogue;
        private string prevAddedDialogue;
        private List<string> responses;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            dialogueList = new DialogueQueue<Tuple<string, string, string, List<string>>>(this.Config.LogLimit);
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            //helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>When loading a save, the dialogue queue is replaced with an empty one.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            dialogueList = new DialogueQueue<Tuple<string, string, string, List<string>>>(this.Config.LogLimit);
        }

        /// <summary>The method invoked when the game updates its state.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(15)) return; // Below code runs every quarter of a second

            // If the currently open menu is a dialogue box
            if (Game1.activeClickableMenu is DialogueBox db)
            {
                // In multi-part dialogues, the transitioningBigger check makes sure that incoming dialogue doesn't get logged too early
                if (prevLoggedDialogue != db.getCurrentString() && db.transitioningBigger)
                {
                    this.Monitor.Log(db.getCurrentString(), LogLevel.Debug);
                    this.prevLoggedDialogue = db.getCurrentString();
                }

                // Dialogue boxes with questions
                responses = new();
                if (db.isQuestion)
                {
                    // Add each response to responses list variable
                    for (int i = 0; i < db.responses.Count; i++) responses.Add(db.responses[i].responseText);
                    // TODO: Get player's response to question
                }

                // NPC information (character name / emotion)
                Dialogue charDiag = db.characterDialogue;
                string charName = null;
                string charEmotion = null;
                if(charDiag is not null)
                {
                    charName = charDiag.speaker.Name;
                    charEmotion = charDiag.CurrentEmotion;
                }

                // TODO: Add repeatable dialogue to log (e.g., interacting with objects, most of Gus's lines)

                // Adding dialogue to log
                if(prevAddedDialogue != db.getCurrentString() && db.transitioningBigger)
                {
                    addToDialogueList(charName, charEmotion, db.getCurrentString(), responses);
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
                this.Monitor.Log($"Log button {e.Button} has been pressed.", LogLevel.Debug);
                // Iterates through each line of dialogue in the dialouge list
                foreach(Tuple<string, string, string, List<string>> dialogue in dialogueList)
                {
                    // Displays the previous X lines of dialogue in the SMAPI console (later will be replaced so that the dialogue lines display in-game)
                    if (dialogue.Item1 is null) // Non-NPCs
                        this.Monitor.Log($"{dialogue.Item3} {(dialogue.Item4.Count > 0 ? "- " + string.Join(',', dialogue.Item4) : "")}", LogLevel.Info);
                    else // NPCs
                        this.Monitor.Log($"{dialogue.Item1} ({dialogue.Item2}): {dialogue.Item3} {(dialogue.Item4.Count > 0 ? "- " + string.Join(',', dialogue.Item4) : "")}", LogLevel.Info);
                }
            }
        }

        private void addToDialogueList(string name, string emotion, string dialogue, List<string> responses = null)
        {
            Tuple<string, string, string, List<string>> dialogueTuple = new(name, emotion, dialogue, responses);
            dialogueList.enqueue(dialogueTuple);
        }
    }
}