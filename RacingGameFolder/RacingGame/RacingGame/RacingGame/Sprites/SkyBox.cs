using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SkyBox : CModel
    {
        public SkyBox(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, Effect eff,Texture2D tex)
            : base(Model, Position, Rotation, Scale, graphicsDevice, eff, tex)
        { }

        public void Draw(Matrix View, Matrix Projection,Vector3 camPos)
        {
            Vector4 camPosition = new Vector4(camPos.X, camPos.Y, camPos.Z, 1.0f);

            Position = camPos;

            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            Matrix baseWorld = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(
                Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);

            graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index]
                * baseWorld;
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    Effect effect = (Effect)meshPart.Effect;
                    effect.Parameters["World"].SetValue(localWorld * translation);
                    effect.Parameters["View"].SetValue(View);
                    effect.Parameters["Projection"].SetValue(Projection);
                }
                mesh.Draw();
            }
        }
    }
}
