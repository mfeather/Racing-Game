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
    public class DriverCamera : PlayerCamera
    {
        public Vector3 Rotation { get; set; }

        public DriverCamera(Viewport viewport, GraphicsDevice graphicsDevice,
            Sprites.CModel TargetModel)
            : base(viewport, graphicsDevice,TargetModel)
        {
            Move();
            this.Rotation = ModelRotation;
            ((Sprites.Player)TargetModel).driverCam = this;
        }

        public override void Rotate(Vector3 RotationChange)
        {
            this.Rotation += RotationChange;
        }

        public override void Update()
        {
            Move();

            // Calculate the rotation matrix for the camera
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                ModelRotation.Y, ModelRotation.X, ModelRotation.Z);

            Position = new Vector3(ModelPosition.X, ModelPosition.Y + 50, ModelPosition.Z);

            // Calculate the new target using the rotation matrix
            Target = Position + Vector3.Transform(Vector3.Forward,rotation);

            // Obtain the up vector from the matrix
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);

            // Recalculate the view matrix
            View = Matrix.CreateLookAt(Position, Target, up);
        }
    }
}
