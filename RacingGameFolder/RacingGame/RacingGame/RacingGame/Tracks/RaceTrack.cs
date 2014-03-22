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

namespace _RacingGame.Tracks
{
    public class RaceTrack
    {
        //Class variables
        // List of control points
        List<TrackNode> positions;
        // List of check point positions
        public List<Vector3> checkpoints;
        // Vertex and index buffers
        VertexBuffer vBuffer;
        IndexBuffer iBuffer;
        int numVertices, numIndices;
        // Rendering variables
        GraphicsDevice graphicsDevice;
        Effect effect;
        Texture2D texture;
        // Total length of the track
        float trackLength;
        // Width of the track
        public float trackWidth;

        public RaceTrack(List<TrackNode> positions, int numDivisions, float trackWidth,
            int textureRepetitions, GraphicsDevice graphicsDevice, ContentManager content)
            : this(positions, numDivisions, trackWidth, textureRepetitions, graphicsDevice, content, null)
        { }

        public RaceTrack(List<TrackNode> positions, int numDivisions, float trackWidth,
            int textureRepetitions, GraphicsDevice graphicsDevice, ContentManager content, Effect eff)
        {
            this.graphicsDevice = graphicsDevice;
            this.trackWidth = trackWidth;

            makeCheckpoints(positions);

            //Smooth out the position list by adding more points
            this.positions = interpolatePositions(positions, numDivisions);

            texture = content.Load<Texture2D>(@"Textures\road");

            if (eff == null)
                effect = new BasicEffect(graphicsDevice);
            else
            {
                effect = eff;
                effect.Parameters["AmbientColor"].SetValue(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
                effect.Parameters["LightColor"].SetValue(new Vector4(0.2f, 0.2f, 0.9f, 1.0f));
                effect.Parameters["xColorTexture"].SetValue(texture);
            }

            createBuffers(textureRepetitions);
        }

        protected void makeCheckpoints(List<TrackNode> positions)
        {
            checkpoints = new List<Vector3>();
            foreach (TrackNode node in positions)
                checkpoints.Add(node.position);
        }

        // Adds the given number of positions between the control points specified,
        // to subdivide/smooth the path
        List<TrackNode> interpolatePositions(List<TrackNode> positions, int numDivisions)
        {
            // Create a new list of positions
            List<TrackNode> newPositions = new List<TrackNode>();

            // Between each control point...
            for (int i = 0; i < positions.Count - 1; i++)
            {
                // Add the control point to the new list
                newPositions.Add(positions[i]);

                // Add the specified number of interpolated points
                for (int j = 0; j < numDivisions; j++)
                {
                    // Determine how far to interpolate
                    float amt = (float)(j + 1) / (float)(numDivisions + 2);

                    // Find the position based on catmull-rom interpolation
                    Vector3 interpPos = Vector3.CatmullRom(
                        positions[wrapIndex(i - 1, positions.Count - 1)].position,
                        positions[i].position,
                        positions[wrapIndex(i + 1, positions.Count - 1)].position,
                        positions[wrapIndex(i + 2, positions.Count - 1)].position, amt);

                    float interpAngle = MathHelper.CatmullRom(
                        positions[wrapIndex(i - 1, positions.Count - 1)].angle,
                        positions[i].angle,
                        positions[wrapIndex(i + 1, positions.Count - 1)].angle,
                        positions[wrapIndex(i + 2, positions.Count - 1)].angle, amt);

                    // Add the new position to the new list
                    newPositions.Add(new TrackNode(interpPos,interpAngle));
                }
            }
            return newPositions;
        }

        // Wraps a number around 0 and the "max" value
        int wrapIndex(int value, int max)
        {
            while (value > max)
                value -= max;
            while (value < 0)
                value += max;
            return value;
        }

        VertexPositionNormalTexture[] createVertices(int textureRepetitions)
        {
            // Create 2 vertices for each track point
            numVertices = positions.Count * 2;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[numVertices];
            int j = 0;
            trackLength = 0;
            //For each point on the track
            for (int i = 0; i < positions.Count; i++)
            {
                // Find the index of the next position
                int next = wrapIndex(i + 1, positions.Count - 1);

                // Find the vector between the current and next position
                float length = positions[i].setForward(positions[next].position - positions[i].position);

                // Find the side vector based on the forward and up vectors
                Vector3 side = positions[i].sideVect * trackWidth;

                // Create a vertex to the left and right of the current position
                vertices[j++] = new VertexPositionNormalTexture(positions[i].position - side,
                    positions[i].upVect,
                    new Vector2(0, trackLength));
                vertices[j++] = new VertexPositionNormalTexture(positions[i].position + side,
                    positions[i].upVect,
                    new Vector2(1, trackLength));
                trackLength += length;
            }

            // Attach the end vertices to the beginning to close the loop
            vertices[vertices.Length - 1].Position = vertices[1].Position;
            vertices[vertices.Length - 2].Position = vertices[0].Position;

            // For each vertex...
            for (int i = 0; i < vertices.Length; i++)
            {
                // Bring the UV's Y coordinate back to the [0, 1] range
                vertices[i].TextureCoordinate.Y /= trackLength;
                // Tile the texture along the track
                vertices[i].TextureCoordinate.Y *= textureRepetitions;
            }
            return vertices;
        }

        int[] createIndices()
        {
            // Create indices
            numIndices = (positions.Count - 1) * 6;
            int[] indices = new int[numIndices];
            int j = 0;
            // Create two triangles between every position
            for (int i = 0; i < positions.Count - 1; i++)
            {
                int index0 = i * 2;
                indices[j++] = index0;
                indices[j++] = index0 + 1;
                indices[j++] = index0 + 2;
                indices[j++] = index0 + 2;
                indices[j++] = index0 + 1;
                indices[j++] = index0 + 3;
            }
            return indices;
        }

        //Creates the vertex and index buffers
        void createBuffers(int textureRepetitions)
        {
            // Create vertex buffer and set data
            VertexPositionNormalTexture[] vertices = createVertices(textureRepetitions);
            vBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture),
                vertices.Length, BufferUsage.WriteOnly);
            vBuffer.SetData<VertexPositionNormalTexture>(vertices);

            // Create index buffer and set data
            int[] indices = createIndices();
            iBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits,
                indices.Length, BufferUsage.WriteOnly);
            iBuffer.SetData<int>(indices);
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 camPosition, Vector4 pointLight)
        {
            // Set the vertex and index buffers to the graphics device
            graphicsDevice.SetVertexBuffer(vBuffer);
            graphicsDevice.Indices = iBuffer;

            // Set effect parameters
            if (effect is BasicEffect)
            {
                ((BasicEffect)effect).World = Matrix.Identity;
                ((BasicEffect)effect).View = View;
                ((BasicEffect)effect).Projection = Projection;
                ((BasicEffect)effect).Texture = texture;
                ((BasicEffect)effect).TextureEnabled = true;
            }
            else
            {
                effect.Parameters["World"].SetValue(Matrix.Identity);
                effect.Parameters["View"].SetValue(View);
                effect.Parameters["Projection"].SetValue(Projection);
                effect.Parameters["PointLight"].SetValue(pointLight);
                effect.Parameters["ViewerPosition"].SetValue(new Vector4(camPosition, 1.0f));
            }

            // Apply the effect
            effect.CurrentTechnique.Passes[0].Apply();

            // Draw the list of triangles
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                0, 0, numVertices, 0, numIndices / 3);
        }

        public TrackNode getNearestPoint(Vector3 pos)
        {
            TrackNode nearestPoint = positions[0];
            Vector3 toNearest = pos - nearestPoint.position;

            foreach (TrackNode node in positions)
            {
                Vector3 toPoint = pos - node.position;
                if (toPoint.Length() < toNearest.Length())
                {
                    nearestPoint = node;
                    toNearest = pos - nearestPoint.position;
                }
            }

            return nearestPoint;
        }

        public TrackNode getNearestHeight(Vector3 pos)
        {
            TrackNode nearestPoint = positions[0];
            TrackNode nextPoint = positions[1];
            Vector3 toNearest = pos - nearestPoint.position;
            Vector3 toNext = pos - nextPoint.position;

            foreach (TrackNode node in positions)
            {
                Vector3 toPoint = pos - node.position;
                if (toPoint.Length() < toNearest.Length())
                {
                    nextPoint = nearestPoint;
                    nearestPoint = node;
                    toNearest = pos - nearestPoint.position;
                    toNext = pos - nextPoint.position;
                }
                else if (toPoint.Length() < toNext.Length())
                {
                    nextPoint = node;
                    toNext = pos - nextPoint.position;
                }
            }
            Vector3 interpPoint = Vector3.Lerp(nearestPoint.position, nextPoint.position,
                (float) (toNearest.Length() / (toNearest.Length() + toNext.Length())));
            float interpAngle = MathHelper.Lerp(nearestPoint.angle, nextPoint.angle,
                (float)(toNearest.Length() / (toNearest.Length() + toNext.Length())));

            TrackNode newNode = new TrackNode(interpPoint, interpAngle);

            Vector3 interpForward = Vector3.Lerp(nearestPoint.forwardVect, nextPoint.forwardVect,
                (float)(toNearest.Length() / (toNearest.Length() + toNext.Length())));
            newNode.setForward(interpForward);

            return newNode;
        }
    }

}
