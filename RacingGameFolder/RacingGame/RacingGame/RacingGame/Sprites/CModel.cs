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
    public enum direction { left, right, up, down, forward, back };

    public class CModel
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Model Model { get; private set; }
        protected Matrix[] modelTransforms;
        protected GraphicsDevice graphicsDevice;
        protected BoundingSphere boundingSphere;

        protected Effect effect;
        protected Texture2D texture,bumpTexture;

        protected Matrix translation,rotation;

        public CModel(Model model, Vector3 position, Vector3 rotation,
            Vector3 scale, GraphicsDevice graphicsDevice, Effect eff, Texture2D tex)
            : this(model, position, rotation, scale, graphicsDevice, eff, tex, null)
        { }

        public CModel(Model Model, Vector3 Position, Vector3 Rotation,
        Vector3 Scale, GraphicsDevice graphicsDevice, Effect eff,Texture2D tex, Texture2D bump)
        {
            this.Model = Model;
            modelTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;
            this.graphicsDevice = graphicsDevice;
            this.effect = eff;
            texture = tex;
            bumpTexture = bump;
            rotation = Matrix.Identity;
            translation = Matrix.Identity;
            effect.Parameters["xColorTexture"].SetValue(texture);
            if(bumpTexture != null)
                effect.Parameters["bumpMapTexture"].SetValue(bumpTexture);


            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();

            Matrix[] bones = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(bones);
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (Effect thisEffect in mesh.Effects)
                    if (thisEffect is BasicEffect)
                        ((BasicEffect)thisEffect).World = bones[mesh.ParentBone.Index];

            buildBoundingSphere();
        }

        public virtual void Update(GameTime gametime)
        { }

        public virtual void Draw(Matrix View, Matrix Projection,Vector3 camPos, Vector4 PointLight)
        {
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Vector4 camPosition = new Vector4(camPos.X, camPos.Y, camPos.Z, 1.0f);
            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            Matrix baseWorld = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(
                Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);

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
                    if (bumpTexture != null)
                    {
                        effect.Parameters["Rotation"].SetValue(rotation);
                        effect.Parameters["PointLight"].SetValue(PointLight);
                        effect.Parameters["ViewerPosition"].SetValue(camPosition);
                    }
                }
                mesh.Draw();
            }
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                // No need for rotation, as this is a sphere
                Matrix worldTransform = Matrix.CreateScale(Scale)
                        * Matrix.CreateTranslation(Position);
                BoundingSphere transformed = boundingSphere;
                transformed = transformed.Transform(worldTransform);
                return transformed;
            }
        }

        private void buildBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);
            // Merge all the model's built in bounding spheres
            foreach (ModelMesh mesh in Model.Meshes)
            {
                BoundingSphere transformed = mesh.BoundingSphere.Transform(modelTransforms[mesh.ParentBone.Index]);
                sphere = BoundingSphere.CreateMerged(sphere, transformed);
            }
            sphere.Radius *= 0.5f;
            this.boundingSphere = sphere;
        }
    }
}
