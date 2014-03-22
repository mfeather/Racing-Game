using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

namespace _RacingGame
{
    class CPlane
    {
        //Variables
        private Effect effect;
        private Vector3 position;
        private Texture2D texture;
        private float textureScale;
        VertexPositionNormalTexture[] verts;
        VertexBuffer vBuffer;

        public CPlane(Effect eff, Texture2D tex, float texScale, float modelScale, float yPos)
            : this(eff, tex, texScale, new Vector3[4]
                {new Vector3(-modelScale, yPos, -modelScale),
                new Vector3( modelScale, yPos, -modelScale),
                new Vector3(-modelScale, yPos, modelScale),
                new Vector3( modelScale, yPos, modelScale)})
        { }

        public CPlane(Effect eff, Texture2D tex, float texScale, float modelScale)
            : this(eff,tex,texScale,new Vector3[4]
                {new Vector3(-modelScale, 0.0f, -modelScale),
                new Vector3( modelScale, 0.0f, -modelScale),
                new Vector3(-modelScale, 0.0f, modelScale),
                new Vector3( modelScale, 0.0f, modelScale)})
        {}

        public CPlane(Effect eff, Texture2D tex, float texScale, Vector3[] pos)
        {
            //Initialize variables
            effect = eff;
            texture = tex;
            textureScale = texScale;
            position = new Vector3((pos[0].X+pos[3].X)/2.0f,
                                    (pos[0].Y+pos[3].Y)/2.0f,
                                    (pos[0].Z+pos[3].Z)/2.0f);

            effect.Parameters["AmbientColor"].SetValue(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            effect.Parameters["LightColor"].SetValue(new Vector4(0.2f, 0.2f, 0.9f, 1.0f));

            //Initialize vertices
            initVerts(pos);
        }

        private void initVerts(Vector3[] pos)
        {
            //Calculate the normal as the cross of two vertices
            Vector3 norm = Vector3.Cross(pos[2] ,pos[0]);
            //Set the vertex position, normal, and texture
            verts = new VertexPositionNormalTexture[4];
            verts[0] = new VertexPositionNormalTexture(pos[0], norm, new Vector2(0.0f, textureScale));
            verts[1] = new VertexPositionNormalTexture(pos[1], norm, new Vector2(textureScale, textureScale));
            verts[2] = new VertexPositionNormalTexture(pos[2], norm, new Vector2(0.0f, 0.0f));
            verts[3] = new VertexPositionNormalTexture(pos[3], norm, new Vector2(textureScale, 0.0f));
        }

        public void Draw(GraphicsDevice device, Matrix view, Matrix projection, Vector3 camPos, Vector4 pointLight)
        {
            
            //Convert the camera position to a Vector4
            Vector4 camPosition = new Vector4(camPos.X, camPos.Y, camPos.Z, 1.0f);

            //Create the world matrix from the plane position
            Matrix world = Matrix.CreateTranslation(position);

            //Set vertex buffer
            vBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), 4, BufferUsage.None);
            vBuffer.SetData(verts.ToArray());
            device.SetVertexBuffer(vBuffer);
            device.SamplerStates[0] = SamplerState.LinearClamp;

            //Draw the plane texture
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["PointLight"].SetValue(pointLight);
            effect.Parameters["ViewerPosition"].SetValue(camPosition);
            effect.Parameters["xColorTexture"].SetValue(texture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives<VertexPositionNormalTexture>
                    (PrimitiveType.TriangleStrip, verts, 0, 2);

            }
        }
    }
}
