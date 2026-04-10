// unity/Assets/Scripts/UI/Screens/ContentImportScreen.cs
using System.IO;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    public class ContentImportScreen
    {
        private readonly string gameType;
        private readonly bool needImport;

        private static readonly string MOM_APP_URL_STEAM = "https://store.steampowered.com/app/478980/Mansions_of_Madness/";
        private static readonly string D2E_APP_URL_STEAM = "https://store.steampowered.com/app/477200/Descent_Road_to_Legend/";

        private static readonly StringKey IMPORT_SCREEN_TITLE    = new StringKey("val", "IMPORT_SCREEN_TITLE");
        private static readonly StringKey REIMPORT_SCREEN_TITLE  = new StringKey("val", "REIMPORT_SCREEN_TITLE");
        private static readonly StringKey IMPORT_SCREEN_SUBTITLE = new StringKey("val", "IMPORT_SCREEN_SUBTITLE");
        private static readonly StringKey IMPORT_FROM_STEAM      = new StringKey("val", "IMPORT_FROM_STEAM");
        private static readonly StringKey REIMPORT_FROM_STEAM    = new StringKey("val", "REIMPORT_FROM_STEAM");
        private static readonly StringKey IMPORT_FROM_STEAM_DESC = new StringKey("val", "IMPORT_FROM_STEAM_DESC");
        private static readonly StringKey IMPORT_STEAM_NOT_FOUND = new StringKey("val", "IMPORT_STEAM_NOT_FOUND");
        private static readonly StringKey IMPORT_INSTALL_STEAM_LINK = new StringKey("val", "IMPORT_INSTALL_STEAM_LINK");
        private static readonly StringKey IMPORT_LOCATE          = new StringKey("val", "IMPORT_LOCATE");
        private static readonly StringKey REIMPORT_LOCATE        = new StringKey("val", "REIMPORT_LOCATE");
        private static readonly StringKey IMPORT_LOCATE_DESC     = new StringKey("val", "IMPORT_LOCATE_DESC");
        private static readonly StringKey IMPORT_FROM_ZIP        = new StringKey("val", "IMPORT_FROM_ZIP");
        private static readonly StringKey REIMPORT_FROM_ZIP      = new StringKey("val", "REIMPORT_FROM_ZIP");
        private static readonly StringKey IMPORT_FROM_ZIP_DESC   = new StringKey("val", "IMPORT_FROM_ZIP_DESC");
        private static readonly StringKey IMPORT_ANDROID_ONLY_ZIP = new StringKey("val", "IMPORT_ANDROID_ONLY_ZIP");
        private static readonly StringKey D2E_NAME               = new StringKey("val", "D2E_NAME");
        private static readonly StringKey MOM_NAME               = new StringKey("val", "MOM_NAME");

        public ContentImportScreen(string gameType, bool needImport)
        {
            this.gameType = gameType;
            this.needImport = needImport;
            Game.Get().contentImport = this;
            Draw();
        }

        private void Draw()
        {
            Destroyer.Destroy();

            float centerX = (UIScaler.GetWidthUnits() - 30) / 2;
            bool steamAvailable = ImportManager.ImportAvailable(gameType);
            bool isAndroid = Application.platform == RuntimePlatform.Android;
            string gameName = GetGameName();
            string steamUrl = gameType.Equals(ValkyrieConstants.typeDescent) ? D2E_APP_URL_STEAM : MOM_APP_URL_STEAM;

            // Back button
            UIElement back = new UIElement();
            back.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            back.SetText(CommonStringKeys.BACK, Color.red);
            back.SetFontSize(UIScaler.GetMediumFont());
            back.SetButton(OnBack);
            back.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(back, Color.red);

            // Title
            UIElement title = new UIElement();
            title.SetLocation(centerX, 1, 30, 2);
            title.SetText(needImport ? IMPORT_SCREEN_TITLE : REIMPORT_SCREEN_TITLE);
            title.SetFontSize(UIScaler.GetMediumFont());
            title.SetBGColor(Color.clear);

            // Subtitle
            UIElement subtitle = new UIElement();
            subtitle.SetLocation(centerX, 3.5f, 30, 2.5f);
            subtitle.SetText(string.Format(IMPORT_SCREEN_SUBTITLE.Translate(), gameName));
            subtitle.SetFontSize(UIScaler.GetSmallFont());
            subtitle.SetBGColor(Color.clear);

            DrawSteamCard(centerX, 7f, steamAvailable, isAndroid, steamUrl);
            DrawLocateCard(centerX, 13f, isAndroid);
            DrawZipCard(centerX, 17f);
        }

        private void DrawSteamCard(float x, float y, bool steamAvailable, bool isAndroid, string steamUrl)
        {
            bool active = steamAvailable && !isAndroid;
            Color cardColor = active ? Color.white : Color.grey;

            string description = isAndroid
                ? IMPORT_ANDROID_ONLY_ZIP.Translate()
                : IMPORT_FROM_STEAM_DESC.Translate();

            string cardText = (needImport ? IMPORT_FROM_STEAM : REIMPORT_FROM_STEAM).Translate()
                + System.Environment.NewLine + description;

            UIElement card = new UIElement();
            card.SetLocation(x, y, 30, 3f);
            card.SetText(cardText, cardColor);
            card.SetFontSize(UIScaler.GetSmallFont());
            card.SetBGColor(new Color(0, 0.03f, 0f));
            if (active)
                card.SetButton(OnImportFromSteam);
            new UIElementBorder(card, cardColor);

            if (!steamAvailable && !isAndroid)
            {
                UIElement notFound = new UIElement();
                notFound.SetLocation(x, y + 3.2f, 14, 1.3f);
                notFound.SetText(IMPORT_STEAM_NOT_FOUND, Color.grey);
                notFound.SetFontSize(UIScaler.GetSmallFont());
                notFound.SetBGColor(new Color(0, 0.03f, 0f));

                UIElement installLink = new UIElement();
                installLink.SetLocation(x + 15, y + 3.2f, 15, 1.3f);
                installLink.SetText(IMPORT_INSTALL_STEAM_LINK, Color.red);
                installLink.SetFontSize(UIScaler.GetSmallFont());
                installLink.SetBGColor(new Color(0, 0.03f, 0f));
                installLink.SetButton(delegate { Application.OpenURL(steamUrl); });
                new UIElementBorder(installLink, Color.red);
            }
        }

        private void DrawLocateCard(float x, float y, bool isAndroid)
        {
            Color cardColor = isAndroid ? Color.grey : Color.white;
            string cardText = (needImport ? IMPORT_LOCATE : REIMPORT_LOCATE).Translate()
                + System.Environment.NewLine + IMPORT_LOCATE_DESC.Translate();

            UIElement card = new UIElement();
            card.SetLocation(x, y, 30, 3f);
            card.SetText(cardText, cardColor);
            card.SetFontSize(UIScaler.GetSmallFont());
            card.SetBGColor(new Color(0, 0.03f, 0f));
            if (!isAndroid)
                card.SetButton(OnLocateManually);
            new UIElementBorder(card, cardColor);
        }

        private void DrawZipCard(float x, float y)
        {
            string cardText = (needImport ? IMPORT_FROM_ZIP : REIMPORT_FROM_ZIP).Translate()
                + System.Environment.NewLine + IMPORT_FROM_ZIP_DESC.Translate();

            UIElement card = new UIElement();
            card.SetLocation(x, y, 30, 3f);
            card.SetText(cardText);
            card.SetFontSize(UIScaler.GetSmallFont());
            card.SetButton(OnImportFromZip);
            card.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(card);
        }

        private void OnBack()
        {
            Game.Get().contentImport = null;
            Game.Get().gameSelect.Draw();
        }

        private void OnImportFromSteam()
        {
            ImportManager.Import(gameType, null, OnImportComplete);
        }

        private void OnLocateManually()
        {
            string appFilename = gameType.Equals(ValkyrieConstants.typeDescent)
                ? "Road to Legend"
                : "Mansions of Madness";

#if UNITY_STANDALONE_OSX
            string extension = "app";
#else
            string extension = "exe";
#endif

            string pickedPath = NativeFileDialog.OpenFilePanel("Locate " + appFilename, "", extension);
            if (string.IsNullOrEmpty(pickedPath)) return;

            string path;
            if (Application.platform == RuntimePlatform.OSXPlayer)
                path = Path.Combine(pickedPath, "Contents/Resources/Data");
            else
                path = Path.Combine(Path.GetDirectoryName(pickedPath), appFilename + "_Data");

            ValkyrieDebug.Log("Using path: " + path);
            if (!Directory.Exists(path)) return;

            ImportManager.Import(gameType, path, OnImportComplete);
        }

        private void OnImportFromZip()
        {
            ImportManager.ImportZip(gameType, OnImportComplete);
        }

        private void OnImportComplete()
        {
            Game.Get().contentImport = null;
            Game.Get().gameSelect.Draw();
        }

        private string GetGameName()
        {
            if (gameType.Equals(ValkyrieConstants.typeDescent))
                return D2E_NAME.Translate();
            if (gameType.Equals(ValkyrieConstants.typeMom))
                return MOM_NAME.Translate();
            return gameType;
        }

        public void Update()
        {
            ImportManager.Update();
        }
    }
}
