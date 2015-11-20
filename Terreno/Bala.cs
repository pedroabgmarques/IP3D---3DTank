﻿using Microsoft.Xna.Framework;
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

        public Bala(ContentManager content, Tank tanqueQueDisparou)
        {

            speed = 0.25f;

            vetorBase = new Vector3(0, 0, 1);

            this.inclinationMatrix = tanqueQueDisparou.inclinationMatrix;
            rotationMatrix = Matrix.CreateRotationX(tanqueQueDisparou.CannonRotation)
                   * Matrix.CreateRotationY(tanqueQueDisparou.TurretRotation)
                   * Matrix.CreateFromQuaternion(tanqueQueDisparou.inclinationMatrix.Rotation)
                   ;
            Vector3 offset = new Vector3(0, 0.5f, 0.6f);
            direcao =
                Vector3.Transform(vetorBase, rotationMatrix);
            position = tanqueQueDisparou.position + Vector3.Transform(offset, rotationMatrix);

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
            position.Y -= totalTimePassed * totalTimePassed * speed; //Gravidade
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
                    effect.DiffuseColor = Color.Green.ToVector3();
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }
    }
}
