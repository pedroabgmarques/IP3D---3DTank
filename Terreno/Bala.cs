using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{
    public class Bala
    {
        private Model bala;
        public Vector3 position;
        private Matrix inclinationMatrix;
        private Vector3 vetorBase;
        private float speed;
        private Vector3 direcao;
        private Matrix rotationMatrix;
        private float totalTimePassed;
        private BoundingSphere boundingSphere;
        public bool alive;
        public Tank tanqueQueDisparou;
        private Matrix[] transforms;

        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
            set { boundingSphere = value; }
        }
        

        public Bala(ContentManager content)
        {
            speed = 0.3f;
            alive = false;

            vetorBase = new Vector3(0, 0, 1);

            LoadContent(content);
        }

        public void RestartBala(Tank tanqueQueDisparou, float desvioAleatorio)
        {
            this.alive = true;
            this.totalTimePassed = 0;
            this.tanqueQueDisparou = tanqueQueDisparou;
            this.inclinationMatrix = tanqueQueDisparou.inclinationMatrix;
            rotationMatrix = Matrix.CreateRotationX(tanqueQueDisparou.CannonRotation)
                   * Matrix.CreateRotationY(tanqueQueDisparou.TurretRotation + desvioAleatorio)
                   * Matrix.CreateFromQuaternion(tanqueQueDisparou.inclinationMatrix.Rotation)
                   ;

            direcao = Vector3.Transform(vetorBase, rotationMatrix);

            Vector3 offset = Vector3.Transform(new Vector3(0, 0.4f, 0), rotationMatrix);
            offset = direcao + offset;

            position = tanqueQueDisparou.position + offset;

            //DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(position, 0.1f), Color.Red, 5);

            boundingSphere.Center = position;
            boundingSphere.Radius = 0.1f;
        }

        public void KillBala()
        {
            this.alive = false;
        }


        private void LoadContent(ContentManager content)
        {
            bala = content.Load<Model>("Sphere");
        }

        public void Update(GameTime gameTime)
        {
            totalTimePassed += (float)gameTime.ElapsedGameTime.Milliseconds/4096.0f ;
            position += direcao * speed;
            position.Y -= totalTimePassed * totalTimePassed * speed * 2; //Gravidade

            boundingSphere.Center = position;
        }

        public void Draw()
        {
            // Copy any parent transforms.
            transforms = new Matrix[bala.Bones.Count];
            bala.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in bala.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = Matrix.CreateScale(0.05f)
                        * Matrix.CreateTranslation(this.position);
                    effect.View = Camera.View;
                    effect.Projection = Camera.Projection;
                    if (tanqueQueDisparou.equipa == Equipa.Empire)
                    {
                        effect.DiffuseColor = Color.Red.ToVector3();
                    }
                    else
                    {
                        effect.DiffuseColor = Color.DarkGreen.ToVector3();
                    }
                    
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }

            //DEBUG
            //DebugShapeRenderer.AddBoundingSphere(boundingSphere, Color.Yellow);
        }
    }
}
