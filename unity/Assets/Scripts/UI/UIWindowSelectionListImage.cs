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
            foreach (SelectionItem item in items)
            {
                Texture2D tex = GetTexture(item.GetKey());
                if (tex == null) continue;

                ItemDraw spriteData = new ItemDraw();
                spriteData.color = item.GetColor();

                spriteData.width = 3.95f;
                if (tex.height <= tex.width)
                {
                    spriteData.sprite = Sprite.Create(tex, new Rect(0, 0, tex.height, tex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
                }
                else
                {
                    spriteData.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
                    spriteData.width = spriteData.width * tex.width / tex.height;
                }
                spriteCache.Add(item.GetKey(), spriteData);
            }
        }

        protected Texture2D GetTexture(string key)
        {
            Game game = Game.Get();
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
                return ContentData.FileToTexture(game.cd.tileSides[key].image);
            }
            else if (File.Exists(Path.GetDirectoryName(game.quest.qd.questPath) + "/" + key))
            {
                return ContentData.FileToTexture(Path.GetDirectoryName(game.quest.qd.questPath) + "/" + key);
            }
            return null;
        }

        protected override void DrawItemList()
        {
            Game game = Game.Get();
            UIElementScrollVertical itemScrollArea = new UIElementScrollVertical();
            itemScrollArea.SetLocation(UIScaler.GetHCenter(-3.5f), 2, 21, 25);
            new UIElementBorder(itemScrollArea);

            float offset = 0;
            float xOffset = 0;
            foreach (SelectionItemTraits item in traitItems)
            {
                bool display = true;
                foreach (TraitGroup tg in traitData)
                {
                    display &= tg.ActiveItem(item);
                }

                if (!display) continue;

                if (spriteCache.ContainsKey(item.GetKey()))
                {
                    xOffset = DrawItem(item.GetKey(), itemScrollArea.GetScrollTransform(), offset, xOffset);
                }
                else
                {
                    if (xOffset > 0) offset += 4;
                    xOffset = 0;
                    offset = DrawItem(item, itemScrollArea.GetScrollTransform(), offset);
                }

                if (xOffset > 16)
                {
                    offset += 4;
                    xOffset = 0;
                }
            }
            if (xOffset != 0)
            {
                offset += 4;
            }
            itemScrollArea.SetScrollSize(offset);
        }

        protected float DrawItem(string key, Transform transform, float offset, float xOffset)
        {
            UIElement ui = new UIElement(transform);
            ui.SetButton(delegate { SelectItem(key); });
            ui.SetBGColor(spriteCache[key].color);
            ui.SetImage(spriteCache[key].sprite);
            ui.SetLocation(xOffset, offset, spriteCache[key].width, 3.95f);
            return xOffset + spriteCache[key].width + 0.05f;
        }

        protected class ItemDraw
        {
            public Sprite sprite;
            public Color color;
            public float width;
        }
    }
}
