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

namespace _RacingGame.Sprites
{
    class Player : Vehicle
    {
        public int playerNum { get; set; }
        public Cameras.PlayerCamera activeCam { get; set; }
        public Cameras.RaceCamera raceCam { get; set; }
        public Cameras.DriverCamera driverCam { get; set; }
        private KeyboardState prevKeyState;

        //Constructor function
        public Player(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, List<Vector3> checkpoints, 
            Effect eff, Texture2D tex, SoundBank soundBank)
            : this(Model, Position, Rotation, Scale, graphicsDevice,
            checkpoints, eff, tex, null, 1, soundBank)
        { }

        public Player(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, List<Vector3> checkpoints, 
            Effect eff, Texture2D tex, Texture2D bump, int playerNum, SoundBank soundBank)
            : base(Model, Position, Rotation, Scale, graphicsDevice, checkpoints, eff,tex,bump,soundBank)
        {
            this.playerNum = playerNum;
            prevKeyState = Keyboard.GetState();
        }
        
        //Update function
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            switch (playerNum)
            {
                case 1:
                    bTurningLeft = keyState.IsKeyDown(Keys.A);
                    bTurningRight = keyState.IsKeyDown(Keys.D);
                    bAccelerating = keyState.IsKeyDown(Keys.W) 
                        || keyState.IsKeyDown(Keys.LeftControl);
                    bBraking = keyState.IsKeyDown(Keys.S)
                        || keyState.IsKeyDown(Keys.LeftShift);
                    if(keyState.IsKeyDown(Keys.Z)&&!prevKeyState.IsKeyDown(Keys.Z))
                        nextCamera();
                    break;
                case 2:
                    bTurningLeft = keyState.IsKeyDown(Keys.Left);
                    bTurningRight = keyState.IsKeyDown(Keys.Right);
                    bAccelerating = keyState.IsKeyDown(Keys.Up)
                        || keyState.IsKeyDown(Keys.RightControl);
                    bBraking = keyState.IsKeyDown(Keys.Down)
                        || keyState.IsKeyDown(Keys.RightShift);
                    if(keyState.IsKeyDown(Keys.RightAlt)&&!prevKeyState.IsKeyDown(Keys.RightAlt))
                        nextCamera();
                    break;
                case 3:
                    bTurningLeft = keyState.IsKeyDown(Keys.NumPad4);
                    bTurningRight = keyState.IsKeyDown(Keys.NumPad6);
                    bAccelerating = keyState.IsKeyDown(Keys.NumPad8)
                        || keyState.IsKeyDown(Keys.NumPad0);
                    bBraking = keyState.IsKeyDown(Keys.NumPad5)
                        || keyState.IsKeyDown(Keys.NumPad1);
                    if(keyState.IsKeyDown(Keys.NumPad7)&&!prevKeyState.IsKeyDown(Keys.NumPad7))
                        nextCamera();
                    break;
                case 4:
                    bTurningLeft = keyState.IsKeyDown(Keys.H);
                    bTurningRight = keyState.IsKeyDown(Keys.K);
                    bAccelerating = keyState.IsKeyDown(Keys.U)
                        || keyState.IsKeyDown(Keys.Space);
                    bBraking = keyState.IsKeyDown(Keys.J)
                        || keyState.IsKeyDown(Keys.B);
                    if(keyState.IsKeyDown(Keys.Y)&&!prevKeyState.IsKeyDown(Keys.Y))
                        nextCamera();
                    break;
            }

            prevKeyState = keyState;
            base.Update(gameTime);
        }

        private void nextCamera()
        {
            if (activeCam == raceCam)
                activeCam = driverCam;
            else
                activeCam = raceCam;
        }
    }
}
