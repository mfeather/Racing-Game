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

namespace _RacingGame.Cameras
{
    public abstract class Camera
    {

        protected GraphicsDevice GraphicsDevice { get; set; }
        public BoundingFrustum boundingFrustum { get; private set; }
        Matrix view;
        Matrix projection;
        public Viewport viewport { get; set; }

        public Camera(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, graphicsDevice.Viewport)
        { }

        public Camera(GraphicsDevice graphicsDevice, Viewport viewport)
        {
            this.GraphicsDevice = graphicsDevice;
            this.viewport = viewport;
            generatePerspectiveProjectionMatrix(MathHelper.PiOver4);
        }
        private void generatePerspectiveProjectionMatrix(float FieldOfView)
        {
            float aspectRatio = (float)viewport.Width/ (float)viewport.Height;

            this.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), aspectRatio, 0.1f, 1000000.0f);
        }
        public virtual void Update()
        {
        }

        public Matrix Projection
        {
            get { return projection; }
            protected set
            {
                projection = value;
                generateFrustum();
            }
        }
        public Matrix View
        {
            get { return view; }
            protected set
            {
                view = value;
                generateFrustum();
            }
        }

        private void generateFrustum()
        {
            Matrix viewProjection = View * Projection;
            boundingFrustum = new BoundingFrustum(viewProjection);
        }

        public bool BoundingVolumeIsInView(BoundingSphere sphere)
        {
            return (boundingFrustum.Contains(sphere) != ContainmentType.Disjoint);
        }
        public bool BoundingVolumeIsInView(BoundingBox box)
        {
            return (boundingFrustum.Contains(box) != ContainmentType.Disjoint);
        }
    }
}
