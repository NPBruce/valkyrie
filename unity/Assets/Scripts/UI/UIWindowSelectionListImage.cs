using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;
using System.Linq;

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
            if (game.cd.TryGet(key, out TokenData tokenData))
            {
                Vector2 texPos = new Vector2(tokenData.x, tokenData.y);
                Vector2 texSize = new Vector2(tokenData.width, tokenData.height);
                return ContentData.FileToTexture(tokenData.image, texPos, texSize);
            }
            else if (game.cd.TryGet(key, out PuzzleData puzzleData))
            {
                return ContentData.FileToTexture(puzzleData.image);
            }
            else if (game.cd.TryGet(key, out ImageData imageData))
            {
                Vector2 texPos = new Vector2(imageData.x, imageData.y);
                Vector2 texSize = new Vector2(imageData.width, imageData.height);
                return ContentData.FileToTexture(imageData.image, texPos, texSize);
            }
            else if (game.cd.TryGet<TileSideData>(key, out var tileSideData))
            {
                aspect = tileSideData.aspect;
                return ContentData.FileToTexture(tileSideData.image);
            }
            else if (File.Exists(Path.GetDirectoryName(game.CurrentQuest.qd.questPath) + Path.DirectorySeparatorChar + key))
            {
                return ContentData.FileToTexture(Path.GetDirectoryName(game.CurrentQuest.qd.questPath) + Path.DirectorySeparatorChar + key);
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
            toDisplay.InsertRange(0, alwaysOnTopTraitItems.Values);

            float offset = 0;
            float xOffset = 0;
            float yOffset = 4;
            foreach (SelectionItemTraits item in allItems)
            {
                if (!traitGroups.All(tg => tg.ActiveItem(item)))
                {
                    continue;
                }

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
