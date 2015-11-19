using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{
    static public class AI
    {
        /// <summary>
        /// Calcula o centro de massa de todos os boids, excluindo ele proprio
        /// </summary>
        /// <param name="lista">Lista de todos os boids</param>
        /// <returns></returns>
        static public Vector3 centroMassaBoids(List<Tank> lista, Tank tank){
            Vector3 centroMassa = Vector3.Zero;
            foreach (Tank boid in lista)
            {
                if (boid != tank)
                {
                    centroMassa += boid.position;
                }
            }
            centroMassa = centroMassa / (lista.Count - 1);
            return (centroMassa - tank.position) / 500; //Move 1% do caminho        
        }

        /// <summary>
        /// Obriga os boids a manterem uma distância entre eles
        /// </summary>
        /// <param name="lista">Lista de boids</param>
        /// <param name="tank">Boid especifico</param>
        /// <param name="distanciaMin">Distancia minima a manter</param>
        /// <returns></returns>
        static public Vector3 manterDistancia(List<Tank> lista, Tank tank, float distanciaMin)
        {
            Vector3 distancia = Vector3.Zero;

            foreach (Tank boid in lista)
            {
                if (boid != tank)
                {
                    if (Vector3.Distance(boid.position, tank.position) < distanciaMin)
                    {
                        distancia -= (boid.position - tank.position);
                    }
                }
            }
            return distancia;
        }

        static public Vector3 combinarDirecao(List<Tank> lista, Tank tank)
        {
            Vector3 direcao = Vector3.Zero;

            foreach (Tank boid in lista)
            {
                if (boid != tank)
                {
                    direcao += boid.direcao;
                }
            }

            if (lista.Count > 1)
            {
                direcao = direcao / (lista.Count - 1);
            }
            

            return (direcao - tank.direcao) / 64;
        }

        public static Vector3 moverParaDirecao(Tank tank, Vector3 posicao)
        {
            return (posicao - tank.position) / 20;
        }

    }
}
