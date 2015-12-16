using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno.Particulas
{
    public class SistemaParticulasPo
    {
        //Propriedades do sistema de particulas
        //Lista de particulas
        List<ParticulaPo> particulas;
        Vector3 posicaoCentro;
        Vector3 offset;
        Vector3 transformedOffset;
        Matrix rotacao;
        int nParticulasSegundo;

        public SistemaParticulasPo(Random random, int nParticulas, Tank tank)
        {
            //Inicializar as propriedades
            this.nParticulasSegundo = nParticulas;
            particulas = new List<ParticulaPo>(1000);
            offset = new Vector3(0, 0.15f, -0.45f);
            
        }

        private void inserirNovaParticula(Random random, Tank tank)
        {

            rotacao = Matrix.CreateFromQuaternion(tank.inclinationMatrix.Rotation) * Matrix.CreateTranslation(tank.position);

            Vector3 posicao = Vector3.Zero;
            posicao.X = (float)random.NextDouble() * 1.8f * 0.5f - 0.5f;

            posicao = Vector3.Transform(posicao + offset, rotacao);

            float velocidadeMedia = 0.01f;
            float perturbacao = 0.005f;

            //Adicionar nova particula à lista de particulas deste sistema
            particulas.Add(new ParticulaPo(posicao, velocidadeMedia, perturbacao, random, Color.Chocolate, tank));
        }

        public void Update(Random random, GameTime gameTime, Tank tank)
        {

            //Atualizar as particulas de chuva
            foreach (ParticulaPo particula in particulas)
            {
                particula.Update(gameTime);
            }
            //Verificar particulas que devem morrer e criar novas particulas para as substituir
            matarERenascerParticulas(random, tank);
        }

        private void matarERenascerParticulas(Random random, Tank tank)
        {

            //Inserir particulas
            for (int i = 0; i < nParticulasSegundo; i++)
            {
                inserirNovaParticula(random, tank);
            }

            //Encontrar todas as particulas que estejam abaixo do plano
            List<ParticulaPo> listaRemover = particulas.FindAll(x => x.posicao.Y < -1);

            //Remover todas as particulas que se encontram abaixo do plano
            foreach (ParticulaPo particula in listaRemover)
            {
                particulas.Remove(particula);
            }

        }

        Matrix world = Matrix.Identity;
        public void Draw(GraphicsDevice graphics, BasicEffect efeito, Tank tank)
        {
            //Desenhar as particulas geridas por este sistema
            foreach (ParticulaPo particula in particulas)
            {
                particula.Draw(graphics, efeito);
            }
        }
    }
}
