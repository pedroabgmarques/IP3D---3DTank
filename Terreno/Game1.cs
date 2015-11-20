using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Terreno
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        BasicEffect efeito3DAxis, efeitoTerrain, efeitoWater, efeitoDeepWater;
        Texture2D heightmap, terrainTexture, waterTexture;
        float anguloLuz;
        float stepAnguloLuz;
        Random random;
        SpriteFont spriteFont;
        SpriteBatch spriteBatch;
        List<Tank> listaTanques;
        Tank tankPlayer1;
        KeyboardState kbAnterior;
        List<Bala> listaBalas;
        List<Bala> listaBalasRemover;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;
            graphics.SynchronizeWithVerticalRetrace = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            random = new Random();

            Create3DAxis.Initialize(GraphicsDevice);

            DebugShapeRenderer.Initialize(GraphicsDevice);

            listaTanques = new List<Tank>();

            listaBalas = new List<Bala>();

            listaBalasRemover = new List<Bala>();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Load assets
            heightmap = Content.Load<Texture2D>("heightmap");
            terrainTexture = Content.Load<Texture2D>("terrainTexture");
            waterTexture = Content.Load<Texture2D>("water_texture");
            spriteFont = Content.Load<SpriteFont>("arial_12");

            //Generate terrain
            Terrain.GenerateTerrain(GraphicsDevice, heightmap);

            //Gerar água
            Water.GenerateWater(GraphicsDevice, heightmap.Width);

            for (int i = 0; i < 50; i++)
            {
                Tank tank = new Tank(GraphicsDevice, new Vector3(random.Next(10, Terrain.altura - 10), 50, random.Next(10, Terrain.altura - 10)));
                tank.LoadContent(Content);
                listaTanques.Add(tank);
            }

            tankPlayer1 = new Tank(graphics.GraphicsDevice, new Vector3(50, 5, 50));
            tankPlayer1.LoadContent(Content);
            tankPlayer1.ativarTanque();
            listaTanques.Add(tankPlayer1);

            //Inicializar a camara
            Camera.Initialize(GraphicsDevice);

            //Load shaders
            efeito3DAxis = new BasicEffect(GraphicsDevice);
            efeito3DAxis.VertexColorEnabled = true;

            efeitoTerrain = new BasicEffect(GraphicsDevice);
            efeitoTerrain.Texture = terrainTexture;
            efeitoTerrain.TextureEnabled = true;
            efeitoTerrain.PreferPerPixelLighting = true;
            efeitoTerrain.LightingEnabled = true; // ativar a iluminação
            efeitoTerrain.DirectionalLight0.DiffuseColor = new Vector3(10, 8, 7);
            efeitoTerrain.DirectionalLight0.Direction = new Vector3(0, 1, 0);
            efeitoTerrain.DirectionalLight0.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);
            efeitoTerrain.AmbientLightColor = new Vector3(2.5f, 2.5f, 2.5f);
            efeitoTerrain.EmissiveColor = new Vector3(0f, 0f, 0f);
            efeitoTerrain.DirectionalLight0.Enabled = true;
            efeitoTerrain.FogColor = new Color(0, 0, 15).ToVector3();
            efeitoTerrain.FogEnabled = true;
            efeitoTerrain.FogStart = Camera.nearPlane;
            efeitoTerrain.FogEnd = Camera.farPlane;

            efeitoDeepWater = new BasicEffect(GraphicsDevice);
            efeitoDeepWater.Texture = waterTexture;
            efeitoDeepWater.TextureEnabled = true;
            efeitoDeepWater.PreferPerPixelLighting = true;
            efeitoDeepWater.LightingEnabled = true; // turn on the lighting subsystem.
            efeitoDeepWater.DirectionalLight0.DiffuseColor = new Vector3(1f, 0.2f, 0.7f);
            efeitoDeepWater.DirectionalLight0.Direction = new Vector3(0, 1, 0);
            efeitoDeepWater.DirectionalLight0.SpecularColor = new Vector3(0, 0.025f, 0);
            efeitoDeepWater.AmbientLightColor = new Vector3(0.05f, 0.05f, 0.05f);
            efeitoDeepWater.EmissiveColor = new Vector3(0, 0, 0);
            efeitoDeepWater.DirectionalLight0.Enabled = true;
            efeitoDeepWater.Alpha = 0.4f;
            efeitoDeepWater.FogColor = new Color(0, 0, 15).ToVector3();
            efeitoDeepWater.FogEnabled = true;
            efeitoDeepWater.FogStart = Camera.nearPlane;
            efeitoDeepWater.FogEnd = Camera.farPlane;

            efeitoWater = new BasicEffect(GraphicsDevice);
            efeitoWater.Texture = waterTexture;
            efeitoWater.TextureEnabled = true;
            efeitoWater.PreferPerPixelLighting = true;
            efeitoWater.LightingEnabled = true; // turn on the lighting subsystem.
            efeitoWater.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0.125f, 0.35f);
            efeitoWater.DirectionalLight0.Direction = new Vector3(0, 1, 0);
            efeitoWater.DirectionalLight0.SpecularColor = new Vector3(0, 0.0125f, 0);
            efeitoWater.AmbientLightColor = new Vector3(0.05f, 0.05f, 0.05f);
            efeitoWater.EmissiveColor = new Vector3(0f, 0f, 0f);
            efeitoWater.DirectionalLight0.Enabled = true;
            efeitoWater.Alpha = 0.6f;
            efeitoWater.FogColor = new Color(0, 0, 15).ToVector3();
            efeitoWater.FogEnabled = true;
            efeitoWater.FogStart = Camera.nearPlane;
            efeitoWater.FogEnd = Camera.farPlane;

            anguloLuz = MathHelper.ToRadians(90);
            stepAnguloLuz = MathHelper.TwoPi / (360 * 5);
            //stepAnguloLuz = 0;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.P) && !kbAnterior.IsKeyDown(Keys.P))
            {
                tankPlayer1.desativarTanque();
                tankPlayer1 = listaTanques[random.Next(0, listaTanques.Count)];
                tankPlayer1.ativarTanque();
            }

            kbAnterior = Keyboard.GetState();

            efeitoTerrain.DirectionalLight0.Direction = new Vector3((float)Math.Cos(anguloLuz), 
                                                        -(float)Math.Sin(anguloLuz), 0);
            efeitoWater.DirectionalLight0.Direction = new Vector3((float)Math.Cos(anguloLuz), 
                                                        -(float)Math.Sin(anguloLuz), 0);
            anguloLuz += stepAnguloLuz;
            if (anguloLuz > MathHelper.ToRadians(245)) anguloLuz = MathHelper.ToRadians(-65);

            foreach (Tank tank in listaTanques)
            {
                tank.Update(gameTime, listaTanques, listaTanques.Find(x => x.isAtivo()), ref listaBalas, Content, random);
            }

            foreach (Bala bala in listaBalas)
            {
                bala.Update(gameTime);
                if (bala.position.X < -10 || bala.position.X > Terrain.altura + 10
                    || bala.position.Z < -10 || bala.position.Z > Terrain.altura + 10
                    || bala.position.Y < -10)
                {
                    listaBalasRemover.Add(bala);
                }
            }

            foreach (Bala bala in listaBalasRemover)
            {
                listaBalas.Remove(bala);
            }
            listaBalasRemover.Clear();

            Camera.Update(gameTime, GraphicsDevice, tankPlayer1);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0, 0, 15));

            // TODO: Add your drawing code here
            Create3DAxis.Draw(GraphicsDevice, efeito3DAxis);

            Terrain.Draw(GraphicsDevice, efeitoTerrain);

            GraphicsDevice.BlendState = BlendState.Opaque;

            DebugShapeRenderer.Draw(gameTime, Camera.View, Camera.Projection);

            foreach (Tank tank in listaTanques)
            {
                tank.Draw(efeitoTerrain);
            }

            foreach (Bala bala in listaBalas)
            {
                bala.Draw();
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Water.Draw(GraphicsDevice, efeitoWater, efeitoDeepWater);

            

            ////DEBUG
            ////Escrever a posição da camara no ecra
            //spriteBatch.Begin();
            //spriteBatch.DrawString(spriteFont,
            //    "Camera: " + Camera.getPosition().X.ToString() + "; " + Camera.getPosition().Y.ToString() + "; " + Camera.getPosition().Z.ToString(), Vector2.Zero, Color.White);
            //spriteBatch.End();

            ////Repor os estados alterados pelo spritebatch
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
