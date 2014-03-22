using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _RacingGame.Menus
{
    public enum MenuState { MAIN, PLAYER, PLAY, QUIT };

    public class GameMenu
    {
        SpriteFont menuFont;
        Texture2D backgroundTexture;
        public int currentSelection { get; private set; }
        protected KeyboardState prevKeyState;
        protected List<string> menuItems = new List<string>();
        protected TimeSpan keyDelay;
        protected TimeSpan keyTimer;
        public MenuState menuState;

        public GameMenu(ContentManager content)
        {
            menuFont = content.Load<SpriteFont>(@"Fonts\Motorwerk");
            backgroundTexture = content.Load<Texture2D>(@"Textures\Menu");
            currentSelection = 0;
            keyDelay = new TimeSpan(0, 0, 0, 0, 250);
            keyTimer = new TimeSpan(0);
            prevKeyState = Keyboard.GetState();
            setMenuState(MenuState.MAIN);
        }

        public void Update(Game1 game, GameTime gameTime)
        {
            if (keyTimer.Ticks > 0)
                keyTimer -= gameTime.ElapsedGameTime;

            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Up) && keyTimer.Ticks <= 0)
            {
                keyTimer = keyDelay;
                currentSelection--;
                if (currentSelection < 0)
                    currentSelection = menuItems.Count - 1;
            }
            else if (keyState.IsKeyDown(Keys.Down) && keyTimer.Ticks <= 0)
            {
                keyTimer = keyDelay;
                currentSelection++;
                if (currentSelection > menuItems.Count - 1)
                    currentSelection = 0;
            }

            if (keyState.IsKeyDown(Keys.Enter) && keyTimer.Ticks <= 0)
            {
                keyTimer = keyDelay;
                GameState newState = makeSelection(game);
                if(newState != GameState.MENU)
                    game.setGameState(newState);
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,SpriteFont debugFont)
        {
            int width = graphicsDevice.Viewport.Width;
            int height = graphicsDevice.Viewport.Height;
            graphicsDevice.Clear(Color.Blue);

            Color itemColor;
            spriteBatch.Begin();
            spriteBatch.Draw(backgroundTexture, 
                new Rectangle(0, 0, width, height), 
                Color.White);
            for (int i = 0; i < menuItems.Count; i++)
            {
                if (i == currentSelection)
                    itemColor = Color.AntiqueWhite;
                else
                    itemColor = Color.Gray;
                spriteBatch.DrawString(menuFont, menuItems[i], new Vector2(width / 2 - 60, height / 2 + i*20), itemColor);
            }
            spriteBatch.End();
        }

        protected virtual GameState makeSelection(Game1 game)
        {
            switch (menuState)
            {
                case MenuState.MAIN:
                    switch (currentSelection)
                    {
                        case 1:
                            return GameState.QUIT;
                        default:
                            setMenuState(MenuState.PLAYER);
                            return GameState.MENU;
                    }
                case MenuState.PLAYER:
                    switch (currentSelection)
                    {
                        case 4:
                            setMenuState(MenuState.MAIN);
                            return GameState.MENU;
                        default:
                            game.numPlayers = currentSelection+1;
                            currentSelection = 0;
                            setMenuState(MenuState.MAIN);
                            return GameState.PLAY;
                    }
                default:
                    return GameState.MENU;
            }
        }

        public void setMenuState(MenuState newState)
        {
            switch (newState)
            {
                case MenuState.MAIN:
                    menuItems.Clear();
                    menuItems.Add("Play Game");
                    menuItems.Add("Quit Game");
                    break;
                case MenuState.PLAYER:
                    menuItems.Clear();
                    menuItems.Add("1 Player");
                    menuItems.Add("2 Players");
                    menuItems.Add("3 Players");
                    menuItems.Add("4 Players");
                    menuItems.Add("Back");
                    break;
                default:
                    menuItems.Clear();
                    break;
            }
            menuState = newState;
        }
    }
}
