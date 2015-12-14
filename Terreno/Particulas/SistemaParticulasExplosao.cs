using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno.Particulas
{
    static public class SistemaParticulasExplosao
    {
        //Propriedades do sistema
        //Lista de particulas
        static List<ParticulaExplosao> particulasVivas;
        static List<ParticulaExplosao> particulasMortas;
        static List<ParticulaExplosao> particulasRemover;
        static ParticulaExplosao particula;
        static Color cor1, cor2;

        static public int getNParticulasVivas()
        {
            return particulasVivas.Count;
        }

        static public int getNParticulasMortas()
        {
            return particulasMortas.Count;
        }

        static public void Initialize(Random random)
        {
            particulasVivas = new List<ParticulaExplosao>(10000);
            particulasMortas = new List<ParticulaExplosao>(10000);
            particulasRemover = new List<ParticulaExplosao>(5000);

            for (int i = 0; i < 10000; i++)
            {
                particulasMortas.Add(new ParticulaExplosao(
                    Vector3.Zero, 
                    0, 
                    0, 
                    random, 
                    Vector3.Zero, 
                    0));
            }
        }

        static public void inserirExplosao(Vector3 posicao, int nParticulas, float velocidadeMedia, float perturbacao, Vector3 direcao, float alturaExplosao, Color corInicial, Color corFinal)
        {
            for (int i = 0; i < nParticulas; i++)
            {
                particula = particulasMortas.First();
                particulasMortas.Remove(particula);
                particula.totalTimePassed = 0;
                particula.posicao = posicao;
                particula.posicao.Y += 0.2f;
                particula.velocidadeMedia = velocidadeMedia;
                particula.perturbacao = perturbacao;
                particula.direcao = direcao;
                particula.alturaExplosao = alturaExplosao;

                //Inicilizar propriedades
                if (alturaExplosao < 1.5f)
                {
                    cor1 = Color.Blue;
                    cor2 = Color.White;
                }
                else
                {
                    cor1 = corInicial;
                    cor2 = corFinal;
                }

                //Criar os vértices da particula
                particula.vertexes[0].Position = posicao;
                particula.vertexes[0].Color = cor1;
                particula.vertexes[1].Position = posicao - new Vector3(0, 0.01f, 0);
                particula.vertexes[1].Color = cor2;

                //Calcular a direção da particula
                float angulo = (float)particula.random.NextDouble() * MathHelper.TwoPi;
                particula.direcao.X = (float)particula.random.NextDouble() * (float)Math.Cos(angulo);
                particula.direcao.Z = (float)particula.random.NextDouble() * (float)Math.Sin(angulo);
                particula.direcao.Normalize();
                particula.direcao *= (float)particula.random.NextDouble() * velocidadeMedia + perturbacao;

                particulasVivas.Add(particula);
                
            }
            
        }

        static public void Update(Random random, GameTime gameTime)
        {
            //Atualizar as particulas deste sistema
            foreach (ParticulaExplosao particula in particulasVivas)
            {
                particula.Update(random, gameTime);
            }

            //Verificar as particulas que devem morrer
            matarParticulas();
        }

        static private void matarParticulas()
        {
            particulasRemover.Clear();
            foreach (ParticulaExplosao particula in particulasVivas)
            {
                if (withinBounds() && particula.posicao.Y < Camera.getAlturaFromHeightmap(particula.posicao))
                {
                    particulasRemover.Add(particula);
                }
            }
            foreach (ParticulaExplosao particula in particulasRemover)
            {
                particulasVivas.Remove(particula);
                particulasMortas.Add(particula);
            }
        }

        static private bool withinBounds()
        {
            return (particula.posicao.X > 1 && particula.posicao.X < Terrain.altura - 1
                && particula.posicao.Z > 1 && particula.posicao.Z < Terrain.altura - 1);
        }

        static public void Draw(GraphicsDevice graphics, BasicEffect efeito)
        {
            //Desenhar as particulas deste sistema 
            foreach (ParticulaExplosao particula in particulasVivas)
            {
                particula.Draw(graphics, efeito);
            }
        }
    }
}
