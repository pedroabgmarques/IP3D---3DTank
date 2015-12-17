using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terreno.Particulas;

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
        public bool sargento; //Indica se este tanque é sargento da sua equipa
        private bool tanqueAtivo; //Indica se este tanque é controlado pelo utilizador ou IA
        private List<Tank> vizinhanca; //Tanques à volta deste tanque, dentro de um determinado raio
        private KeyboardState kbAnterior;
        public Matrix inclinationMatrix; //Matriz que descreve a inclinação do tanque, causada pelos declives do terreno

        public Equipa equipa;
        Tank alvo;
        Vector3 centroMassa;
        int contadorVerificarCentroMassa;
        int contadorVerificarCentroMassaMax;

        List<Tank> listaAlvosPotenciais;
        bool readyToFire = false;

        public SistemaParticulasPo sistemaParticulasPo;

        public void ativarTanque(List<Tank> listaTanques)
        {
            foreach (Tank tank in listaTanques)
            {
                if (tank.equipa == this.equipa)
                {
                    tank.sargento = false;
                    tank.tanqueAtivo = false;
                }
            }
            this.sargento = true;
            this.tanqueAtivo = true;
            this.alive = true;
            this.CannonRotation = 0f;
            this.TurretRotation = 0f;
        }

        public void desativarTanque()
        {
            this.tanqueAtivo = false;
            this.sargento = false;
        }

        public bool isAtivo()
        {
            return this.tanqueAtivo;
        }

        public bool moving;

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

        public Tank(Random random, GraphicsDevice graphicsDevice, Vector3 position, Equipa equipa)
        {
            moving = false;
            alive = true;
            vizinhanca = new List<Tank>(300);
            listaAlvosPotenciais = new List<Tank>(300);
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
            boundingSphere = new BoundingSphere(position, 0.5f);

            device = graphicsDevice;
            world = rotacao
                * Matrix.CreateScale(scale)
                * Matrix.CreateTranslation(position);
            tanqueAtivo = false;

            this.equipa = equipa;
            this.sargento = false;
            contadorVerificarCentroMassa = 0;
            contadorVerificarCentroMassaMax = random.Next(1000, 4000);
            centroMassa = Vector3.Zero;

            sistemaParticulasPo = new SistemaParticulasPo(random, 10, this);
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

        
        public void Update(GameTime gameTime, List<Tank> listaTanques, Tank player, ContentManager content, Random random)
        {
            sistemaParticulasPo.Update(random, gameTime, this);
            if (tanqueAtivo)
            {
                //Tanque controlado pelo utilizador
                UpdateInput(gameTime, content, random);
                
            }
            else
            {

                //Tanque controlado por IA

                //Surface follow
                position.Y = Terrain.getAlturaFromHeightmap(position);

                //IA
                float maxDistanciaVizinhanca = 20;

                vizinhanca.Clear();
                foreach (Tank tank in listaTanques)
                {
                    if (tank != this && Vector3.Distance(tank.position, this.position) < maxDistanciaVizinhanca)
                    {
                        vizinhanca.Add(tank);
                    }
                }

                if (contadorVerificarCentroMassa > contadorVerificarCentroMassaMax
                    || centroMassa == Vector3.Zero)
                {
                    centroMassa = AI.centroMassaBoids(listaTanques, this);
                    contadorVerificarCentroMassa = 0;
                }
                Vector3 manterDistancia = AI.manterDistancia(vizinhanca, this, 3f);
                Vector3 combinarDirecao = AI.combinarDirecao(vizinhanca, this);

                Tank sargento = encontrarSargentoEquipa(listaTanques);

                if (alvo != null && (!alvo.alive || !listaTanques.Contains(alvo)))
                {
                    alvo = null;
                }

                if (alvo == null && contarAlvosPotenciais(listaTanques) > 0)
                {
                    alvo = encontrarAlvo(player, listaTanques, random);
                }
                
                if (alvo != null)
                {
                    Vector3 moverParaDirecao;
                    if (this.sargento)
                    {
                        moverParaDirecao = AI.moverParaDirecao(this, alvo.position, alvo.sargento);
                    }
                    else
                    {
                        moverParaDirecao = AI.moverParaDirecao(this, sargento.position, false);
                    }

                    direcao += manterDistancia + moverParaDirecao + combinarDirecao + centroMassa;

                    //Impede o tanque de virar instantâneamente
                    direcao = Vector3.Lerp(direcaoAnterior, direcao, 0.01f);

                    if ((direcao - direcaoAnterior).Length() > 0.0065f)
                    {
                        moving = true;
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

                        //Estamos parados,  disparar
                        direcao = direcaoAnterior;

                        if (readyToFire && lastCannonFire > 300)
                        {
                            //Canhão em posição, arma recarregada, FIRE!

                            //Desvio aleatório para tornar a coisa mais interessante
                            float desvioAleatorio = random.Next(-1200, 1200) / 10000f;

                            BalaManager.ShootBala(this, desvioAleatorio);

                            lastCannonFire = 0;
                        }
                    }
                }

                lastCannonFire++;

                rotateCannonToTarget();

                Vector3 Up = Terrain.getNormalFromHeightmap(position);
                Vector3 Right = Vector3.Cross(Up, direcao);
                Vector3 Frente = Vector3.Cross(Up, Right);

                inclinationMatrix = Matrix.CreateWorld(position, Frente, Up);

                this.world = inclinationMatrix
                        * Matrix.CreateScale(scale)
                        * Matrix.CreateTranslation(position);

                direcaoAnterior = direcao;
            }

            //Atualizar posição do collider do tanque
            boundingSphere.Center = Vector3.Transform(Vector3.Zero, this.world);

            //Verificar colisões com outros tanques
            CollisionDetector.CollisionTankTank(this, listaTanques);

            //Manter tanque dentro dos limites do terreno
            CollisionDetector.CollisionTankFrontiers(this);

            positionAnterior = position;
            contadorVerificarCentroMassa++;
        }

        private void rotateCannonToTarget()
        {
            //Rodar o canhão para posição
            CannonRotation = 0f;

            if (alvo != null)
            {
                Quaternion q = this.inclinationMatrix.Rotation;

                Matrix rotationMatrix = Matrix.CreateScale(scale)
                * Matrix.CreateRotationX(CannonRotation) //rotação vertical do canhão
                * Matrix.CreateRotationY(TurretRotation) //rotação horizontal do canhão
                * Matrix.CreateFromQuaternion(q) //rotação do corpo do tanque - world excepto scale e translation
                ;

                Matrix matrizInclinacao = Matrix.CreateFromQuaternion(q);

                Vector3 direcaoCanhao =
                    Vector3.Transform(vetorBase, rotationMatrix);

                Vector3 direcaoParaAlvo = alvo.position - position;
                direcaoParaAlvo = Vector3.Reflect(direcaoParaAlvo, Terrain.getNormalFromHeightmap(position)) + direcaoParaAlvo;

                //DEBUG
                //Desenhar os vetores relevantes do tanque
                //DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(direcaoParaAlvo), Color.Blue);
                //DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(direcaoCanhao), Color.Red);


                //DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Forward), Color.Green);
                //DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Up), Color.Green);
                //DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Right), Color.Green);
                //DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Backward), Color.Green);
                //DebugShapeRenderer.AddLine(position, position + Vector3.Normalize(matrizInclinacao.Left), Color.Green);

                float anguloHorizontal = (float)Math.Acos(Vector3.Dot(Vector3.Normalize(direcaoParaAlvo), Vector3.Normalize(direcaoCanhao)));
                readyToFire = false;
                this.turretRotationTarget = anguloHorizontal;

                if (turretRotationTarget > 0.007f)
                {
                    TurretRotation -= 0.00625f;
                }
                else
                {
                    readyToFire = true;
                }

                CannonRotation = -MathHelper.ToRadians(Vector3.Distance(position, alvo.position) / 2);
            }
        }

        private Tank encontrarSargentoEquipa(List<Tank> listaTanques)
        {
            Tank sargento = null;
            foreach (Tank tank in listaTanques)
            {
                if (tank.equipa == this.equipa && tank.sargento)
                {
                    sargento = tank;
                    break;
                }
            }
            if (sargento != null)
            {
                return sargento;
            }
            else
            {
                //A equipa não tem sargento, acabámos de ser promovidos!
                this.sargento = true;
                return this;
            }
        }

        private int contarAlvosPotenciais(List<Tank> listaTanques)
        {
            listaAlvosPotenciais.Clear();
            foreach (Tank tank in listaTanques)
            {
                if (tank.equipa != this.equipa)
                {
                    listaAlvosPotenciais.Add(tank);
                }
            }
            return listaAlvosPotenciais.Count;
        }

        private Tank encontrarAlvo(Tank player, List<Tank> listaTanques, Random random)
        {
            listaAlvosPotenciais.Clear();
            foreach (Tank tank in listaTanques)
            {
                if (tank.equipa != this.equipa)
                {
                    listaAlvosPotenciais.Add(tank);
                }
            }
            if (listaAlvosPotenciais.Count > 0)
            {
                if (this.sargento)
                {
                    return player;
                }
                else
                {
                    return listaAlvosPotenciais[random.Next(0, listaAlvosPotenciais.Count)];
                }
                
            }
            else
            {
                return null;
            }
            
        }

        private void UpdateInput(GameTime gameTime, ContentManager content, Random random)
        {

            moving = false;

            KeyboardState currentKeyboardState = Keyboard.GetState();

            steerRotationValue = 0;

            //  Move torre (só até 90 graus)
            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                if (this.TurretRotation < 1.6f)
                    this.TurretRotation += 0.00425f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                if (this.TurretRotation > -1.6f)
                    this.TurretRotation -= 0.00425f;
            }

            //  Move canhão (sem atirar 90 graus nem no próprio tanque)
            if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                if (this.CannonRotation > -0.8f)
                    this.CannonRotation -= 0.00425f;

            }
            if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                if (this.CannonRotation < 0.2f)
                    this.CannonRotation += 0.00425f;
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
                moving = true;
            }

            if (currentKeyboardState.IsKeyDown(Keys.NumPad2))
            {
                //Andar para trás
                this.wheelBackLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 5;
                this.wheelBackRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 5;
                this.wheelFrontLeftRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 5;
                this.wheelFrontRightRotationValue = -(float)gameTime.TotalGameTime.TotalSeconds * 5;

                position -= Vector3.Normalize(direcao) * velocidade;
                moving = true;
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
                BalaManager.ShootBala(this, 0f);
            }

            //Surface follow
            position.Y = Terrain.getAlturaFromHeightmap(position);

            rotacao = Matrix.CreateRotationY(MathHelper.ToRadians(180)) * Matrix.CreateRotationY(MathHelper.ToRadians(rotacaoY));
            direcao = Vector3.Transform(vetorBase, rotacao);

            Vector3 Up = Terrain.getNormalFromHeightmap(position);
            Vector3 Right = Vector3.Cross(Up, direcao);
            Vector3 Frente = Vector3.Cross(Up, Right);

            inclinationMatrix = Matrix.CreateWorld(position, Frente, Up);

            this.world = inclinationMatrix
                * Matrix.CreateScale(scale)
                * Matrix.CreateTranslation(position);

            kbAnterior = currentKeyboardState;
        }

        

        /// <summary>
        /// Draws the tank model, using the current animation settings.
        /// </summary>
        public void Draw(GraphicsDevice graphics, BasicEffect efeito)
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
                    if (this.equipa == Equipa.Rebels)
                    {
                        effect.DirectionalLight0.DiffuseColor = Color.Green.ToVector3();
                    }
                    else
                    {
                        effect.DirectionalLight0.DiffuseColor = Color.Red.ToVector3();
                    }
                    effect.DirectionalLight0.Enabled = true;
                    

                }
                mesh.Draw();
            }

            //DEBUG
            //Desenhar collider do tanque
            //DebugShapeRenderer.AddBoundingSphere(boundingSphere, this.equipa == Equipa.Empire ? Color.Red : Color.Green);
            if (this.sargento)
            {
                if (this.equipa == Equipa.Rebels)
                {
                    DebugShapeRenderer.AddBoundingSphere(boundingSphere, Color.Green);
                }
                else
                {
                    DebugShapeRenderer.AddBoundingSphere(boundingSphere, Color.Red);
                }

            }
        }
    }
}
