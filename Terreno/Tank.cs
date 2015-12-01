using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{
    public class Tank
    {
        #region Fields

        Model tankModel;
        public Matrix world;
        private GraphicsDevice device;
        public bool alive;

        Vector3 vetorBase;
        public Vector3 direcao, direcaoAnterior, target, position, positionAnterior;
        public Matrix rotacao;
        public float rotacaoY;
        public float scale;
        float velocidade;
        private bool tanqueAtivo; //Indica se este tanque é controlado pelo utilizador ou IA
        private List<Tank> vizinhanca; //Tanques à volta deste tanque, dentro de um determinado raio
        private KeyboardState kbAnterior;
        public Matrix inclinationMatrix; //Matriz que descreve a inclinação do tanque, causada pelos declives do terreno

        public void ativarTanque()
        {
            this.tanqueAtivo = true;
        }

        public void desativarTanque()
        {
            this.tanqueAtivo = false;
        }

        public bool isAtivo()
        {
            return this.tanqueAtivo;
        }

        public BoundingSphere boundingSphere;


        // Shortcut references to the bones that we are going to animate.
        // We could just look these up inside the Draw method, but it is more
        // efficient to do the lookups while loading and cache the results.
        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;
        ModelBone leftSteerBone;
        ModelBone rightSteerBone;
        ModelBone turretBone;
        ModelBone cannonBone;
        ModelBone hatchBone;


        // Store the original transform matrix for each animating bone.
        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;
        Matrix leftSteerTransform;
        Matrix rightSteerTransform;
        Matrix turretTransform;
        Matrix cannonTransform;
        Matrix hatchTransform;


        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.
        Matrix[] boneTransforms;


        // Current animation positions.
        float wheelBackLeftRotationValue, wheelBackRightRotationValue, wheelFrontLeftRotationValue, wheelFrontRightRotationValue;
        float steerRotationValue;
        float turretRotationValue;
        public float cannonRotationValue;
        float hatchRotationValue;

        float turretRotationTarget;
        int lastCannonFire = 0;

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation
        {
            get { return steerRotationValue; }
            set { steerRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the turret rotation amount.
        /// </summary>
        public float TurretRotation
        {
            get { return turretRotationValue; }
            set { turretRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the cannon rotation amount.
        /// </summary>
        public float CannonRotation
        {
            get { return cannonRotationValue; }
            set { cannonRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the entry hatch rotation amount.
        /// </summary>
        public float HatchRotation
        {
            get { return hatchRotationValue; }
            set { hatchRotationValue = value; }
        }


        #endregion

        public Tank(GraphicsDevice graphicsDevice, Vector3 position)
        {
            alive = true;
            vizinhanca = new List<Tank>();
            vetorBase = new Vector3(0, 0, 1);
            direcao = vetorBase;
            direcaoAnterior = vetorBase;
            positionAnterior = position;
            this.position = position;
            target = position + direcao;
            rotacaoY = 0;
            rotacao = Matrix.CreateRotationY(rotacaoY);
            velocidade = 0.025f;
            scale = 0.00125f;
            boundingSphere = new BoundingSphere(position, 0.7f);

            device = graphicsDevice;
            world = rotacao
                * Matrix.CreateScale(scale)
                * Matrix.CreateTranslation(position);
            tanqueAtivo = false;

        }

        private double NextDouble(Random rng, double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        /// <summary>
        /// Loads the tank model.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            // Load the tank model from the ContentManager.
            tankModel = content.Load<Model>("tank");

            // Look up shortcut references to the bones we are going to animate.
            leftBackWheelBone = tankModel.Bones["l_back_wheel_geo"];
            rightBackWheelBone = tankModel.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = tankModel.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = tankModel.Bones["r_front_wheel_geo"];
            leftSteerBone = tankModel.Bones["l_steer_geo"];
            rightSteerBone = tankModel.Bones["r_steer_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["canon_geo"];
            hatchBone = tankModel.Bones["hatch_geo"];

            // Store the original transform matrix for each animating bone.
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
            leftSteerTransform = leftSteerBone.Transform;
            rightSteerTransform = rightSteerBone.Transform;
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;
            hatchTransform = hatchBone.Transform;

            // Allocate the transform matrix array.
            boneTransforms = new Matrix[tankModel.Bones.Count];

        }

        public void Update(GameTime gameTime, List<Tank> listaTanques, Tank player, ref List<Bala> listaBalas, ContentManager content, Random random)
        {

            if (tanqueAtivo)
            {
                //Tanque controlado pelo utilizador
                UpdateInput(gameTime, content, ref listaBalas);
            }
            else
            {

                //Tanque controlado por IA

                //Surface follow
                position.Y = getAlturaFromHeightmap();

                //IA
                float maxDistanciaVizinhanca = 20;
                vizinhanca = listaTanques.FindAll(x => Vector3.Distance(x.position, this.position) < maxDistanciaVizinhanca);

                Vector3 centroMassa = AI.centroMassaBoids(listaTanques, this);
                Vector3 manterDistancia = AI.manterDistancia(vizinhanca, this, 3f);
                Vector3 combinarDirecao = AI.combinarDirecao(vizinhanca, this);
                Vector3 moverParaDirecao = AI.moverParaDirecao(this, player.position);

                direcao += manterDistancia + moverParaDirecao + combinarDirecao + centroMassa;

                //Impede o tanque de virar instantâneamente
                direcao = Vector3.Lerp(direcaoAnterior, direcao, 0.01f);

                if ((direcao - direcaoAnterior).Length() > 0.0075f)
                {
                    //Se a "vontade" de mover é suficientemente forte..
                    direcao.Normalize();
                    position += direcao * velocidade;

                    //Rodar as rodas porque estamos a andar
                    this.wheelBackLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 5;
                    this.wheelBackRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 5;
                    this.wheelFrontLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 5;
                    this.wheelFrontRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 5;
                }
                else
                {

                    //Estamos parados, apontar canhão e disparar
                    direcao = direcaoAnterior;
                    //Rodar o canhão para posição
                    CannonRotation = 0f;
                    Vector3 alvo = player.position;

                    Quaternion q = this.inclinationMatrix.Rotation;

                    Matrix rotationMatrix = Matrix.CreateScale(scale) 
                    * Matrix.CreateRotationX(CannonRotation) //rotação vertical do canhão
                    * Matrix.CreateRotationY(TurretRotation) //rotação horizontal do canhão
                    * Matrix.CreateFromQuaternion(q) //rotação do corpo do tanque - world excepto scale e translation
                    ;

                    Vector3 direcaoCanhao =
                        Vector3.Transform(vetorBase, rotationMatrix);

                    Vector3 direcaoParaAlvo = alvo - position;
                    direcaoParaAlvo.Y = 0;

                    Matrix teste = Matrix.CreateFromQuaternion(q);

                    //Isto ainda não funciona...
                    Vector3 direcaoParaAlvoRodado = Vector3.Transform(direcaoParaAlvo,
                        teste
                    );
                    
                    //DEBUG
                    //Desenhar os vetores relevantes do tanque
                    DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(direcaoParaAlvoRodado), Color.Blue);
                    DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(direcaoCanhao), Color.Red);

                    Matrix matrizInclinacao = Matrix.CreateFromQuaternion(q);
                    DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Forward), Color.Green);
                    DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Up), Color.Green);
                    DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Right), Color.Green);
                    DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Backward), Color.Green);
                    DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Left), Color.Green);
                    
                    float anguloHorizontal = (float)Math.Acos(Vector3.Dot(Vector3.Normalize(direcaoParaAlvoRodado), Vector3.Normalize(direcaoCanhao)));
                    bool readyToFire = false;
                    this.turretRotationTarget = anguloHorizontal;

                    if (turretRotationTarget > 0.007f)
                    {
                        TurretRotation -= 0.00625f;
                    }
                    else
                    {
                        readyToFire = true;
                    }

                    if (readyToFire && lastCannonFire > 300)
                    {
                        //Canhão em posição, arma recarregada, FIRE!
                        Bala bala = new Bala(content, this);
                        listaBalas.Add(bala);
                        lastCannonFire = 0;
                    }
                    lastCannonFire++;

                }

                Vector3 Up = getNormalFromHeightmap();
                Vector3 Right = Vector3.Cross(Up, direcao);
                Vector3 Frente = Vector3.Cross(Up, Right);

                inclinationMatrix = Matrix.CreateWorld(position, Frente, Up);

                this.world = inclinationMatrix
                        * Matrix.CreateScale(scale)
                        * Matrix.CreateTranslation(position);

                direcaoAnterior = direcao;
            }

            //Atualizar posição do collider do tanque
            boundingSphere.Center = position;

            //Verificar colisões com outros tanques
            CollisionDetector.CollisionTankTank(this, listaTanques);

            //Manter tanque dentro dos limites do terreno
            CollisionDetector.CollisionTankFrontiers(this);

            positionAnterior = position;
        }

        private void UpdateInput(GameTime gameTime, ContentManager content, ref List<Bala> listaBalas)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            steerRotationValue = 0;

            //  Move torre (só até 90 graus)
            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                if (this.TurretRotation < 1.6f)
                    this.TurretRotation += 0.00625f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                if (this.TurretRotation > -1.6f)
                    this.TurretRotation -= 0.00625f;
            }

            //  Move canhão (sem atirar 90 graus nem no próprio tanque)
            if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                if (this.CannonRotation > -0.8f)
                    this.CannonRotation -= 0.00625f;

            }
            if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                if (this.CannonRotation < 0.2f)
                    this.CannonRotation += 0.00625f;
            }

            //  Abre e fecha porta
            if (currentKeyboardState.IsKeyDown(Keys.PageUp))
            {
                this.HatchRotation = -1;
            }
            if (currentKeyboardState.IsKeyDown(Keys.PageDown))
            {
                this.HatchRotation = 0;
            }

            if (currentKeyboardState.IsKeyDown(Keys.NumPad8))
            {
                //Andar para a frente
                this.wheelBackLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 5;
                this.wheelBackRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 5;
                this.wheelFrontLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 5;
                this.wheelFrontRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 5;

                position += Vector3.Normalize(direcao) * velocidade;
            }

            if (currentKeyboardState.IsKeyDown(Keys.NumPad2))
            {
                //Andar para trás
                this.wheelBackLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 5;
                this.wheelBackRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 5;
                this.wheelFrontLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 5;
                this.wheelFrontRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 5;

                position -= Vector3.Normalize(direcao) * velocidade;
            }

            if (currentKeyboardState.IsKeyDown(Keys.NumPad6))
            {
                //Virar para a direita
                if (!currentKeyboardState.IsKeyDown(Keys.NumPad8) && !currentKeyboardState.IsKeyDown(Keys.NumPad2))
                {
                    this.wheelBackLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 2;
                    this.wheelBackRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 1;
                    this.wheelFrontLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 2;
                    this.wheelFrontRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 1;
                }
                else
                {
                    if (currentKeyboardState.IsKeyDown(Keys.NumPad8))
                    {
                        this.wheelBackLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 4;
                        this.wheelBackRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 2;
                        this.wheelFrontLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 4;
                        this.wheelFrontRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 2;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.NumPad2))
                    {
                        this.wheelBackLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 1;
                        this.wheelBackRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 4;
                        this.wheelFrontLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 1;
                        this.wheelFrontRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 4;
                    }

                }
                rotacaoY -= 0.6f;
                steerRotationValue = -0.2f;

            }

            if (currentKeyboardState.IsKeyDown(Keys.NumPad4))
            {
                //Virar para a esquerda
                if (!currentKeyboardState.IsKeyDown(Keys.NumPad8) && !currentKeyboardState.IsKeyDown(Keys.NumPad2))
                {
                    this.wheelBackLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 2;
                    this.wheelBackRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 2;
                    this.wheelFrontLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 2;
                    this.wheelFrontRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 2;
                }
                else
                {
                    if (currentKeyboardState.IsKeyDown(Keys.NumPad8))
                    {
                        this.wheelBackLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 2;
                        this.wheelBackRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 4;
                        this.wheelFrontLeftRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 2;
                        this.wheelFrontRightRotationValue = (float)gameTime.TotalGameTime.TotalSeconds * 4;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.NumPad2))
                    {
                        this.wheelBackLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 2;
                        this.wheelBackRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 4;
                        this.wheelFrontLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 2;
                        this.wheelFrontRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 4;
                    }
                }
                rotacaoY += 0.6f;
                steerRotationValue = 0.2f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Space) && !kbAnterior.IsKeyDown(Keys.Space))
            {
                Bala bala = new Bala(content, this);
                listaBalas.Add(bala);
            }

            //Surface follow
            position.Y = getAlturaFromHeightmap();

            rotacao = Matrix.CreateRotationY(MathHelper.ToRadians(180)) * Matrix.CreateRotationY(MathHelper.ToRadians(rotacaoY));
            direcao = Vector3.Transform(vetorBase, rotacao);

            Vector3 Up = getNormalFromHeightmap();
            Vector3 Right = Vector3.Cross(Up, direcao);
            Vector3 Frente = Vector3.Cross(Up, Right);

            inclinationMatrix = Matrix.CreateWorld(position, Frente, Up);

            this.world = inclinationMatrix
                * Matrix.CreateScale(scale)
                * Matrix.CreateTranslation(position);

            kbAnterior = currentKeyboardState;
        }

        private float getAlturaFromHeightmap()
        {
            //Posição arredondada para baixo da camara
            int xTank, zTank;
            xTank = (int)position.X;
            zTank = (int)position.Z;

            //Os 4 vértices que rodeiam a posição da camara
            Vector2 pontoA, pontoB, pontoC, pontoD;
            pontoA = new Vector2(xTank, zTank);
            pontoB = new Vector2(xTank + 1, zTank);
            pontoC = new Vector2(xTank, zTank + 1);
            pontoD = new Vector2(xTank + 1, zTank + 1);

            //Recolher a altura de cada um dos 4 vértices à volta do tanque a partir do heightmap
            float Ya, Yb, Yc, Yd;
            Ya = Terrain.vertexes[(int)pontoA.X * Terrain.altura + (int)pontoA.Y].Position.Y;
            Yb = Terrain.vertexes[(int)pontoB.X * Terrain.altura + (int)pontoB.Y].Position.Y;
            Yc = Terrain.vertexes[(int)pontoC.X * Terrain.altura + (int)pontoC.Y].Position.Y;
            Yd = Terrain.vertexes[(int)pontoD.X * Terrain.altura + (int)pontoD.Y].Position.Y;

            //Interpolação bilenear (dada nas aulas)
            float Yab = (1 - (position.X - pontoA.X)) * Ya + (position.X - pontoA.X) * Yb;
            float Ycd = (1 - (position.X - pontoC.X)) * Yc + (position.X - pontoC.X) * Yd;
            float Y = (1 - (position.Z - pontoA.Y)) * Yab + (position.Z - pontoA.Y) * Ycd;

            //Devolver a altura + um offset
            return Y + 0.01f;
        }

        private Vector3 getNormalFromHeightmap()
        {
            //Posição arredondada para baixo da camara
            int xTank, zTank;
            xTank = (int)position.X;
            zTank = (int)position.Z;

            //Os 4 vértices que rodeiam a posição da camara
            Vector2 pontoA, pontoB, pontoC, pontoD;
            pontoA = new Vector2(xTank, zTank);
            pontoB = new Vector2(xTank + 1, zTank);
            pontoC = new Vector2(xTank, zTank + 1);
            pontoD = new Vector2(xTank + 1, zTank + 1);

            
            Vector3 Ya, Yb, Yc, Yd;
            Ya = Terrain.vertexes[(int)pontoA.X * Terrain.altura + (int)pontoA.Y].Normal;
            Yb = Terrain.vertexes[(int)pontoB.X * Terrain.altura + (int)pontoB.Y].Normal;
            Yc = Terrain.vertexes[(int)pontoC.X * Terrain.altura + (int)pontoC.Y].Normal;
            Yd = Terrain.vertexes[(int)pontoD.X * Terrain.altura + (int)pontoD.Y].Normal;

            //Interpolação bilenear (dada nas aulas)
            Vector3 Yab = (1 - (position.X - pontoA.X)) * Ya + (position.X - pontoA.X) * Yb;
            Vector3 Ycd = (1 - (position.X - pontoC.X)) * Yc + (position.X - pontoC.X) * Yd;
            Vector3 Y = (1 - (position.Z - pontoA.Y)) * Yab + (position.Z - pontoA.Y) * Ycd;

            //Devolver a altura + um offset
            return Y;
        }
        


        /// <summary>
        /// Draws the tank model, using the current animation settings.
        /// </summary>
        public void Draw(BasicEffect efeito)
        {
            // Set the world matrix as the root transform of the model.
            tankModel.Root.Transform = world;

            // Calculate matrices based on the current animation position.
            Matrix wheelRotationBackLeft = Matrix.CreateRotationX(wheelBackLeftRotationValue);
            Matrix wheelRotationBackRight = Matrix.CreateRotationX(wheelBackRightRotationValue);
            Matrix wheelRotationFrontLeft = Matrix.CreateRotationX(wheelFrontLeftRotationValue);
            Matrix wheelRotationFrontRight = Matrix.CreateRotationX(wheelFrontRightRotationValue);
            Matrix steerRotation = Matrix.CreateRotationY(steerRotationValue);
            Matrix turretRotation = Matrix.CreateRotationY(turretRotationValue);
            Matrix cannonRotation = Matrix.CreateRotationX(cannonRotationValue);
            Matrix hatchRotation = Matrix.CreateRotationX(hatchRotationValue);

            // Apply matrices to the relevant bones.
            leftBackWheelBone.Transform = wheelRotationBackLeft * leftBackWheelTransform;
            rightBackWheelBone.Transform = wheelRotationBackRight * rightBackWheelTransform;
            leftFrontWheelBone.Transform = wheelRotationFrontLeft * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = wheelRotationFrontRight * rightFrontWheelTransform;
            leftSteerBone.Transform = steerRotation * leftSteerTransform;
            rightSteerBone.Transform = steerRotation * rightSteerTransform;
            turretBone.Transform = turretRotation * turretTransform;
            cannonBone.Transform = cannonRotation * cannonTransform;
            hatchBone.Transform = hatchRotation * hatchTransform;

            // Look up combined bone matrices for the entire model.
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = Camera.View;
                    effect.Projection = Camera.Projection;

                    effect.EnableDefaultLighting();
                    effect.DirectionalLight0.Direction = efeito.DirectionalLight0.Direction;
                    effect.DirectionalLight0.Enabled = true;
                    

                }
                mesh.Draw();
            }

            //DEBUG
            //Desenhar collider do tanque
            DebugShapeRenderer.AddBoundingSphere(boundingSphere, Color.Blue);
        }
    }
}
