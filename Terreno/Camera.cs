using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{

    enum TipoCamera
    {
        FPS,
        Free,
        thirdPerson
    }

    static class Camera
    {

        //Matrizes World, View e Projection
        static public Matrix World, View, Projection;

        //Posição da camara
        static private Vector3 position;

        //Rotação horizontal
        static float leftrightRot = 0f;
        //Rotação vertical
        static float updownRot = 0f;
        //Velocidade da rotação
        const float rotationSpeed = 0.3f;
        //Velocidade do movimento com o rato
        static float moveSpeed = 5f;
        //Estado do rato
        static private MouseState originalMouseState;
        //BoundingFrustum da camâra
        static public BoundingFrustum frustum;
        //Tamanho do "mundo"
        static public int worldSize = 1024;
        //Near e far plane
        static public float nearPlane = 0.1f;
        static public float farPlane = worldSize;

        static RasterizerState rasterizerStateSolid;
        static RasterizerState rasterizerStateWireFrame;

        static KeyboardState keyStateAnterior;
        static MouseState mouseStateAnterior;

        static public bool drawNormals = false;

        static public TipoCamera tipoCamera;

        /// <summary>
        /// Inicializa os componentes da camara
        /// </summary>
        /// <param name="graphics">Instância de GraphicsDevice</param>
        static public void Initialize(GraphicsDevice graphics)
        {

            tipoCamera = TipoCamera.FPS;

            //Posição inicial da camâra
            position = new Vector3(10, 30, 20);
            //Inicializar as matrizes world, view e projection
            World = Matrix.Identity;
            UpdateViewMatrix();
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                graphics.Viewport.AspectRatio,
                nearPlane,
                farPlane);

            //Criar e definir o resterizerState a utilizar para desenhar a geometria
            rasterizerStateSolid = new RasterizerState();
            //Desenha todas as faces, independentemente da orientação
            rasterizerStateSolid.CullMode = CullMode.None;
            rasterizerStateSolid.MultiSampleAntiAlias = true;
            rasterizerStateSolid.FillMode = FillMode.Solid;
            rasterizerStateSolid.SlopeScaleDepthBias = 0.1f;
            graphics.RasterizerState = rasterizerStateSolid;

            rasterizerStateWireFrame = new RasterizerState();
            //Desenha todas as faces, independentemente da orientação
            rasterizerStateWireFrame.CullMode = CullMode.None;
            rasterizerStateWireFrame.MultiSampleAntiAlias = true;
            rasterizerStateWireFrame.FillMode = FillMode.WireFrame;

            //Coloca o rato no centro do ecrã
            Mouse.SetPosition(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);

            originalMouseState = Mouse.GetState();
        }

        static private float getAlturaFromHeightmap()
        {
            //Posição arredondada para baixo da camara
            int xCamera, zCamera;
            xCamera = (int)position.Y;
            zCamera = (int)position.Z;
            
            //Os 4 vértices que rodeiam a posição da camara
            Vector2 pontoA, pontoB, pontoC, pontoD;
            pontoA = new Vector2(xCamera, zCamera);
            pontoB = new Vector2(xCamera + 1, zCamera);
            pontoC = new Vector2(xCamera, zCamera + 1);
            pontoD = new Vector2(xCamera + 1, zCamera + 1);

            //Recolher a altura de cada um dos 4 vértices à volta da câmara a partir do heightmap
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
            return Y + 1;
        }

        /// <summary>
        /// Calcula e atualiza a ViewMatrix para cada frame, consoante a posição e rotação da camâra
        /// </summary>
        static private void UpdateViewMatrix()
        {

            switch (tipoCamera)
            {
                case TipoCamera.FPS:
                    
                    float alturaHeightmap = getAlturaFromHeightmap();
                    position.Y = alturaHeightmap;

                    //Cálculo da matriz de rotação
                    Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
                    //Target
                    Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
                    Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
                    Vector3 cameraFinalTarget = position + cameraRotatedTarget;
                    //Cálculo do vector Up
                    Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
                    Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);
                    //Matriz View
                    View = Matrix.CreateLookAt(position, cameraFinalTarget, cameraRotatedUpVector);
                    break;
                case TipoCamera.Free:
                    //Cálculo da matriz de rotação
                    cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
                    //Target
                    cameraOriginalTarget = new Vector3(0, 0, -1);
                    cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
                    cameraFinalTarget = position + cameraRotatedTarget;
                    //Cálculo do vector Up
                    cameraOriginalUpVector = new Vector3(0, 1, 0);
                    cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);
                    //Matriz View
                    View = Matrix.CreateLookAt(position, cameraFinalTarget, cameraRotatedUpVector);
                    break;
                case TipoCamera.thirdPerson:
                    break;
                default:
                    break;
            }               
           
            //Atualiza o frustum
            frustum = new BoundingFrustum(View * Projection);
        }

        /// <summary>
        /// Implementa os controlos da camâra
        /// </summary>
        /// <param name="amount">Tempo decorrido desde o ultimo update</param>
        /// <param name="graphics">Instância de graphicsDevice</param>
        static private void ProcessInput(float amount, GraphicsDevice graphics)
        {
            //Movimento do rato
            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference * amount;
                updownRot -= rotationSpeed * yDifference * amount;
                try
                {
                    Mouse.SetPosition(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
                }
                catch (Exception)
                {
                    //Impede de dar erro quando se sai do programa
                }
                UpdateViewMatrix();
            }

            if (Mouse.GetState().ScrollWheelValue > mouseStateAnterior.ScrollWheelValue)
            {
                moveSpeed += (Mouse.GetState().ScrollWheelValue - mouseStateAnterior.ScrollWheelValue) / 100;
            }
            if (Mouse.GetState().ScrollWheelValue < mouseStateAnterior.ScrollWheelValue)
            {
                if (moveSpeed > 0.5f) moveSpeed -= (mouseStateAnterior.ScrollWheelValue - Mouse.GetState().ScrollWheelValue) / 50;
                if (moveSpeed < 0.2f) moveSpeed = 0.2f;
            }

            //Controlos do teclado
            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Q))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.E))
                moveVector += new Vector3(0, -1, 0);
            if (keyState.IsKeyDown(Keys.O) && !keyStateAnterior.IsKeyDown(Keys.O))
            {
                if (graphics.RasterizerState == rasterizerStateSolid)
                {
                    graphics.RasterizerState = rasterizerStateWireFrame;
                }
                else
                {
                    graphics.RasterizerState = rasterizerStateSolid;
                }
            }
            if (keyState.IsKeyDown(Keys.C) && !keyStateAnterior.IsKeyDown(Keys.C))
            {
                if (tipoCamera == TipoCamera.FPS)
                {
                    tipoCamera = TipoCamera.Free;
                }
                else
                {
                    tipoCamera = TipoCamera.FPS;
                }
            }
            if (keyState.IsKeyDown(Keys.N) && !keyStateAnterior.IsKeyDown(Keys.N))
            {
                drawNormals = !drawNormals;
            }
            if (keyState.IsKeyDown(Keys.Add))
            {
                moveSpeed += 0.5f;
            }
            if (keyState.IsKeyDown(Keys.Subtract))
            {
                if(moveSpeed > 0.5f) moveSpeed -= 0.5f;
            }
               
            AddToCameraPosition(moveVector * amount);

            keyStateAnterior = keyState;
            mouseStateAnterior = Mouse.GetState();
        }

        /// <summary>
        /// Atualiza a posição da camâra
        /// </summary>
        /// <param name="vectorToAdd"></param>
        static private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            position += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        /// <summary>
        /// Atualiza os parâmetros da camâra
        /// </summary>
        /// <param name="gameTime">Instância de gameTime</param>
        /// <param name="graphics">Instância de graphicsDevice</param>
        static public void Update(GameTime gameTime, GraphicsDevice graphics)
        {
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            ProcessInput(timeDifference, graphics);
        }


    }
}
