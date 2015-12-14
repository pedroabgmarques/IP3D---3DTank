using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno.Particulas
{
    public class ParticulaExplosao
    {
        //Propriedades da particula
        public Vector3 posicao;
        public float velocidadeMedia;
        public float perturbacao;
        public Vector3 direcao;
        public float totalTimePassed;
        public float alturaExplosao;
        public Random random;

        //Array de vértices da particula
        public VertexPositionColor[] vertexes;

        public ParticulaExplosao(Vector3 posicao, float velocidadeMedia, float perturbacao, Random random, Vector3 direcaoInicial, float alturaExplosao)
        {
            this.random = random;
            //Inicializar o array de vértices, sendo que cada particula tem dois vértices
            vertexes = new VertexPositionColor[2];

        }

        public void Update(Random random, GameTime gameTime)
        {
            //Atualizar a posição da particula
            posicao += direcao;
            totalTimePassed += (float)gameTime.ElapsedGameTime.Milliseconds / 4096.0f;
            posicao.Y -= totalTimePassed * totalTimePassed * velocidadeMedia * 7f; //Gravidade

            //Atualizar vértices
            vertexes[0].Position = posicao + direcao;
            vertexes[1].Position = (posicao + new Vector3(0, 0.05f, 0));
        }

        public void Draw(GraphicsDevice graphics, BasicEffect efeito)
        {

            //World, View, Projection
            efeito.World = Matrix.Identity;
            efeito.View = Camera.View;
            efeito.Projection = Camera.Projection;

            foreach (EffectPass pass in efeito.CurrentTechnique.Passes)
            {
                pass.Apply();

                //Desenhar as primitivas
                graphics.DrawUserPrimitives(PrimitiveType.LineList, vertexes, 0, 1);
            }
        }
    }
}
