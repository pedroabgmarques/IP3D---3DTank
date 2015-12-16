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

        //Array de vértices da particula
        private VertexPositionColor[] vertexes;

        public ParticulaPo(Vector3 posicao, float velocidadeMedia, float perturbacao, Random random, Color cor, Tank tank)
        {
            //Inicializar o array de vértices (dois vértices para cada particula)
            vertexes = new VertexPositionColor[2];

            //Inicilizar propriedades
            this.posicao = posicao;
            this.velocidadeMedia = velocidadeMedia;
            this.perturbacao = perturbacao;
            this.totalTimePassed = 0;

            //Gerar os dois vértices da particula, um ligeiramente mais abaixo que o outro
            vertexes[0] = new VertexPositionColor(this.posicao, cor);
            vertexes[1] = new VertexPositionColor(this.posicao - new Vector3(0, 0.02f, 0), cor);

            direcao = Vector3.Zero;
            //Calcular direção da particula
            direcao.X = tank.inclinationMatrix.Forward.X * (float)random.NextDouble() * (2 * perturbacao - perturbacao);
            direcao.Z = tank.inclinationMatrix.Forward.Z * (float)random.NextDouble() * (2 * perturbacao - perturbacao);
            direcao.Normalize();
            direcao *= (float)random.NextDouble() * velocidadeMedia + perturbacao;


        }

        public void Update(GameTime gameTime)
        {
            //Atualizar a posição da particula
            posicao += direcao;
            totalTimePassed += (float)gameTime.ElapsedGameTime.Milliseconds / 4096.0f;
            posicao.Y -= totalTimePassed * totalTimePassed * velocidadeMedia * 7f; //Gravidade

            //Atualizar vértices da particula
            vertexes[0].Position = posicao;
            vertexes[1].Position = posicao - new Vector3(0, 0.02f, 0);
        }

        public void Draw(GraphicsDevice graphics, BasicEffect efeito)
        {

            //World, View, Projection
            efeito.World = Matrix.Identity;
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
