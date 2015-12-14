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
                if (boid != tank && tank.equipa == boid.equipa)
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
                    if (tank.sargento && boid.sargento)
                    {
                        distancia -= (boid.position - tank.position) * 80;
                    }
                    else
                    {
                        if (Vector3.Distance(boid.position, tank.position) < distanciaMin)
                        {
                            distancia -= (boid.position - tank.position);
                        }
                    }
                    
                }
            }
            return distancia;
        }

        //Faz com que andem juntinhos mais ou menos no mesmo sentido
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

        //Dirigir-se para uma determinada posição
        public static Vector3 moverParaDirecao(Tank tank, Vector3 posicao, bool alvoSargento)
        {
            if (tank.sargento && alvoSargento)
            {
                return (posicao - tank.position) / 50f;
            }
            else
            {
                return (posicao - tank.position) / 20f;
            }
            
        }

    }
}
