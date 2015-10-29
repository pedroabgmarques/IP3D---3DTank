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
        Free
    }

    static class Camera
    {

        //Matrizes World, View, Projection
        static public Matrix View, Projection, World;

        //Posição, direção e target
        static private Vector3 position, direction, target;

        //Position getter
        static public Vector3 getPosition()
        {
            return position;
        }

        //Velocidade do movimento (translação)
        static private float moveSpeed = 0.1f;

        //Velocidade da rotação
        static private float rotationSpeed = 0.005f;

        //Orientação da camara
        static private float yaw = 0, pitch = 0;
        
        //Quantidade de pixeis que o rato se moveu
        static private Vector2 mouseMoved;

        //Matriz de rotação da camara
        static private Matrix cameraRotation;

        //Posição original do rato
        static private MouseState mouseStateOriginal;

        //Posição anterior do rato
        static private MouseState mouseStateAnterior;

        //Estado anterior do teclado
        static KeyboardState keyStateAnterior;

        //Near e far plane
        static public float nearPlane = 0.1f;
        static public float farPlane;

        //RasterizerStates para solid / wireframe
        static RasterizerState rasterizerStateSolid;
        static RasterizerState rasterizerStateWireFrame;

        //Desenhar normais do terreno
        static public bool drawNormals = false;

        //Tipo de camara (FPS, livre)
        static public TipoCamera tipoCamera;


        static public void Initialize(GraphicsDevice graphics)
        {
            tipoCamera = TipoCamera.FPS;

            farPlane = Terrain.altura + (Terrain.altura / 2);

            //Posição inicial da camâra
            position = new Vector3(10, 20, 30);

            //Vector de direção inicial
            direction = new Vector3(0, -1f, 0);

            //Colocar o rato no centro do ecrã
            Mouse.SetPosition(graphics.Viewport.Height / 2, graphics.Viewport.Width / 2);

            //Guardar a posição original do rato
            mouseStateOriginal = Mouse.GetState();

            //Inicializar as matrizes world, view e projection
            World = Matrix.Identity;
            Foward();
            UpdateViewMatrix();
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                graphics.Viewport.AspectRatio,
                nearPlane,
                farPlane);

            //Criar e definir os resterizerStates a utilizar para desenhar a geometria
            //SOLID
            rasterizerStateSolid = new RasterizerState();
            rasterizerStateSolid.CullMode = CullMode.None;
            rasterizerStateSolid.MultiSampleAntiAlias = true;
            rasterizerStateSolid.FillMode = FillMode.Solid;
            rasterizerStateSolid.SlopeScaleDepthBias = 0.1f;
            graphics.RasterizerState = rasterizerStateSolid;

            //WIREFRAME
            rasterizerStateWireFrame = new RasterizerState();
            rasterizerStateWireFrame.CullMode = CullMode.None;
            rasterizerStateWireFrame.MultiSampleAntiAlias = true;
            rasterizerStateWireFrame.FillMode = FillMode.WireFrame;         
        }

        static private float getAlturaFromHeightmap()
        {
            //Posição arredondada para baixo da camara
            int xCamera, zCamera;
            xCamera = (int)position.X;
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

        static private void Foward()
        {
            position = position + moveSpeed * direction;
            target = position + direction;
        }

        static private void Backward()
        {
            position = position - moveSpeed * direction;
            target = position + direction;
        }

        static private void Up()
        {
            position = position + moveSpeed * Vector3.Up;
            target = position + direction;
        }

        static private void Down()
        {
            position = position + moveSpeed * Vector3.Down;
            target = position + direction;
        }

        static private void rotateLeftRight()
        {
            yaw -= mouseMoved.X * rotationSpeed;
        }

        static private void rotateUpDown()
        {
            pitch -= mouseMoved.Y * rotationSpeed;
        }


        static private void strafeLeft(GameTime gameTime, float strafe)
        {
            strafe = strafe + moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
            position = position - moveSpeed * Vector3.Cross(direction, Vector3.Up);
            target = position + direction;
        }

        static private void strafeRight(GameTime gameTime, float strafe)
        {
            strafe = strafe + moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
            position = position + moveSpeed * Vector3.Cross(direction, Vector3.Up);
            target = position + direction;
        }

        static public void Update(GameTime gameTime, GraphicsDevice graphics)
        {

            float posicaoXAnterior = position.X;
            float posicaoZAnterior = position.Z;

            //Controlos do teclado
            KeyboardState kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.W))
            {
                Foward();
            }
            if (kb.IsKeyDown(Keys.S))
            {
                Backward();

            }
            if (kb.IsKeyDown(Keys.A))
            {
                strafeLeft(gameTime, moveSpeed / 2);

            }
            if (kb.IsKeyDown(Keys.D))
            {
                strafeRight(gameTime, moveSpeed / 2);
            }
            if (kb.IsKeyDown(Keys.Q))
            {
                Up();
            }
            if (kb.IsKeyDown(Keys.E))
            {
                Down();
            }
            if (kb.IsKeyDown(Keys.O) && !keyStateAnterior.IsKeyDown(Keys.O))
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
            if (kb.IsKeyDown(Keys.C) && !keyStateAnterior.IsKeyDown(Keys.C))
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
            if (kb.IsKeyDown(Keys.N) && !keyStateAnterior.IsKeyDown(Keys.N))
            {
                drawNormals = !drawNormals;
            }

            //Controlo da velocidade com a roda do rato
            MouseState mouseState = Mouse.GetState();

            if (mouseState.ScrollWheelValue > mouseStateAnterior.ScrollWheelValue)
            {
                if(moveSpeed < 2f)
                    moveSpeed += (mouseState.ScrollWheelValue - mouseStateAnterior.ScrollWheelValue) / 10000f;
            }
            if (Mouse.GetState().ScrollWheelValue < mouseStateAnterior.ScrollWheelValue)
            {
                if(moveSpeed > 0.05f)
                    moveSpeed -= (mouseStateAnterior.ScrollWheelValue - mouseState.ScrollWheelValue) / 10000f;
            }
            Console.WriteLine(moveSpeed);

            //Controlo da rotação com o rato
            if (mouseState != mouseStateOriginal)
            {
                mouseMoved.X = mouseState.Position.X - mouseStateOriginal.Position.X;
                mouseMoved.Y = mouseState.Position.Y - mouseStateOriginal.Position.Y;
                rotateLeftRight();
                rotateUpDown();
                try
                {
                    Mouse.SetPosition(graphics.Viewport.Height / 2, graphics.Viewport.Width / 2);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            

            if (position.X - 1 < 0)
            {
                position.X = posicaoXAnterior;
            }
            if (position.Z - 1 < 0)
            {
                position.Z = posicaoZAnterior;
            }
            if (position.X + 1 > 127)
            {
                position.X = posicaoXAnterior;
            }
            if (position.Z + 1 > 127)
            {
                position.Z = posicaoZAnterior;
            }

            UpdateViewMatrix();
            mouseStateAnterior = mouseState;
            keyStateAnterior = kb;
        }

        static private void UpdateViewMatrix()
        {

            if (tipoCamera == TipoCamera.FPS)
            {
                position.Y = getAlturaFromHeightmap();
            }

            cameraRotation = Matrix.CreateFromYawPitchRoll(yaw, 0, pitch);
            World = cameraRotation;
            direction = Vector3.Transform(new Vector3(1, -0.5f, 0), cameraRotation);
            target = position + direction;
            View = Matrix.CreateLookAt(position, target, Vector3.Up);
        }
    }
}
