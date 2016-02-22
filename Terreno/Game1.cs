using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terreno.Particulas;

namespace Terreno
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        BasicEffect efeito3DAxis, efeitoTerrain, efeitoWater, efeitoDeepWater, efeitoBasico;
        Texture2D heightmap, terrainTexture, waterTexture;
        float anguloLuz;
        float stepAnguloLuz;
        Random random;
        SpriteFont spriteFont;
        SpriteBatch spriteBatch;
        List<Tank> listaTanques;
        Tank tankPlayer1;
        KeyboardState kbAnterior;
        SistemaParticulasChuva particulasChuva;
        int raioNuvem;
        List<Palmeira> listaPalmeiras;

        bool desenharTanques, desenharTerreno;
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = 1366; //683;
            graphics.PreferredBackBufferHeight = 768; //384;
            graphics.IsFullScreen = true;
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

            BalaManager.Initialize(Content);

            listaTanques = new List<Tank>();
            listaPalmeiras = new List<Palmeira>();


            this.Window.Title = "Tank Simulator";
            desenharTanques = true;
            desenharTerreno = true;

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
            terrainTexture = Content.Load<Texture2D>("sand");
            waterTexture = Content.Load<Texture2D>("water_texture");
            spriteFont = Content.Load<SpriteFont>("arial_12");

           

            //Generate terrain
            Terrain.GenerateTerrain(GraphicsDevice, heightmap);

            raioNuvem = Terrain.altura / 2;

            particulasChuva = new SistemaParticulasChuva(random, raioNuvem / 2, 100, 50);
            SistemaParticulasExplosao.Initialize(random);

            //Gerar água
            Water.GenerateWater(GraphicsDevice, heightmap.Width);

            //Gerar palmeiras
            Model modeloPalmeira = Content.Load<Model>("MY_PALM");
            for (int i = 0; i < 30; i++)
            {
                int x, z;
                x = random.Next(10, Terrain.altura - 10);
                z = random.Next(10, Terrain.altura - 10);
                listaPalmeiras.Add(new Palmeira(modeloPalmeira, new Vector3(x, Terrain.getAlturaFromHeightmap(new Vector3(x, 0, z)), z)));
            }
                

            Equipa equipa;
            for (int i = 0; i < 32; i++)
            {
                equipa = ContarTanquesEquipa(Equipa.Empire) <= ContarTanquesEquipa(Equipa.Rebels) ? Equipa.Empire : Equipa.Rebels;
                Tank tank;
                if (equipa == Equipa.Empire)
                {
                    tank = new Tank(random, GraphicsDevice, new Vector3(random.Next(Terrain.altura - 40, Terrain.altura - 10), 5, random.Next(10, 40)), equipa);
                }
                else
                {
                    tank = new Tank(random, GraphicsDevice, new Vector3(random.Next(10, 40), 5, random.Next(Terrain.altura - 40, Terrain.altura - 10)), equipa);
                }
                
                tank.LoadContent(Content);
                listaTanques.Add(tank);
            }

            equipa = ContarTanquesEquipa(Equipa.Empire) <= ContarTanquesEquipa(Equipa.Rebels) ? Equipa.Empire : Equipa.Rebels;
            if (equipa == Equipa.Empire)
            {
                tankPlayer1 = new Tank(random, GraphicsDevice, new Vector3(random.Next(Terrain.altura - 50, Terrain.altura - 10), 5, random.Next(10, 50)), equipa);
            }
            else
            {
                tankPlayer1 = new Tank(random, GraphicsDevice, new Vector3(random.Next(10, 50), 5, random.Next(Terrain.altura - 50, Terrain.altura - 10)), equipa);
            }
            tankPlayer1.LoadContent(Content);
            tankPlayer1.ativarTanque(listaTanques);
            listaTanques.Add(tankPlayer1);

            DefinirSargentoInimigo(tankPlayer1, random);

            //Inicializar a camara
            Camera.Initialize(GraphicsDevice);

            //Load shaders
            efeito3DAxis = new BasicEffect(GraphicsDevice);
            efeito3DAxis.VertexColorEnabled = true;

            efeitoBasico = new BasicEffect(GraphicsDevice);
            efeitoBasico.VertexColorEnabled = true;

            efeitoTerrain = new BasicEffect(GraphicsDevice);
            efeitoTerrain.Texture = terrainTexture;
            efeitoTerrain.TextureEnabled = true;
            efeitoTerrain.PreferPerPixelLighting = true;
            efeitoTerrain.LightingEnabled = true; // ativar a iluminação
            efeitoTerrain.DirectionalLight0.DiffuseColor = new Vector3(0.58f, 0.58f, 0.58f);
            efeitoTerrain.DirectionalLight0.Direction = new Vector3(0, 1, 0);
            efeitoTerrain.DirectionalLight0.SpecularColor = new Vector3(0, 0, 0);
            efeitoTerrain.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
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

        private int ContarTanquesEquipa(Equipa equipa)
        {
            int contador = 0;
            foreach (Tank tank in listaTanques)
            {
                if (tank.equipa == equipa)
                {
                    contador++;
                }
            }
            return contador;
        }


        private void DefinirSargentoInimigo(Tank player, Random random)
        {
            List<Tank> sargentosPotenciais = new List<Tank>(300);
            foreach (Tank tank in listaTanques)
            {
                if (tank.equipa != player.equipa)
                {
                    sargentosPotenciais.Add(tank);
                }
            }
            sargentosPotenciais[random.Next(0, sargentosPotenciais.Count)].sargento = true;
            sargentosPotenciais.Clear();
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
                tankPlayer1.ativarTanque(listaTanques);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Y) && !kbAnterior.IsKeyDown(Keys.Y))
            {
                desenharTanques = !desenharTanques;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.T) && !kbAnterior.IsKeyDown(Keys.T))
            {
                desenharTerreno = !desenharTerreno;
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
                tank.Update(gameTime, listaTanques, listaTanques.Find(x => x.isAtivo()), Content, random, listaPalmeiras);
            }

            Camera.Update(gameTime, GraphicsDevice, listaTanques.Find(x => x.isAtivo()));

            particulasChuva.Update(random, gameTime);
            SistemaParticulasExplosao.Update(random, gameTime);

            //Colisões entre balas e tanques
            CollisionDetector.CollisionBalaTank(listaTanques, BalaManager.getListaBalas(), random);
            //Colisões entre balas e terreno
            CollisionDetector.CollisionBalaTerrain(BalaManager.getListaBalas(), random);
            //Remover coisas mortas
            removeDeadStuff(gameTime);

            base.Update(gameTime);
        }

        public void removeDeadStuff(GameTime gameTime)
        {
            //Remover balas que sairam dos limites do ecrã
            foreach (Bala bala in BalaManager.getListaBalas())
            {
                bala.Update(gameTime);
                if (bala.position.X < -10 || bala.position.X > Terrain.altura + 10
                    || bala.position.Z < -10 || bala.position.Z > Terrain.altura + 10
                    || bala.position.Y < 0
                    || !bala.alive)
                {
                    BalaManager.KillBala(bala);
                }
            }

            BalaManager.RemoveDeadBalas();
            
            //Remover tanques que colidiram e foram marcadas como mortas
            listaTanques.RemoveAll(x => !x.alive);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0, 0, 15));

            // TODO: Add your drawing code here

            if(desenharTerreno)
                Terrain.Draw(GraphicsDevice, efeitoTerrain);

            GraphicsDevice.BlendState = BlendState.Opaque;

            if (desenharTanques)
            {
                //Desenhar os tanques visiveis do ponto de vista da camara
                foreach (Tank tank in listaTanques)
                {
                    if (Camera.frustum.Contains(tank.boundingSphere) != ContainmentType.Disjoint)
                    {
                        tank.Draw(GraphicsDevice, efeitoTerrain);
                    }

                }
            }
            

            //Desenhar as balas visiveis do ponto de vista da camara
            foreach (Bala bala in BalaManager.getListaBalas())
            {
                if (Camera.frustum.Contains(bala.BoundingSphere) != ContainmentType.Disjoint)
                {
                    bala.Draw();
                }
                
            }

            if (desenharTerreno) particulasChuva.Draw(GraphicsDevice, efeitoBasico);
            SistemaParticulasExplosao.Draw(GraphicsDevice, efeitoBasico);
            foreach (Tank tank in listaTanques)
            {
                tank.sistemaParticulasPo.Draw(GraphicsDevice, efeitoBasico, tank);
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (Palmeira palmeira in listaPalmeiras)
            {
                palmeira.Draw(efeitoTerrain);
            }

            if (desenharTerreno)
                Water.Draw(GraphicsDevice, efeitoWater, efeitoDeepWater);
            
            DebugShapeRenderer.Draw(gameTime, Camera.View, Camera.Projection);
            
            ////////DEBUG
            ////////Escrever a posição da camara no ecra
            //spriteBatch.Begin();
            //spriteBatch.DrawString(spriteFont,
            //    "Particulas: " + SistemaParticulasExplosao.getNParticulasVivas().ToString() + "; " + SistemaParticulasExplosao.getNParticulasMortas().ToString(), Vector2.Zero, Color.Red);
            //spriteBatch.DrawString(spriteFont,
            //    "Balas: " + BalaManager.getNBalasVivas().ToString() + "; " + BalaManager.getNBalasMortas().ToString(), new Vector2(0, 10), Color.Red);
            //spriteBatch.End();

            ////Repor os estados alterados pelo spritebatch
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.RasterizerState = Camera.currentRasterizerState;

            base.Draw(gameTime);
        }

    }
}
