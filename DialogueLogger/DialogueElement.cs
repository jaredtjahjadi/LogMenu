using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace DialogueLogger
{
    internal class DialogueElement
    {
        public readonly Dialogue charDiag;
        public readonly string text;
        private Rectangle bounds;

        public DialogueElement(Dialogue charDiag, string text)
        {
            this.charDiag = charDiag;
            // Text width differs depending on if a character is saying 
            this.text = Game1.parseText(text, Game1.smallFont, 1000 - IClickableMenu.borderWidth - 128);
            bounds = new Rectangle(8 * Game1.pixelZoom, 4 * Game1.pixelZoom, 9 * Game1.pixelZoom, 9 * Game1.pixelZoom);
        }

        

        public virtual void draw(SpriteBatch b, int slotX, int slotY)
        {
            if(charDiag is not null)
            {
                // NPC name
                Utility.drawBoldText(b, charDiag.speaker.displayName, Game1.dialogueFont, new Vector2(slotX + bounds.X, slotY + bounds.Y / 2), Game1.textColor, 0.75f);
                // NPC dialogue text
                Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2(slotX + bounds.X, slotY + 42), Game1.textColor);
            }
            else
            {
                Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2(slotX + bounds.X, slotY + bounds.Y), Game1.textColor);
            }
            // TODO: Responses
            
        }
    }
}
