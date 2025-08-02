using UnityEngine;

namespace Assets.Scripts.UI.Screens
{
    internal interface IContentImageDrawer
    {
        void DrawPicture(Texture2D texture, UIElement ui_picture_shadow);
    }
}