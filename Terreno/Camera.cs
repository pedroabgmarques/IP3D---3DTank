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
        SurfaceFollow,
        Free,
        ThirdPerson
    }

    static class Camera
    {

        //Matrizes World, View, Projection
        static public Matrix View, Projection, World;

        //Posição, direção e target
        static private Vector3 position, positionAnterior, direction, target;

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
        static public RasterizerState currentRasterizerState;

        //Desenhar normais do terreno
        static public bool drawNormals = false;

        //Tipo de camara (FPS, livre)
        static public TipoCamera tipoCamera;

        //Frustum da camara
        static public BoundingFrustum frustum;


        static public void Initialize(GraphicsDevice graphics)
        {
            tipoCamera = TipoCamera.ThirdPerson;

            farPlane = Terrain.altura + (Terrain.altura / 2);

            //Posição inicial da camâra
            position = new Vector3(30, 20, 30);

            //Vector de direção inicial
            direction = new Vector3(0, 0, -1f);

            //Colocar o rato no centro do ecrã
            Mouse.SetPosition(graphics.Viewport.Height / 2, graphics.Viewport.Width / 2);

            //Guardar a posição original do rato
            mouseStateOriginal = Mouse.GetState();

            //Inicializar as matrizes world, view e projection
            World = Matrix.Identity;
            Foward();
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

            currentRasterizerState = rasterizerStateSolid;

            frustum = new BoundingFrustum(Matrix.Identity);
        }

        static public float getAlturaFromHeightmap(Vector3 posicao)
        {
            //Posição arredondada para baixo
            int xPos, zPos;
            xPos = (int)posicao.X;
            zPos = (int)posicao.Z;

            //Os 4 vértices que rodeiam a posição da camara
            Vector2 pontoA, pontoB, pontoC, pontoD;
            pontoA = new Vector2(xPos, zPos);
            pontoB = new Vector2(xPos + 1, zPos);
            pontoC = new Vector2(xPos, zPos + 1);
            pontoD = new Vector2(xPos + 1, zPos + 1);

            if(pontoA.X > 0 && pontoA.X < Terrain.altura
            && pontoA.Y > 0 && pontoA.Y < Terrain.altura
            && pontoB.X > 0 && pontoB.X < Terrain.altura
            && pontoB.Y > 0 && pontoB.Y < Terrain.altura
            && pontoC.X > 0 && pontoC.X < Terrain.altura
            && pontoC.Y > 0 && pontoC.Y < Terrain.altura
            && pontoD.X > 0 && pontoD.X < Terrain.altura
            && pontoD.Y > 0 && pontoD.Y < Terrain.altura)
            {
                //Recolher a altura de cada um dos 4 vértices à volta da câmara a partir do heightmap
                float Ya, Yb, Yc, Yd;
                Ya = Terrain.vertexes[(int)pontoA.X * Terrain.altura + (int)pontoA.Y].Position.Y;
                Yb = Terrain.vertexes[(int)pontoB.X * Terrain.altura + (int)pontoB.Y].Position.Y;
                Yc = Terrain.vertexes[(int)pontoC.X * Terrain.altura + (int)pontoC.Y].Position.Y;
                Yd = Terrain.vertexes[(int)pontoD.X * Terrain.altura + (int)pontoD.Y].Position.Y;

                //Interpolação bilenear (dada nas aulas)
                float Yab = (1 - (posicao.X - pontoA.X)) * Ya + (posicao.X - pontoA.X) * Yb;
                float Ycd = (1 - (posicao.X - pontoC.X)) * Yc + (posicao.X - pontoC.X) * Yd;
                float Y = (1 - (posicao.Z - pontoA.Y)) * Yab + (posicao.Z - pontoA.Y) * Ycd;

                //Devolver a altura
                return Y;
            }
            else
            {
                return -1;
            }

            
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

        static public void Update(GameTime gameTime, GraphicsDevice graphics, Tank tank)
        {

            KeyboardState kb = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (tipoCamera == TipoCamera.SurfaceFollow || tipoCamera == TipoCamera.Free)
            {
                //Controlos do teclado
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

                if (mouseState.ScrollWheelValue > mouseStateAnterior.ScrollWheelValue)
                {
                    if (moveSpeed < 2f)
                        moveSpeed += (mouseState.ScrollWheelValue - mouseStateAnterior.ScrollWheelValue)
                            / 10000f;
                }
                if (Mouse.GetState().ScrollWheelValue < mouseStateAnterior.ScrollWheelValue)
                {
                    if (moveSpeed > 0.05f)
                        moveSpeed -= (mouseStateAnterior.ScrollWheelValue - mouseState.ScrollWheelValue)
                            / 10000f;
                }

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
            }
            
            if (kb.IsKeyDown(Keys.O) && !keyStateAnterior.IsKeyDown(Keys.O))
            {
                if (graphics.RasterizerState == rasterizerStateSolid)
                {
                    graphics.RasterizerState = rasterizerStateWireFrame;
                    currentRasterizerState = rasterizerStateWireFrame;
                }
                else
                {
                    graphics.RasterizerState = rasterizerStateSolid;
                    currentRasterizerState = rasterizerStateSolid;
                }
            }
            if (kb.IsKeyDown(Keys.C) && !keyStateAnterior.IsKeyDown(Keys.C))
            {
                switch (tipoCamera)
                {
                    case TipoCamera.SurfaceFollow:
                        tipoCamera = TipoCamera.Free;
                        break;
                    case TipoCamera.Free:
                        tipoCamera = TipoCamera.ThirdPerson;
                        break;
                    case TipoCamera.ThirdPerson:
                        tipoCamera = TipoCamera.SurfaceFollow;
                        break;
                    default:
                        tipoCamera = TipoCamera.Free;
                        break;
                }
                
            }
            if (kb.IsKeyDown(Keys.N) && !keyStateAnterior.IsKeyDown(Keys.N))
            {
                drawNormals = !drawNormals;
            }

            position = LimitarCameraTerreno(position);

            UpdateViewMatrix(tank);
            mouseStateAnterior = mouseState;
            keyStateAnterior = kb;

            positionAnterior = position;
        }

        static private Vector3 LimitarCameraTerreno(Vector3 position)
        {
            //Limitar a câmara aos limites do terreno
            if (position.X - 1 < 0)
            {
                position.X = positionAnterior.X;
            }
            if (position.Z - 1 < 0)
            {
                position.Z = positionAnterior.Z;
            }
            if (position.X + 1 > Terrain.altura - 1)
            {
                position.X = positionAnterior.X;
            }
            if (position.Z + 1 > Terrain.altura - 1)
            {
                position.Z = positionAnterior.Z;
            }
            return position;
        }

        static private void UpdateViewMatrix(Tank tank = null)
        {

            if (tipoCamera == TipoCamera.SurfaceFollow)
            {
                position.Y = getAlturaFromHeightmap(position) + 1;
            }

            if (tipoCamera == TipoCamera.SurfaceFollow || tipoCamera == TipoCamera.Free)
            {
                cameraRotation = Matrix.CreateFromYawPitchRoll(yaw, 0, pitch);
                World = cameraRotation;
                direction = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation);
                target = position + direction;
                View = Matrix.CreateLookAt(position, target, Vector3.Up);
                if (tipoCamera == TipoCamera.SurfaceFollow)
                {
                    position.Y = getAlturaFromHeightmap(position) + 1;
                }
                if (tipoCamera == TipoCamera.Free)
                {
                    if (position.Y < getAlturaFromHeightmap(position) + 0.2f)
                    {
                        position.Y = getAlturaFromHeightmap(position) + 0.2f;
                    }
                }
            }

            if (tipoCamera == TipoCamera.ThirdPerson)
            {

                Matrix rotationMatrix = Matrix.CreateRotationX(-tank.CannonRotation)
                    * Matrix.CreateRotationY(tank.TurretRotation)
                    * Matrix.CreateRotationY(MathHelper.ToRadians(tank.rotacaoY))
                    ;

                //Vector3 thirdPersonReference = new Vector3(0, 5f, 8f);
                Vector3 thirdPersonReference = new Vector3(0, 2.5f, 4f);

                Vector3 transformedReference =
                    Vector3.Transform(thirdPersonReference, rotationMatrix);

                Vector3 cameraPosition = transformedReference + tank.position;

                cameraPosition = LimitarCameraTerreno(cameraPosition);

                float alturaTerrenoPosicaoCam = getAlturaFromHeightmap(cameraPosition);

                if (cameraPosition.Y < alturaTerrenoPosicaoCam + 0.5f)
                {
                    cameraPosition.Y = alturaTerrenoPosicaoCam + 0.5f;
                }

                View = Matrix.CreateLookAt(cameraPosition, tank.position + rotationMatrix.Forward * 3,
                    Vector3.Cross(rotationMatrix.Left, transformedReference));

                position = cameraPosition;
                direction = tank.position - position;

            }

            frustum.Matrix = View * Projection;
            
            
        }
    }
}
