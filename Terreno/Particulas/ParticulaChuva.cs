using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno.Particulas
{
    public class ParticulaChuva
    {
        //Propriedades da particula
        public Vector3 posicao;
        float velocidadeMedia;
        private float totalTimePassed;
        float perturbacao;
        Vector3 direcao;

        //Array de vértices da particula
        private VertexPositionColor[] vertexes;

        public ParticulaChuva(Vector3 posicao, float velocidadeMedia, float perturbacao, Random random, Color cor)
        {
            //Inicializar o array de vértices (dois vértices para cada particula)
            vertexes = new VertexPositionColor[2];
            
            //Inicilizar propriedades
            this.posicao = posicao;
            this.velocidadeMedia = velocidadeMedia;
            this.perturbacao = perturbacao;

            //Gerar os dois vértices da particula, um ligeiramente mais abaixo que o outro
            vertexes[0] = new VertexPositionColor(this.posicao, cor);
            vertexes[1] = new VertexPositionColor(this.posicao - new Vector3(0, 0.1f, 0), cor);

            //Calcular direção da particula
            direcao = Vector3.Down;
            direcao.X = (float)random.NextDouble() * (2 * perturbacao - perturbacao);
            direcao.Z = (float)random.NextDouble() * (2 * perturbacao - perturbacao);
            direcao.Normalize();
            direcao *= (float)random.NextDouble() * velocidadeMedia + perturbacao;
            
            
        }

        public void Update(GameTime gameTime)
        {
            //Atualizar posição da particula
            posicao += direcao;
            totalTimePassed += (float)gameTime.ElapsedGameTime.Milliseconds / 4096.0f;
            posicao.Y -= totalTimePassed * totalTimePassed * velocidadeMedia * 7f; //Gravidade

            //Atualizar vértices da particula
            vertexes[0].Position = posicao;
            vertexes[0].Color = Color.Blue;
            vertexes[1].Position = posicao - new Vector3(0, 0.1f, 0);
            vertexes[1].Color = Color.LightBlue;
        }

        public void Draw(GraphicsDevice graphics, BasicEffect efeito)
        {

            //World, View, Projection
            efeito.World = Matrix.CreateTranslation(posicao);
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
