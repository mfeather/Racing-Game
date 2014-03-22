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

namespace _RacingGame.Sprites
{
    class Vehicle : CModel
    {
        //Class Variables
        protected bool bTurningLeft, bTurningRight, bAccelerating, bBraking, bBoosting;
        protected float acceleration, drag, braking, topSpeed;
        protected float thrust, turnSpeed, grip;
        protected float wallSoftness;
        protected SoundBank soundBank;
        protected Cue thrustCue;
        protected List<Vector3> checkpoints;
        public List<Vector3> checkpointsLeft;
        public float gripValue { get; private set; }
        public Vector3 inertiaVect { get; set;}
        public Vector3 thrustVect { get; private set; }
        public Vector3 headingVect { get; private set; }
        public Vector3 upVect { get; set; }
        public Vector3 forwardVect { get; set; }
        public Vector3 prevRotation { get; set; }
        public Matrix upMatrix;
        public float mass;
        public int lapNum;
        public List<TimeSpan> lapTimes;
        public TimeSpan lastLapTime;
        public bool bDone;
        public bool bOnRamp;

        //Constructor function
        public Vehicle(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, List<Vector3> checkpoints, Effect eff, Texture2D tex)
            : this(Model, Position, Rotation, Scale, graphicsDevice, checkpoints, eff, tex, null, null)
        { }

        public Vehicle(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, List<Vector3> checkpoints, 
            Effect eff, Texture2D tex, Texture2D bump, SoundBank soundBank)
            : base(Model, Position, Rotation, Scale, graphicsDevice, eff, tex, bump)
        {
            bTurningLeft = bTurningRight = bAccelerating = bBraking = bBoosting = bOnRamp = false;
            this.checkpoints = checkpoints;
            this.soundBank = soundBank;
            resetCheckpoints();
            acceleration = 20.0f;
            drag = -1.0f;
            braking = -7f;
            topSpeed = 70.0f;
            thrust = 0.0f;
            turnSpeed = 0.05f;
            grip = 15.0f;
            lapNum = 1;
            mass = 100.0f;
            wallSoftness = 8.0f;
            inertiaVect = Vector3.Zero;
            thrustVect = Vector3.Zero;
            headingVect = Vector3.Forward;
            prevRotation = Vector3.Zero;
            upVect = Vector3.Up;
            upMatrix = Matrix.Identity;
            lapTimes = new List<TimeSpan>();
            lastLapTime = new TimeSpan();
            bDone = false;
            thrustCue = soundBank.GetCue("thrustSnd");
            thrustCue.Play();
            thrustCue.Pause();
        }

        //Update function
        public override void Update(GameTime gameTime)
        {
            //Calculate time past
            float timePassed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Determine on which axes the ship should be rotated, if any
            Vector3 rotChange = new Vector3(0, 0, 0);

            if (bTurningLeft)
                rotChange += new Vector3(0, 1, 0);
            if (bTurningRight)
                rotChange += new Vector3(0, -1, 0);
            Rotation += rotChange * turnSpeed;

            //Apply acceleration or braking
            if (bBraking)
                thrust = braking;
            else if (bAccelerating)
            {
                thrust = acceleration;
                if (thrustCue.IsPaused)
                    thrustCue.Resume();
            }
            else
            {
                thrust = 0.0f;
                if (thrustCue.IsPlaying)
                    thrustCue.Pause();
            }

            // Determine what direction to thrust in
            Matrix rotation = Matrix.CreateFromAxisAngle(upVect,Rotation.Y);

            //Calculate the heading
            headingVect = Vector3.Transform(Vector3.Forward, rotation);

            //Calculate the thrust vector
            thrustVect = headingVect * thrust * timePassed;
            
            //If the vehicle is moving, add the grip
            if (inertiaVect.Length() > 0)
            {
                //Calculate the grip value
                gripValue = grip * (Vector3.Dot(headingVect, Vector3.Normalize(inertiaVect)) - 1) / 2.0f;

                //Add the grip vector to the inertia vector
                inertiaVect += -headingVect * gripValue * timePassed;
            }

            //Add the thrust vector to the inertia vector
            inertiaVect += thrustVect;

            //Add the drag to the inertia vector if the vehicle is moving
            if(inertiaVect.Length() > 0)
                inertiaVect += drag * Vector3.Normalize(inertiaVect) * timePassed;

            //Clamp the inertia to the topSpeed
            if (inertiaVect.Length() > topSpeed)
                inertiaVect = Vector3.Normalize(inertiaVect) * topSpeed;

            // Move in the direction dictated by our inertia vector
            Position += inertiaVect;
        }
        public override void Draw(Matrix View, Matrix Projection, Vector3 camPos, Vector4 PointLight)
        {
            Vector4 camPosition = new Vector4(camPos.X, camPos.Y, camPos.Z, 1.0f);

            rotation = Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);

            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            Matrix baseWorld = Matrix.CreateScale(Scale)
                * rotation * upMatrix
                * Matrix.CreateTranslation(Position);

            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

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
                    effect.Parameters["Rotation"].SetValue(rotation);
                    effect.Parameters["PointLight"].SetValue(PointLight);
                    effect.Parameters["ViewerPosition"].SetValue(camPosition);
                    effect.Parameters["AmbientColor"].SetValue(new Vector4(0.8f, 0.8f, 0.8f, 0.8f));
                }
                mesh.Draw();
            }
        }
        
        //Set the current position on the track and check for collisions
        public void setTrackNode(Tracks.TrackNode node,float trackWidth)
        {
            //Get the 2D vector from the node to the vehicle
            Vector2 nodeDist2D = new Vector2(Position.X - node.position.X, Position.Z - node.position.Z);
            //upVect = node.upVect;
            //float newSpeed = inertiaVect.Length();
            //if (inertiaVect.Length() != 0)
            //{
            //    Vector3 newSideVect = Vector3.Cross(Vector3.Normalize(inertiaVect), node.upVect);
            //    Vector3 newForward = Vector3.Cross(node.upVect, newSideVect);
            //    inertiaVect = newForward * newSpeed;
            //}
            //Set the height to be above the nearest node
            float nodeHeight = node.position.Y + 70.0f;
            if (Position.Y - 10 <= nodeHeight)
                Position = new Vector3(Position.X, nodeHeight, Position.Z);
            //If the vehicle is already above the track, lower it slowly
            else if(!bOnRamp)
            {
                Position = new Vector3(Position.X,
                    Position.Y - 10,
                    Position.Z);
                inertiaVect += Vector3.Down * 0.5f;
            }
            
            //Bounce off edge of track
            //If the length is more than the width, then the vehicle is off the track
            if (nodeDist2D.Length() > trackWidth - boundingSphere.Radius)
            {
                //Undo the last frame of movement to keep the vehicle on the track
                Position -= inertiaVect;
                //Scale the speed based upon the angle of the bounce
                float bounceScale = (float) Math.Pow(Math.Abs(Vector3.Dot(Vector3.Normalize(inertiaVect),
                                            Vector3.Normalize(node.forwardVect))),wallSoftness);
                //The angle to bounce is twice the angle between inertia and forward of the track
                float bounceAngle = 2.0f * (float)Math.Acos(Vector3.Dot(
                                            Vector3.Normalize(inertiaVect),
                                            Vector3.Normalize(node.forwardVect)));
                //Side dot determines which side of the track the vehicle is on
                float sideDot = Vector2.Dot(Vector2.Normalize(nodeDist2D),
                                   Vector2.Normalize(new Vector2(node.sideVect.X,node.sideVect.Z)));
                //If the vehicle is on the right side, use a negative bounce angle
                if (sideDot > 0)
                    bounceAngle *= -1;
                //Create a rotation matrix around the up vector by the bounce angle
                Matrix bounceMatix = Matrix.CreateFromAxisAngle(node.upVect, bounceAngle);
                //Rotate the inertia vector by the bounce matrix
                inertiaVect = Vector3.Transform(inertiaVect, bounceMatix);
                //Apply the new inertia vector
                Position += inertiaVect;
                //Scale the new inertia by the bounce scale
                inertiaVect *= bounceScale;

                upMatrix = Matrix.CreateFromAxisAngle(Vector3.Cross(Vector3.Up, node.upVect),
                                                        (float)Math.Acos(Vector3.Dot(Vector3.Up, node.upVect)));
                //Play wall sound
                soundBank.PlayCue("wallSnd");
            }
        }

        public void checkForCheckpoint(Game1 game, TimeSpan raceTimer)
        {
            Vector2 toPoint = new Vector2(Position.X - checkpointsLeft[0].X,
                Position.Z- checkpointsLeft[0].Z);
            if (toPoint.Length() < 1000)
                checkpointsLeft.RemoveAt(0);
            if (checkpointsLeft.Count == 0)
            {
                if (lapNum < 3)
                    lapNum++;
                else
                {
                    lastLapTime = TimeSpan.FromMilliseconds(raceTimer.TotalMilliseconds);
                    bDone = true;
                }
                TimeSpan lapTime = TimeSpan.FromMilliseconds(raceTimer.TotalMilliseconds);
                lapTime = lapTime.Subtract(lastLapTime);
                lastLapTime = TimeSpan.FromMilliseconds(raceTimer.TotalMilliseconds);
                lapTimes.Add(lapTime);
                resetCheckpoints();
            }
        }

        public void resetCheckpoints()
        {
            checkpointsLeft = new List<Vector3>(checkpoints);
        }
    }
}
