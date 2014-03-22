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
    public class RaceCamera : PlayerCamera
    {
        public Vector3 PositionOffset { get; set; }
        public Vector3 TargetOffset { get; set; }
        public Vector3 RelativeCameraRotation { get; set; }
        float springiness = .10f;

        public float Springiness
        {
            get { return springiness; }
            set { springiness = MathHelper.Clamp(value, 0, 1); }
        }

        public RaceCamera(Vector3 PositionOffset, Vector3 TargetOffset,
            Vector3 RelativeCameraRotation, Viewport viewport, GraphicsDevice graphicsDevice,
            Sprites.CModel TargetModel)
            : base(viewport,graphicsDevice,TargetModel)
        {
            this.PositionOffset = PositionOffset;
            this.TargetOffset = TargetOffset;
            this.RelativeCameraRotation = RelativeCameraRotation;
            ((Sprites.Player)TargetModel).raceCam = this;
            ((Sprites.Player)TargetModel).activeCam = this;
        }

        public override void Rotate(Vector3 RotationChange)
        {
            this.RelativeCameraRotation += RotationChange;
        }

        public override void Update()
        {
            Move();

            // Sum the rotations of the model and the camera to ensure it
            // is rotated to the correct position relative to the model's
            // rotation
            Vector3 combinedRotation = ModelRotation + RelativeCameraRotation;

            // Calculate the rotation matrix for the camera
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                combinedRotation.Y, combinedRotation.X, combinedRotation.Z);

            // Calculate the position the camera would be without the spring
            // value, using the rotation matrix and target position
            Vector3 desiredPosition = ModelPosition + Vector3.Transform(PositionOffset, rotation);

            // Interpolate between the current position and desired position
            Position = Vector3.Lerp(Position, desiredPosition, Springiness);

            // Calculate the new target using the rotation matrix
            Target = ModelPosition + Vector3.Transform(TargetOffset, rotation);

            // Obtain the up vector from the matrix
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);

            // Recalculate the view matrix
            View = Matrix.CreateLookAt(Position, Target, up);
        }
    }
}
