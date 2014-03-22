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
    public class TrackNode
    {
        //Class variables
        public Vector3 position;
        public Vector3 upVect;
        public Vector3 forwardVect;
        public Vector3 sideVect;
        public Matrix rotation;
        public float angle;

        //Constructor
        public TrackNode(Vector3 Position, float Angle)
        {
            position = Position;
            angle = Angle;
        }

        public float setForward(Vector3 newForward)
        {
            float length = newForward.Length();
            forwardVect = Vector3.Normalize(newForward);
            rotation = Matrix.CreateFromAxisAngle(forwardVect, angle);
            upVect =  Vector3.Cross(Vector3.Cross(forwardVect,Vector3.Up),forwardVect);
            upVect = Vector3.Transform(upVect, rotation);
            sideVect = -Vector3.Cross(forwardVect, upVect);
            rotation.Up = upVect;
            rotation.Forward = forwardVect;
            rotation.Right = sideVect;
            return length;
        }
    }
}
