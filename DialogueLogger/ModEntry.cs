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
        private DialogueQueue<Tuple<string, string, string>> dialogueList; // The log of dialogue lines. Tuple is <name, emotion, dialogue line>
        private Dictionary<Tuple<string, string, string>, Boolean> dialogueInList;
        private ModConfig Config; // The mod configuration from the player
        private Tuple<string, string, string> prevDiag;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            dialogueList = new DialogueQueue<Tuple<string, string, string>>(this.Config.LogLimit);
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        /*********
        ** Private methods
        *********/
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            dialogueList = new DialogueQueue<Tuple<string, string, string>>(this.Config.LogLimit);
        }

        /// <summary>The method invoked when the game updates its state.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // If the currently open menu is a dialogue box
            if (Game1.activeClickableMenu is DialogueBox db)
            {
                Dialogue charDiag = db.characterDialogue;
                string resps = "";
                for (int i = 0; i < db.responses.Count; i++)
                {
                    resps += db.responses[i];
                    if (i != db.responses.Count - 1) resps += ", ";
                }
                // Interacting with objects or anything else that's not an NPC
                foreach (string dialogue in db.dialogues)
                {
                    dialogue.Replace("^", Environment.NewLine);
                    if (!string.IsNullOrEmpty(dialogue) && !dialogueList.Any(item => item.Item3 == dialogue))
                    {
                        this.Monitor.Log($"{dialogue}" + (resps.Length > 0 ? $" {resps}" : ""), LogLevel.Debug);
                        addToDialogueList(null, null, dialogue);
                    }
                }

                // Interacting with NPCs
                if (charDiag is not null)
                {
                    string currCharDiag = charDiag.getCurrentDialogue();
                    string emotion = charDiag.CurrentEmotion;
                    if (emotion == "$neutral") emotion = "Neutral";
                    else if (emotion == "$h") emotion = "Happy";
                    else if (emotion == "$s") emotion = "Sad";
                    else if (emotion == "$l") emotion = "Love";
                    else if (emotion == "$a") emotion = "Angry";
                    else emotion = "Unique";

                    // TODO: Fix issue in which repeated same-box dialogue lines will not repeat being logged
                    // Debug string format: "[DialogueLogger] <NPC name> (<emotion>): <dialogue line> <responses>"
                    // Deals with the case that dialogue continues in the same dialogue box
                    if (!string.IsNullOrEmpty(currCharDiag) && db.dialogueContinuedOnNextPage)
                    {
                        // If the specific dialogue line is not already in the dialogue list
                        if (!dialogueList.Any(item => item.Item3 == currCharDiag))
                        {
                            if (charDiag.currentDialogueIndex != 0)
                            {
                                this.Monitor.LogOnce($"{charDiag.speaker.Name}" + ((emotion == "" ? "" : $" ({emotion})") + $": {currCharDiag}") + resps, LogLevel.Debug);
                                addToDialogueList(charDiag.speaker.Name, emotion, currCharDiag);
                                prevDiag = new Tuple<string, string, string>(charDiag.speaker.Name, emotion, currCharDiag);
                            }
                        }
                        else
                        {
                            if (dialogueList.indexOf(prevDiag) != dialogueList.Count - 1 && dialogueList.indexOf(prevDiag) != -1 && charDiag.currentDialogueIndex != 0)
                            {
                                addToDialogueList(charDiag.speaker.Name, emotion, currCharDiag);
                                this.Monitor.LogOnce($"Index of {currCharDiag}: {dialogueList.indexOf(prevDiag)}", LogLevel.Debug);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return; // Do nothing if no save file has been loaded yet
            if (e.Button == this.Config.LogButton) // Below code runs when player presses the Log button
            {
                this.Monitor.Log($"Log button {e.Button} has been pressed.", LogLevel.Info);
                // Iterates through each line of dialogue in the dialouge list
                foreach(Tuple<string, string, string> dialogue in dialogueList)
                {
                    // Displays the previous X lines of dialogue in the SMAPI console (later will be replaced so that the dialogue lines display in-game)
                    if (dialogue.Item1 is null) // Non-NPCs
                        this.Monitor.Log(dialogue.Item3, LogLevel.Info);
                    else // NPCs
                        this.Monitor.Log($"{dialogue.Item1} ({dialogue.Item2}): {dialogue.Item3}", LogLevel.Info);
                }
            }
        }

        /// <summary>Raised after the menu changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if(e.NewMenu is DialogueBox db)
            {
                Dialogue charDiag = db.characterDialogue;
                string resps = "";
                for (int i = 0; i < db.responses.Count; i++)
                {
                    resps += db.responses[i];
                    if (i != db.responses.Count - 1) resps += ", ";
                }
                
                // Interacting with a non-NPC (e.g., object, any dialogue box that doesn't have a portrait)
                foreach (string dialogue in db.dialogues)
                {
                    dialogue.Replace("^", Environment.NewLine);
                    if (!string.IsNullOrEmpty(dialogue) && !dialogueList.Any(item => item.Item3 == dialogue))
                    {
                        this.Monitor.Log($"{dialogue}" + (resps.Length > 0 ? $" {resps}" : ""), LogLevel.Debug);
                        addToDialogueList(null, null, dialogue);
                    }
                }

                // Interacting with an NPC
                if (charDiag is not null)
                {
                    string currCharDiag = charDiag.getCurrentDialogue();
                    currCharDiag.Replace("^", Environment.NewLine); // Some in-game dialogue has line breaks represented by ^
                    string emotion = charDiag.CurrentEmotion;
                    if (emotion == "$neutral") emotion = "Neutral";
                    else if (emotion == "$h") emotion = "Happy";
                    else if (emotion == "$s") emotion = "Sad";
                    else if (emotion == "$l") emotion = "Love";
                    else if (emotion == "$a") emotion = "Angry";
                    else emotion = "Unique";

                    // Debug string format: "[DialogueLogger] <NPC name> (<emotion>): <dialogue line> <responses>"
                    if (!string.IsNullOrEmpty(currCharDiag))
                    {
                        this.Monitor.Log($"{charDiag.speaker.Name}" + ((emotion == "" ? "" : $" ({emotion})") + $": {currCharDiag}") + resps, LogLevel.Debug);
                        addToDialogueList(charDiag.speaker.Name, emotion, currCharDiag);
                    }
                }
            }
        }

        private void addToDialogueList(string name, string emotion, string dialogue) 
        {
            dialogueList.enqueue(Tuple.Create<string, string, string>(name, emotion, dialogue));
        }
    }
}
