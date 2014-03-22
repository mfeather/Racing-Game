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

namespace _RacingGame.Cameras
{
    public abstract class PlayerCamera : Camera
    {
        public Vector3 Position { get; protected set; }
        public Vector3 Target { get; protected set; }
        public Vector3 ModelPosition { get; protected set; }
        public Vector3 ModelRotation { get; protected set; }
        public Sprites.CModel TargetModel { get; set; }

        public PlayerCamera(Viewport viewport, GraphicsDevice graphicsDevice,
            Sprites.CModel TargetModel)
            : base(graphicsDevice, viewport)
        {
            this.TargetModel = TargetModel;
        }

        public void Move()
        {
            this.ModelPosition = TargetModel.Position;
            this.ModelRotation = TargetModel.Rotation;
        }

        public abstract void Rotate(Vector3 RotationChange);

        public abstract override void Update();
    }
}
