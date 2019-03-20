using Assets.Scripts.Content;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{

    // Class for creation and management of the main menu
    public class EndGameScreen
    {
        private static readonly string IMG_BG_MOM = "ImageGreenBG";
        private static readonly string IMG_BG_DESCENT = "ImageDarkBackground";
        private static readonly StringKey STATS_WELCOME = new StringKey("val", "STATS_WELCOME");
        private static readonly StringKey STATS_ASK_VICTORY = new StringKey("val", "STATS_ASK_VICTORY");
        private static readonly StringKey STATS_ASK_VICTORY_YES = new StringKey("val", "STATS_ASK_VICTORY_YES");
        private static readonly StringKey STATS_ASK_VICTORY_NO = new StringKey("val", "STATS_ASK_VICTORY_NO");
        private static readonly StringKey STATS_ASK_RATING = new StringKey("val", "STATS_ASK_RATING");
        private static readonly StringKey STATS_ASK_COMMENTS = new StringKey("val", "STATS_ASK_COMMENTS");
        private static readonly StringKey STATS_SEND_BUTTON = new StringKey("val", "STATS_SEND_BUTTON");
        private static readonly StringKey STATS_MENU_BUTTON = new StringKey("val", "STATS_MENU_BUTTON");
        private static readonly StringKey STATS_MISSING_INFO = new StringKey("val", "STATS_MISSING_INFO");
        
        private static readonly float TitleWidth = 28;
        private static readonly float QuestionsWidth = 14;
        private static readonly float VictoryButtonWidth = 4;
        private static readonly float RatingButtonWidth = 1.5f;
        private static readonly float CommentsWidth = 20f;
        private static readonly float ActionButtonWidth = 6;

        private UIElement button_yes = null;
        private UIElement button_no = null;
        private UIElement[] rating_buttons = null;
        private UIElementEditable comments = null;
        private UIElement error_message = null;
        private string game_won="not set";
        private int selected_rating=0;


        // Create a menu which will take up the whole screen and have options.  All items are dialog for destruction.
        public EndGameScreen()
        {
            ValkyrieDebug.Log("INFO: Show end screen");

            Game game = Game.Get();

            // Investigator picture in background full screen
            UIElement bg = new UIElement(Game.ENDGAME);
            Texture2D bgTex;
            if (game.gameType.TypeName() == "MoM")
            {
                bgTex = ContentData.FileToTexture(game.cd.images[IMG_BG_MOM].image);
            }
            else if (game.gameType.TypeName() == "D2E")
            {
                bgTex = ContentData.FileToTexture(game.cd.images[IMG_BG_DESCENT].image);
            }
            else
            {
                // TODO: support a background picture for IA
                Destroyer.MainMenu();
                return;
            }
            bg.SetImage(bgTex);
            bg.SetLocation(0, 0, UIScaler.GetWidthUnits(), UIScaler.GetHeightUnits());
            
            // Welcome text
            UIElement ui = new UIElement(Game.ENDGAME);
            ui.SetLocation((UIScaler.GetWidthUnits() - TitleWidth) / 2, 1, TitleWidth, 4);
            ui.SetText(STATS_WELCOME);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetSmallFont());
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);

            float offset = 5;

            // First question : player has won ?
            ui = new UIElement(Game.ENDGAME);
            ui.SetLocation(4, offset + 2, QuestionsWidth, 2);
            ui.SetText(STATS_ASK_VICTORY);
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetSmallFont());
            ui.SetBGColor(new Color(0, 0.03f, 0f, 0.2f));

            // yes button
            button_yes = new UIElement(Game.ENDGAME);
            button_yes.SetLocation((UIScaler.GetWidthUnits() / 10) * 5, offset + 2.5f, VictoryButtonWidth, 1);
            button_yes.SetText(STATS_ASK_VICTORY_YES);
            button_yes.SetFont(game.gameType.GetHeaderFont());
            button_yes.SetFontSize(UIScaler.GetSmallFont());
            button_yes.SetButton(PressVictoryYes);
            button_yes.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(button_yes);

            // no button
            button_no = new UIElement(Game.ENDGAME);
            button_no.SetLocation((UIScaler.GetWidthUnits() / 10) * 7, offset + 2.5f, VictoryButtonWidth, 1);
            button_no.SetText(STATS_ASK_VICTORY_NO);
            button_no.SetFont(game.gameType.GetHeaderFont());
            button_no.SetFontSize(UIScaler.GetSmallFont());
            button_no.SetButton(PressVictoryNo);
            button_no.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(button_no);


            offset += 4;

            // Second question : rating ?
            ui = new UIElement(Game.ENDGAME);
            ui.SetLocation(4, offset + 2, QuestionsWidth, 2);
            ui.SetText(STATS_ASK_RATING);
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetSmallFont());
            ui.SetBGColor(new Color(0, 0.03f, 0f, 0.2f));

            // rating 1 to 10
            rating_buttons = new UIElement[10];
            for(int i=0;i<=9;i++)
            {
                rating_buttons[i] = new UIElement(Game.ENDGAME);
                rating_buttons[i].SetLocation((UIScaler.GetWidthUnits()/22)*(11+i),
                                               offset + 2.5f,
                                               RatingButtonWidth,
                                               1);
                rating_buttons[i].SetText((i+1).ToString());
                rating_buttons[i].SetFont(game.gameType.GetHeaderFont());
                rating_buttons[i].SetFontSize(UIScaler.GetSmallFont());
                rating_buttons[i].SetButtonWithParams(PressRatingButton, (i + 1).ToString());
                rating_buttons[i].SetBGColor(new Color(0, 0.03f, 0f));
                new UIElementBorder(rating_buttons[i]);
            }

            offset += 4;


            // Third question : comments ?
            ui = new UIElement(Game.ENDGAME);
            ui.SetLocation(4, offset + 2, QuestionsWidth, 2);
            ui.SetText(STATS_ASK_COMMENTS);
            ui.SetTextAlignment(TextAnchor.MiddleLeft);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetSmallFont());
            ui.SetBGColor(new Color(0, 0.03f, 0f, 0.2f));
            
            comments = new UIElementEditable(Game.ENDGAME);
            comments.SetLocation((UIScaler.GetWidthUnits()/2), offset+ 2, CommentsWidth, 5);
            comments.SetText(" ");
            comments.SetTextAlignment(TextAnchor.UpperLeft);
            comments.SetTextPadding(0.1f);
            comments.SetFont(game.gameType.GetFont());
            comments.SetFontSize(UIScaler.GetSmallFont());
            comments.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(comments);

            offset += 8.5f;
            

            // Go back to menu button
            ui = new UIElement(Game.ENDGAME);
            ui.SetLocation((UIScaler.GetWidthUnits() / 6) * 2, offset, ActionButtonWidth, 2);
            ui.SetText(STATS_MENU_BUTTON);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(MainMenu);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);

            // Publish button
            ui = new UIElement(Game.ENDGAME);
            ui.SetLocation((UIScaler.GetWidthUnits() / 6) * 3 , offset, ActionButtonWidth, 2);
            ui.SetText(STATS_SEND_BUTTON);
            ui.SetFont(game.gameType.GetHeaderFont());
            ui.SetFontSize(UIScaler.GetMediumFont());
            ui.SetButton(SendStats);
            ui.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(ui);

            offset += 3.5f;

            // Error message if any information is missing
            error_message = new UIElement(Game.ENDGAME);
            error_message.SetLocation((UIScaler.GetWidthUnits() - TitleWidth) / 2, offset, TitleWidth, 4);
            error_message.SetText(" ");
            error_message.SetFont(game.gameType.GetHeaderFont());
            error_message.SetTextAlignment(TextAnchor.MiddleCenter);
            error_message.SetFontSize(UIScaler.GetSmallFont());
            error_message.SetBGColor(Color.clear);
        }

        // Send data to Google forms
        private void SendStats()
        {
            ValkyrieDebug.Log("INFO: Go back to main menu and provide feedback");

            if (game_won=="not set" || selected_rating == 0)
            {
                error_message.SetText(STATS_MISSING_INFO,Color.red);
                error_message.SetBGColor(Color.clear);
                return;
            }
            
            Game.Get().stats.PrepareStats(game_won, selected_rating, comments.GetText());
            Game.Get().stats.PublishData();

            // todo: manage the result / error with a callback
            Destroyer.MainMenu();
        }

        private void MainMenu()
        {
            ValkyrieDebug.Log("INFO: Go back to main menu without providing feedback");
            Destroyer.MainMenu();
        }


        private void PressVictoryYes()
        {
            button_yes.SetBGColor(new Color(0.8f, 0.8f, 0.8f));
            button_yes.SetText(STATS_ASK_VICTORY_YES, Color.black);
            game_won = "true";

            button_no.SetBGColor(new Color(0, 0.03f, 0f));
            button_no.SetText(STATS_ASK_VICTORY_NO, Color.white);
        }

        private void PressVictoryNo()
        {
            button_no.SetBGColor(new Color(0.9f, 0.9f, 0.9f));
            button_no.SetText(STATS_ASK_VICTORY_NO, Color.black);
            game_won = "false";

            button_yes.SetBGColor(new Color(0, 0.03f, 0f));
            button_yes.SetText(STATS_ASK_VICTORY_YES,Color.white);
        }

        private void PressRatingButton(string value)
        {
            int i;
            UIElement button = null;

            for (i = 0; i <= 9; i++)
            {
                button = rating_buttons[i];

                if ((i+1).ToString() == value)
                {
                    button.SetBGColor(new Color(0.9f, 0.9f, 0.9f));
                    button.SetText((i + 1).ToString(), Color.black);
                    selected_rating = i + 1;
                }
                else
                {
                    button.SetBGColor(new Color(0, 0.03f, 0f));
                    button.SetText((i + 1).ToString(), Color.white);
                }
            }
        }

    }
}
