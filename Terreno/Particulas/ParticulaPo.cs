using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno.Particulas
{
    public class ParticulaPo
    {
        //Propriedades da particula
        public Vector3 posicao;
        float velocidadeMedia;
        float perturbacao;
        Vector3 direcao;
        float totalTimePassed;
        Matrix worldSistema;

        //Array de vértices da particula
        private VertexPositionColor[] vertexes;

        public ParticulaPo(float velocidadeMedia, float perturbacao, Random random, Color cor, Tank tank, Matrix worldSistema)
        {
            this.worldSistema = worldSistema;

            //Inicializar o array de vértices (dois vértices para cada particula)
            vertexes = new VertexPositionColor[2];

            Vector3 posicao = Vector3.Zero;
            posicao.X = (float)random.NextDouble() * 1.8f * 0.5f - 0.5f;

            //Inicilizar propriedades
            this.posicao = posicao;
            this.velocidadeMedia = velocidadeMedia;
            this.perturbacao = perturbacao;
            this.totalTimePassed = 0;

            //Gerar os dois vértices da particula, um ligeiramente mais abaixo que o outro
            vertexes[0] = new VertexPositionColor(this.posicao, cor);
            vertexes[1] = new VertexPositionColor(this.posicao - new Vector3(0, 0.01f, 0), cor);

            direcao = Vector3.Zero;
            //Calcular direção da particula
            direcao.X = tank.inclinationMatrix.Backward.X * (float)random.NextDouble() * (1f * perturbacao - perturbacao);
            direcao.Z = tank.inclinationMatrix.Backward.Z * (float)random.NextDouble() * (1f * perturbacao - perturbacao);
            direcao += new Vector3(0, 0.01f, -0.01f);
            direcao.Normalize();
            direcao *= (float)random.NextDouble() * velocidadeMedia + perturbacao;


        }

        public void Update(GameTime gameTime)
        {
            //Atualizar a posição da particula
            posicao += direcao;
            totalTimePassed += (float)gameTime.ElapsedGameTime.Milliseconds / 4096.0f;
            posicao.Y -= totalTimePassed * totalTimePassed * velocidadeMedia * 20f; //Gravidade

            //Atualizar vértices da particula
            vertexes[0].Position = posicao;
            vertexes[1].Position = posicao - new Vector3(0, 0.01f, 0);
        }

        public void Draw(GraphicsDevice graphics, BasicEffect efeito)
        {

            //World, View, Projection
            efeito.World = worldSistema;
            efeito.View = Camera.View;
            efeito.Projection = Camera.Projection;

            //Create3DAxis.Draw(graphics, efeito, world);

            foreach (EffectPass pass in efeito.CurrentTechnique.Passes)
            {
                pass.Apply();

                //Desenhar as primitivas
                graphics.DrawUserPrimitives(PrimitiveType.LineList, vertexes, 0, 1);
            }
        }
    }
}
