using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI
{
    class UIWindowSelectionListImage : UIWindowSelectionListTraits
    {
        protected Dictionary<string, ItemDraw> spriteCache = new Dictionary<string, ItemDraw>();

        public UIWindowSelectionListImage(UnityEngine.Events.UnityAction<string> call, string title = "") : base(call, title)
        {
        }

        public UIWindowSelectionListImage(UnityEngine.Events.UnityAction<string> call, StringKey title) : base(call, title)
        {
        }

        override public void Draw()
        {
            GenerateSpriteCache();
            base.Draw();
        }

        protected void GenerateSpriteCache()
        {
            foreach (SelectionItem item in traitItems.Values)
            {
                float aspect = 0;
                Texture2D tex = GetTexture(item.GetKey(), out aspect);
                if (tex == null) continue;

                ItemDraw spriteData = new ItemDraw();
                spriteData.color = item.GetColor();
                spriteData.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);

                spriteData.height = 3.95f;
                spriteData.width = spriteData.height * tex.width / tex.height;
                if (aspect != 0)
                {
                    spriteData.width = spriteData.height * aspect;
                }
                if (spriteData.width > 20)
                {
                    spriteData.width = 20;
                    spriteData.height = spriteData.width * tex.height / tex.width;
                }
                spriteCache.Add(item.GetKey(), spriteData);
            }
        }

        protected Texture2D GetTexture(string key, out float aspect)
        {
            Game game = Game.Get();
            aspect = 0;
            if (game.cd.tokens.ContainsKey(key))
            {
                Vector2 texPos = new Vector2(game.cd.tokens[key].x, game.cd.tokens[key].y);
                Vector2 texSize = new Vector2(game.cd.tokens[key].width, game.cd.tokens[key].height);
                return ContentData.FileToTexture(game.cd.tokens[key].image, texPos, texSize);
            }
            else if (game.cd.puzzles.ContainsKey(key))
            {
                return ContentData.FileToTexture(game.cd.puzzles[key].image);
            }
            else if (game.cd.images.ContainsKey(key))
            {
                Vector2 texPos = new Vector2(game.cd.images[key].x, game.cd.images[key].y);
                Vector2 texSize = new Vector2(game.cd.images[key].width, game.cd.images[key].height);
                return ContentData.FileToTexture(game.cd.images[key].image, texPos, texSize);
            }
            else if (game.cd.tileSides.ContainsKey(key))
            {
                aspect = game.cd.tileSides[key].aspect;
                return ContentData.FileToTexture(game.cd.tileSides[key].image);
            }
            else if (File.Exists(Path.GetDirectoryName(game.quest.qd.questPath) + Path.DirectorySeparatorChar + key))
            {
                return ContentData.FileToTexture(Path.GetDirectoryName(game.quest.qd.questPath) + Path.DirectorySeparatorChar + key);
            }
            return null;
        }

        protected override void DrawItemList()
        {
            UIElementScrollVertical itemScrollArea = new UIElementScrollVertical();
            itemScrollArea.SetLocation(UIScaler.GetHCenter(-3.5f), 2, 21, 25);
            new UIElementBorder(itemScrollArea);


            List<SelectionItemTraits> toDisplay = new List<SelectionItemTraits>(traitItems.Values);
            if (alphaSort)
            {
                toDisplay = new List<SelectionItemTraits>(alphaTraitItems.Values);
            }
            if (reverseSort)
            {
                toDisplay.Reverse();
            }

            float offset = 0;
            float xOffset = 0;
            float yOffset = 4;
            foreach (SelectionItemTraits item in toDisplay)
            {
                bool display = true;
                foreach (TraitGroup tg in traitData)
                {
                    display &= tg.ActiveItem(item);
                }

                if (!display) continue;

                if (spriteCache.ContainsKey(item.GetKey()))
                {
                    if (20 - xOffset < spriteCache[item.GetKey()].width)
                    {
                        offset += yOffset;
                        xOffset = 0;
                    }
                    xOffset = DrawItem(item.GetKey(), itemScrollArea.GetScrollTransform(), offset, xOffset, out yOffset);
                }
                else
                {
                    if (xOffset > 0) offset += yOffset;
                    xOffset = 0;
                    offset = DrawItem(item, itemScrollArea.GetScrollTransform(), offset);
                }
            }
            if (xOffset != 0)
            {
                offset += yOffset;
            }
            itemScrollArea.SetScrollSize(offset);
        }

        protected float DrawItem(string key, Transform transform, float offset, float xOffset, out float yOffset)
        {
            UIElement ui = new UIElement(transform);
            ui.SetButton(delegate { SelectItem(key); });
            ui.SetImage(spriteCache[key].sprite);
            ui.SetBGColor(spriteCache[key].color);
            ui.SetLocation(xOffset, offset, spriteCache[key].width, spriteCache[key].height);
            yOffset = spriteCache[key].height + 0.05f;
            return xOffset + spriteCache[key].width + 0.05f;
        }

        protected class ItemDraw
        {
            public Sprite sprite;
            public Color color;
            public float width;
            public float height;
        }
    }
}
