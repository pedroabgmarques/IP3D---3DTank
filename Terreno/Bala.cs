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

        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
            set { boundingSphere = value; }
        }
        

        public Bala(ContentManager content, Tank tanqueQueDisparou)
        {
            this.tanqueQueDisparou = tanqueQueDisparou;
            speed = 0.2f;
            alive = true;

            vetorBase = new Vector3(0, 0, 1);

            this.inclinationMatrix = tanqueQueDisparou.inclinationMatrix;
            rotationMatrix = Matrix.CreateRotationX(tanqueQueDisparou.CannonRotation)
                   * Matrix.CreateRotationY(tanqueQueDisparou.TurretRotation)
                   * Matrix.CreateFromQuaternion(tanqueQueDisparou.inclinationMatrix.Rotation)
                   ;
            Vector3 offset = new Vector3(0, 0.5f, 0.8f);
           
            direcao = Vector3.Transform(vetorBase, rotationMatrix);

            offset = Vector3.Transform(offset, rotationMatrix);

            position = tanqueQueDisparou.position + offset;

            boundingSphere.Center = position;
            boundingSphere.Radius = 0.1f;

            LoadContent(content);
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
            Matrix[] transforms = new Matrix[bala.Bones.Count];
            bala.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in bala.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = Matrix.CreateScale(0.1f)
                        * Matrix.CreateTranslation(this.position);
                    effect.View = Camera.View;
                    effect.Projection = Camera.Projection;
                    effect.DiffuseColor = Color.Red.ToVector3();
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }

            //DEBUG
            DebugShapeRenderer.AddBoundingSphere(boundingSphere, Color.Yellow);
        }
    }
}
