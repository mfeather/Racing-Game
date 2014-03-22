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

namespace _RacingGame.Sprites
{
    class Ramp : SpeedPanel
    {
        public Vector3 upVect;

        public Ramp(Model model, Vector3 position, Vector3 rotation,
            Vector3 scale, GraphicsDevice graphicsDevice, Effect eff, Texture2D tex)
            : base(model, position, rotation, scale, graphicsDevice, eff, tex)
        {
            Vector3 forward = Vector3.Transform(Vector3.Forward,
                Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z));
            upVect = Vector3.Cross(Vector3.Cross(forward, Vector3.Up), forward);
            upVect.Normalize();
        }

        public override void Draw(Matrix View, Matrix Projection, Vector3 camPos, Vector4 PointLight)
        {
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Vector4 camPosition = new Vector4(camPos.X, camPos.Y, camPos.Z, 1.0f);
            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            Matrix baseWorld = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(
                Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);

            Matrix localWorld = modelTransforms[this.Model.Meshes[0].ParentBone.Index]
                * baseWorld;
            foreach (ModelMeshPart meshPart in this.Model.Meshes[0].MeshParts)
            {
                Effect effect = (Effect)meshPart.Effect;
                effect.Parameters["World"].SetValue(localWorld * translation);
                effect.Parameters["View"].SetValue(View);
                effect.Parameters["Projection"].SetValue(Projection);
            }
            this.Model.Meshes[0].Draw();
        }
    }
}
