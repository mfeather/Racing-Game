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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Diagnostics;

namespace _RacingGame.Sprites
{
    class SpeedPanel : CModel
    {
        //Class Variables
        public Vector3 direction { get; private set; }

        public SpeedPanel(Model model, Vector3 position, Vector3 rotation,
            Vector3 scale, GraphicsDevice graphicsDevice, Effect eff, Texture2D tex)
            : base(model, position, rotation, scale, graphicsDevice, eff, tex, null)
        {
            Matrix rotateMat = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            direction = Vector3.Transform(Vector3.Forward, rotateMat);
        }
    }
}
