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

namespace _RacingGame
{
    public enum GameState { MENU, PAUSE, PLAY, OVER, QUIT };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //Class Variables
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont debugFont;
        SpriteFont clockFont;
        TimeSpan raceTimer = new TimeSpan(0, 0, 0);
        GameState currentGameState;
        Menus.GameMenu mainMenu;
        public int numPlayers = 1;
        SpriteFont menuFont;
        Texture2D backgroundTexture;

        //Sound variables
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        //Model variables
        List<Sprites.CModel> models = new List<Sprites.CModel>();
        List<Sprites.Player> players = new List<Sprites.Player>();
        List<Sprites.Vehicle> vehicles = new List<Sprites.Vehicle>();
        List<Sprites.SpeedPanel> panels = new List<Sprites.SpeedPanel>();
        List<Sprites.Ramp> ramps = new List<Sprites.Ramp>();
        Sprites.SkyBox skyBox;
        Texture2D floorTexture, playerTexture, playerBumpTexture, skyTexture;
        Effect bumpEffect, lightEffect, unlitEffect,directEffect;
        Vector4 pointLightPosition;
        Terrain terrain;
        Tracks.RaceTrack track1;

        //Camera variables
        List<Cameras.Camera> cameras;
        const int CAMERA_SPACING = 1;
        Viewport defaultViewport;


        //Constructor function
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        // Called when the game should load its content
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Store the default viewport to restore it later
            defaultViewport = GraphicsDevice.Viewport;

            cameras = new List<Cameras.Camera>();

            //Sound
            audioEngine = new AudioEngine(@"Content\Sounds\GameAudio.xgs");
            waveBank = new WaveBank(audioEngine, @"Content\Sounds\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, @"Content\Sounds\Sound Bank.xsb");

            //Load textures
            floorTexture = Content.Load<Texture2D>(@"Textures\floor");
            playerTexture = Content.Load<Texture2D>(@"Textures\Speeder_diff");
            playerBumpTexture = Content.Load<Texture2D>(@"Textures\Speeder_bump");
            skyTexture = Content.Load<Texture2D>(@"Textures\skybox_texture2");
            menuFont = Content.Load<SpriteFont>(@"Fonts\Motorwerk");
            backgroundTexture = Content.Load<Texture2D>(@"Textures\Menu");

            //Load effects
            bumpEffect = Content.Load<Effect>(@"Effects\BumpLightEffect");
            lightEffect = Content.Load<Effect>(@"Effects\LightEffect2");
            unlitEffect = Content.Load<Effect>(@"Effects\UnlitEffect");
            directEffect = Content.Load<Effect>(@"Effects\DirectionalEffect");
            debugFont = Content.Load<SpriteFont>(@"Fonts\Arial");
            clockFont = Content.Load<SpriteFont>(@"Fonts\Clock");

            //Start the game in the menu state
            setGameState( GameState.MENU);
        }

        // Called when the game should update itself
        protected override void Update(GameTime gameTime)
        {
            //Store the keyboard state
            KeyboardState keyboardState = Keyboard.GetState();

            switch (currentGameState)
            {
                case GameState.PLAY:
                    //Allow the game to quit by pressing "Esc"
                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        setGameState(GameState.MENU);
                        break;
                    }
                    //Update each model
                    foreach (Sprites.CModel model in models)
                    {
                        if (model is Sprites.Vehicle)
                        {
                            if (!((Sprites.Vehicle)model).bDone)
                                model.Update(gameTime);
                        }
                        else
                            model.Update(gameTime);
                    }
                    //Update each camera
                    foreach (Cameras.Camera camera in cameras)
                    {
                        if (!((Sprites.Vehicle)((Cameras.PlayerCamera)camera).TargetModel).bDone)
                            camera.Update();
                    }

                    //Tick the race timer
                    raceTimer += gameTime.ElapsedGameTime;

                    //Detect model collisions
                    detectCollision();

                    //Set the player position on the track and check for collision with the edge
                    //and checkpoints
                    foreach (Sprites.Player player in players)
                    {
                        player.setTrackNode(track1.getNearestHeight(player.Position), track1.trackWidth);
                        player.checkForCheckpoint(this,raceTimer);
                        bool bOnRamp = false;
                        foreach (Sprites.Ramp ramp in ramps)
                            if (ramp.BoundingSphere.Intersects(player.BoundingSphere))
                                bOnRamp = true;
                        player.bOnRamp = bOnRamp;
                    }

                    if(players.TrueForAll(isDone))
                            setGameState(GameState.OVER);
                    break;
                case GameState.MENU:
                    mainMenu.Update(this,gameTime);
                    break;
                case GameState.OVER:
                    //Return to menu when Enter is pressed
                    if (keyboardState.IsKeyDown(Keys.Enter))
                    {
                        setGameState(GameState.MENU);
                        break;
                    }
                    break;
                default:
                    break;
            }
            audioEngine.Update();
            base.Update(gameTime);
        }

        // Called when the game should draw itself
        protected override void Draw(GameTime gameTime)
        {
            switch (currentGameState)
            {
                case GameState.PLAY:
                    //Clear the screen to black to fill the viewport spacer
                    GraphicsDevice.Clear(Color.Black);

                    //Draw each camera's viewport
                    foreach (Sprites.Player player in players)
                    {
                        //Store the active camera for the player
                        Cameras.PlayerCamera camera = player.activeCam;

                        //Set the viewport for the current camera
                        GraphicsDevice.Viewport = camera.viewport;
                        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                        GraphicsDevice.BlendState = BlendState.Opaque;

                        if (!player.bDone)
                        {
                            //Draw the skyBox
                            skyBox.Draw(camera.View, camera.Projection, player.Position);

                            //Draw each model that is within the frustum
                            foreach (Sprites.CModel model in models)
                                if (!(camera is Cameras.DriverCamera && model == player))
                                    if (camera.BoundingVolumeIsInView(model.BoundingSphere))
                                    {
                                        if (model is Sprites.Vehicle)
                                        {
                                            if (!((Sprites.Vehicle)model).bDone)
                                            {
                                                //Set the position of the point light
                                                pointLightPosition = new Vector4(
                                                    ((Sprites.Vehicle)model).Position.X - 100.0f,
                                                    ((Sprites.Vehicle)model).Position.Y + 100.0f,
                                                    ((Sprites.Vehicle)model).Position.Z - 100.0f, 0);

                                                model.Draw(camera.View, camera.Projection,
                                                    camera.Position, pointLightPosition);
                                            }

                                        }
                                        else
                                            model.Draw(camera.View, camera.Projection,
                                                camera.Position, pointLightPosition);
                                    }

                            //Draw the terrain
                            terrain.Draw(GraphicsDevice, camera.View, camera.Projection);


                            //Draw the track
                            track1.Draw(camera.View, camera.Projection, camera.Position,
                                new Vector4(player.Position.X, player.Position.Y + 50.0f, player.Position.Z, 1));
                        }
                        //Draw the speed HUD in the viewport
                        spriteBatch.Begin();
                        spriteBatch.DrawString(debugFont, "Speed: " + (int)(player.inertiaVect.Length() * 10)
                            + " mph", new Vector2(10, camera.viewport.Height - 40), Color.Yellow);
                        spriteBatch.DrawString(debugFont, "Lap " + player.lapNum + "/3",
                            new Vector2(10, camera.viewport.Height - 65), Color.Yellow);
                        for (int i = 0; i < player.lapTimes.Count; i++)
                            spriteBatch.DrawString(clockFont,
                                String.Format("{0:00}", player.lapTimes[i].Minutes)
                                + ":" + String.Format("{0:00}", player.lapTimes[i].Seconds)
                                + ":" + String.Format("{0:000}", player.lapTimes[i].Milliseconds),
                                new Vector2(camera.viewport.Width - 160, camera.viewport.Height - (50 + 30 * i)),
                                Color.Gray);

                        spriteBatch.End();

                    }

                    Vector2 timerPos;
                    if (numPlayers == 1)
                        timerPos = new Vector2(320, 10);
                    else
                        timerPos = new Vector2(320, 220);

                    //Reset the default viewport to draw across all viewports
                    GraphicsDevice.Viewport = defaultViewport;

                    //Draw the timer HUD across all viewports
                    spriteBatch.Begin();
                    spriteBatch.DrawString(clockFont,
                        String.Format("{0:00}", raceTimer.Minutes)
                        + ":" + String.Format("{0:00}", raceTimer.Seconds)
                        + ":" + String.Format("{0:000}", raceTimer.Milliseconds),
                        timerPos, Color.LightGray);
                    spriteBatch.End();
                    break;
                case GameState.MENU:
                    mainMenu.Draw(GraphicsDevice,spriteBatch,debugFont);
                    break;
                case GameState.OVER:
                    GraphicsDevice.Clear(Color.Blue);

                    Color timeColor = Color.Yellow;
                    Color nameColor = Color.AntiqueWhite;
                    spriteBatch.Begin();
                    spriteBatch.Draw(backgroundTexture,
                        new Rectangle(0, 0, 
                            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                        Color.White);
                    for (int i = 0; i < players.Count; i++)
                    {
                        spriteBatch.DrawString(debugFont, 
                            "Player " + players[i].playerNum,
                            new Vector2(GraphicsDevice.Viewport.Width / 2 - 100,
                                GraphicsDevice.Viewport.Height / 2 + i * 40 - 150),
                                nameColor);
                        spriteBatch.DrawString(clockFont,
                            String.Format("{0:00}", players[i].lastLapTime.Minutes)
                            + ":" + String.Format("{0:00}", players[i].lastLapTime.Seconds)
                            + ":" + String.Format("{0:000}", players[i].lastLapTime.Milliseconds),
                            new Vector2(GraphicsDevice.Viewport.Width / 2 + 60,
                                GraphicsDevice.Viewport.Height / 2 + i * 40 - 150),
                            timeColor);
                    }
                    spriteBatch.End();
                    break;
                default:
                    break;
            }
            base.Draw(gameTime);
        }

        //Load the track from the checkpoints
        void loadTrack()
        {
            List<Tracks.TrackNode> trackPositions = new List<Tracks.TrackNode>()
            {
                new Tracks.TrackNode(new Vector3(-11900, 2200,     0),0),
                new Tracks.TrackNode(new Vector3(-8000,  1500,   8000),0),
                new Tracks.TrackNode(new Vector3(    0,   1200,  9200),0),
                new Tracks.TrackNode(new Vector3( 8000,   800,   8000),0),
                new Tracks.TrackNode(new Vector3(10000,   850,   5000),0),
                new Tracks.TrackNode(new Vector3( 8000,   900,   2000),0),
                new Tracks.TrackNode(new Vector3( 2200,  1600,   1900),0),
                new Tracks.TrackNode(new Vector3(    0,  1700,  -1000),0),
                new Tracks.TrackNode(new Vector3( 2000,   1500, -3000),0),
                new Tracks.TrackNode(new Vector3( 8000,    800, -3000),0),
                new Tracks.TrackNode(new Vector3(10000,    650, -5500),0),
                new Tracks.TrackNode(new Vector3( 8000,    600, -8000),0),
                new Tracks.TrackNode(new Vector3(    0,  1350,  -9000),0),
                new Tracks.TrackNode(new Vector3(-8000,  1700,  -8000),0),
                new Tracks.TrackNode(new Vector3(-11900, 2200,   200),0)
            };

            track1 = new Tracks.RaceTrack(trackPositions, 500, 1000, 30, GraphicsDevice, Content, lightEffect);
        }

        //Load a model for each player
        void loadPlayerModels(List<Vector3> checkpoints)
        {
            //Load the player models
            models.Add(new Sprites.Player(Content.Load<Model>(@"Models\LandShark"),
                new Vector3(-11900, 2270, 0), new Vector3(0,MathHelper.ToRadians(180),0),
                new Vector3(0.5f), GraphicsDevice,
                checkpoints, bumpEffect.Clone(), playerTexture,
                playerBumpTexture, 1, soundBank));
            if (numPlayers > 1)
                models.Add(new Sprites.Player(Content.Load<Model>(@"Models\LandShark"),
                new Vector3(-12200, 2270, 0), new Vector3(0, MathHelper.ToRadians(180), 0),
                new Vector3(0.5f), GraphicsDevice,
                checkpoints, bumpEffect.Clone(), playerTexture, 
                playerBumpTexture, 2, soundBank));
            if (numPlayers > 2)
                models.Add(new Sprites.Player(Content.Load<Model>(@"Models\LandShark"),
                new Vector3(-11600, 2270, 0), new Vector3(0, MathHelper.ToRadians(180), 0),
                new Vector3(0.5f), GraphicsDevice,
                checkpoints, bumpEffect.Clone(), playerTexture,
                playerBumpTexture, 3, soundBank));
            if (numPlayers > 3)
                models.Add(new Sprites.Player(Content.Load<Model>(@"Models\LandShark"),
                new Vector3(-11300, 2270, 0), new Vector3(0, MathHelper.ToRadians(180), 0),
                new Vector3(0.5f), GraphicsDevice,
                checkpoints, bumpEffect.Clone(), playerTexture,
                playerBumpTexture, 4, soundBank));
            foreach (Sprites.CModel model in models)
            {
                vehicles.Add(((Sprites.Vehicle)model));
                players.Add(((Sprites.Player)model));
            }
        }

        //Load a camera and viewport for each player
        void loadCameras()
        {
            //For each player...
            for (int i = 0; i < numPlayers; i++)
            {
                //Create a viewport
                Viewport vp = GraphicsDevice.Viewport;

                //If the number of players is over 2, draw quarter screen viewports
                if (numPlayers > 2)
                {
                    vp.Width = vp.Width / 2 - CAMERA_SPACING;
                    vp.Height = vp.Height / 2 - CAMERA_SPACING;

                    vp.X = 0 + (GraphicsDevice.Viewport.Width / 2 + CAMERA_SPACING) * (i / 2);
                    vp.Y = 0 + (GraphicsDevice.Viewport.Height / 2 + CAMERA_SPACING) * (i % 2);
                }
                //Otherwise, draw either half or full viewports
                else
                {
                    vp.X = 0;
                    vp.Y = 0 + (GraphicsDevice.Viewport.Height / 2 + CAMERA_SPACING) * (i);
                    if (numPlayers != 1)
                        vp.Height = vp.Height / 2 - CAMERA_SPACING;
                }
                //Create a camera to follow the model
                cameras.Add(new Cameras.RaceCamera(new Vector3(0, 300, 750), //Camera position offset from model
                                            new Vector3(0, 150, 0),     //Camera look offset from model
                                            new Vector3(0, 0, 0),       //Camera rotaion offset from model
                                            vp,                         //Viewport
                                            GraphicsDevice,             //Graphic device
                                            models[i]));                //Model to follow

                //Create a driver camera
                cameras.Add(new Cameras.DriverCamera(vp, GraphicsDevice, models[i]));
            }
        }

        //Load the speed panels for the level
        void loadSpeedPanels()
        {
            panels.Add(new Sprites.SpeedPanel(Content.Load<Model>(@"Models\SpeedPanel"),
                new Vector3(8000, 900, 2000), new Vector3(MathHelper.ToRadians(10), MathHelper.ToRadians(90), 0),
                new Vector3(2.0f), GraphicsDevice,unlitEffect.Clone(),
                Content.Load<Texture2D>(@"Textures\boost_texture")));
            panels.Add(new Sprites.SpeedPanel(Content.Load<Model>(@"Models\SpeedPanel"),
                new Vector3(8000, 600, -8000), new Vector3(MathHelper.ToRadians(10), MathHelper.ToRadians(90), 0),
                new Vector3(2.0f), GraphicsDevice, unlitEffect.Clone(),
                Content.Load<Texture2D>(@"Textures\boost_texture")));
            foreach(Sprites.SpeedPanel panel in panels)
                models.Add(panel);
        }

        //Load the ramps for the level
        void loadRamps()
        {
            ramps.Add(new Sprites.Ramp(Content.Load<Model>(@"Models\SpeedPanel"),
                new Vector3(-4000, 1500, 9200), 
                new Vector3(MathHelper.ToRadians(30), MathHelper.ToRadians(270), 0),
                new Vector3(8.0f), GraphicsDevice, unlitEffect.Clone(),
                Content.Load<Texture2D>(@"Textures\boost_texture")));
            foreach (Sprites.Ramp ramp in ramps)
                models.Add(ramp);

        }

        //Check each vehicle to see if it collided with any other vehicle
        void detectCollision()
        {
            for (int i = 0; i < models.Count - 1; i++)
            {
                for (int j = i + 1; j < models.Count; j++)
                {
                    if (models[i].BoundingSphere.Intersects(models[j].BoundingSphere))
                    {
                        if (models[i] is Sprites.Vehicle && models[j] is Sprites.Vehicle)
                            resolveVehicleCollision(((Sprites.Vehicle)models[i]), ((Sprites.Vehicle)models[j]));
                        else if ((models[i] is Sprites.Ramp || models[j] is Sprites.Ramp)
                                && (models[i] is Sprites.Vehicle || models[j] is Sprites.Vehicle))
                            resolveRampCollision(models[i], models[j]);
                        else if ((models[i] is Sprites.SpeedPanel || models[j] is Sprites.SpeedPanel)
                                && (models[i] is Sprites.Vehicle || models[j] is Sprites.Vehicle))
                            resolvePanelCollision(models[i], models[j]);
                    }
                }
            }
        }

        //Calculate new inertia vectors for the two colliding vehicles
        void resolveVehicleCollision(Sprites.Vehicle vehicleA, Sprites.Vehicle vehicleB)
        {
            //If either vehicle is done, don't process the collision
            if (vehicleA.bDone || vehicleB.bDone)
                return;

            //Get information about the vehicles
            float restitution = 0.5f;           //Coefficient of Restitution of the collision
            Vector3 velA = vehicleA.inertiaVect;//Initial velocity of vehicle A
            Vector3 velB = vehicleB.inertiaVect;//Initial velocity of vehicle B
            float massA = vehicleA.mass;        //Mass of vehicle A
            float massB = vehicleB.mass;        //Mass of vehicle B
            Vector3 newVelA = Vector3.Zero;     //New velocity of vehicle A
            Vector3 newVelB = Vector3.Zero;     //New velocity of vehicle B

            //Undo the last frame of movement of the faster vehicle
            if (velA.Length() >= velB.Length())
                vehicleA.Position -= velA;
            else
                vehicleB.Position -= velB;

            //Calculate the new inertia vectors
                //Vehicle A
            newVelA.X = (restitution * massB * (velB.X - velA.X) + massA * velA.X + massB * velB.X) / (massA + massB);
            newVelA.Y = (restitution * massB * (velB.Y - velA.Y) + massA * velA.Y + massB * velB.Y) / (massA + massB);
            newVelA.Z = (restitution * massB * (velB.Z - velA.Z) + massA * velA.Z + massB * velB.Z) / (massA + massB);
                //Vehicle B
            newVelB.X = (restitution * massA * (velA.X - velB.X) + massA * velA.X + massB * velB.X) / (massA + massB);
            newVelB.Y = (restitution * massA * (velA.Y - velB.Y) + massA * velA.Y + massB * velB.Y) / (massA + massB);
            newVelB.Z = (restitution * massA * (velA.Z - velB.Z) + massA * velA.Z + massB * velB.Z) / (massA + massB);

            //Apply the new inertia vectors
            vehicleA.inertiaVect = newVelA;
            vehicleB.inertiaVect = newVelB;

            //Play the hit sound
            soundBank.PlayCue("hitSnd");
        }

        //Add the boost effect of a speed panel to a vehicle
        void resolvePanelCollision(Sprites.CModel modelA, Sprites.CModel modelB)
        {
            Sprites.Vehicle vehicle;
            Sprites.SpeedPanel panel;

            if (modelB is Sprites.SpeedPanel)
            {
                vehicle = ((Sprites.Vehicle)modelA);
                panel = ((Sprites.SpeedPanel)modelB);
            }
            else
            {
                vehicle = ((Sprites.Vehicle)modelB);
                panel = ((Sprites.SpeedPanel)modelA);
            }

            vehicle.inertiaVect += panel.direction * 2;
            soundBank.PlayCue("boostSnd");
        }

        //Alter the vehicle's inertia vector by the ramp's up vector
        void resolveRampCollision(Sprites.CModel modelA, Sprites.CModel modelB)
        {
            Sprites.Vehicle vehicle;
            Sprites.Ramp ramp;

            if (modelB is Sprites.SpeedPanel)
            {
                vehicle = ((Sprites.Vehicle)modelA);
                ramp = ((Sprites.Ramp)modelB);
            }
            else
            {
                vehicle = ((Sprites.Vehicle)modelB);
                ramp = ((Sprites.Ramp)modelA);
            }
            float speed = vehicle.inertiaVect.Length();
            Vector3 sideVect = Vector3.Cross(vehicle.inertiaVect, Vector3.Up);
            sideVect.Normalize();
            vehicle.inertiaVect = Vector3.Normalize(Vector3.Cross(ramp.upVect,sideVect))*speed;
        }

        //Set a new game state
        public void setGameState(GameState newState)
        {
            switch (newState)
            {
                case GameState.PLAY:
                    //Load the skybox model
                    skyBox = new Sprites.SkyBox(Content.Load<Model>(@"Models\sphere"),
                        new Vector3(0.0f, 400.0f, 0.0f), Vector3.Zero, new Vector3(100.0f), GraphicsDevice,
                        unlitEffect.Clone(), skyTexture);
                    loadTrack();//Load the track 
                    loadPlayerModels(track1.checkpoints);//Load the player models         
                    loadSpeedPanels();//Load the speed panels
                    loadRamps();//Load the ramps
                    //Load the terrain
                    terrain = new Terrain(Content.Load<Texture2D>(@"HeightMaps\Map1"), 150, 2700,
                        Content.Load<Texture2D>(@"Textures\terrain_texture"), 20, new Vector3(1, -1, 0),
                        GraphicsDevice, Content, unlitEffect.Clone());
                    //Load the cameras
                    loadCameras();
                    //Reset the timer
                    TimeSpan raceTimer = new TimeSpan(0, 0, 0);
                    break;
                case GameState.MENU:
                    models.Clear();
                    players.Clear();
                    vehicles.Clear();
                    panels.Clear();
                    cameras.Clear();
                    mainMenu = new Menus.GameMenu(Content);
                    break;
                case GameState.OVER:
                    bool bSorted = false;
                    while (!bSorted)
                    {
                        bSorted = true;
                        for (int i = 0; i < players.Count() - 1; i++)
                        {
                            if (players[i].lastLapTime.TotalMilliseconds
                                > players[i + 1].lastLapTime.TotalMilliseconds)
                            {
                                Sprites.Player temp = players[i];
                                players[i] = players[i + 1];
                                players[i + 1] = temp;
                                bSorted = false;
                            }
                        }
                    }
                    break;
                case GameState.QUIT:
                    this.Exit();
                    break;
                default:
                    break;
            }
            currentGameState = newState;
        }

        bool isDone(Sprites.Vehicle vehicle) { return vehicle.bDone; }
    }
}
