using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terreno.Particulas;

namespace Terreno
{
    static public class CollisionDetector
    {

        static private List<Tank> potenciaisTanquesJogadorMorto = new List<Tank>(300);
        static public void CollisionBalaTank(List<Tank> tanques, List<Bala> balas, Random random)
        {
            foreach (Bala bala in balas)
            {
                foreach (Tank tanque in tanques)
                {
                    if (tanque != bala.tanqueQueDisparou)
                    {
                        if (bala.BoundingSphere.Intersects(tanque.boundingSphere))
                        {
                            tanque.alive = false;
                            BalaManager.KillBala(bala);
                            SistemaParticulasExplosao.inserirExplosao(tanque.position, 75, 0.06f, 0.04f,
                                Terrain.getNormalFromHeightmap(tanque.position), tanque.position.Y, Color.Red, Color.Orange);
                            SistemaParticulasExplosao.inserirExplosao(tanque.position, 50, 0.06f, 0.04f, Vector3.Right,
                                tanque.position.Y, Color.Yellow, Color.White);

                            //Se for o tanque do jogador, escolher um novo tanque para o jogador
                            if (tanque.isAtivo())
                            {
                                tanque.desativarTanque();
                                potenciaisTanquesJogadorMorto.Clear();
                                foreach (Tank tank in tanques)
                                {
                                    if (tank.equipa == tanque.equipa)
                                    {
                                        potenciaisTanquesJogadorMorto.Add(tank);
                                    }
                                }
                                Tank tankPlayer1 = potenciaisTanquesJogadorMorto[random.Next(0, potenciaisTanquesJogadorMorto.Count)];
                                tankPlayer1.ativarTanque(tanques);
                            }

                        } 
                    }
                }
            }
        }

        static public void CollisionTankTank(Tank tanque, List<Tank> listaTanques)
        {
            foreach (Tank outroTanque in listaTanques)
            {
                if (tanque != outroTanque && tanque.boundingSphere.Intersects(outroTanque.boundingSphere))
                {
                    tanque.position = tanque.positionAnterior;
                    tanque.direcao += tanque.direcao;
                    break;
                }
            }
        }

        static public void CollisionTankFrontiers(Tank tanque)
        {
            if(tanque.position.X < 0 + tanque.boundingSphere.Radius
                || tanque.position.X > Terrain.altura - tanque.boundingSphere.Radius)
            {

                tanque.position.X = tanque.positionAnterior.X;
            }
            if(tanque.position.Z < 0 + tanque.boundingSphere.Radius
                || tanque.position.Z > Terrain.altura - tanque.boundingSphere.Radius)
            {
                tanque.position.Z = tanque.positionAnterior.Z;
            }
        }

        static public void CollisionBalaTerrain(List<Bala> listaBalas, Random random)
        {
            foreach (Bala bala in listaBalas)
            {
                if(bala.position.X > 1 && bala.position.X < Terrain.altura - 1
                        && bala.position.Z > 1 && bala.position.Z < Terrain.altura - 1 &&
                        bala.position.Y <= Camera.getAlturaFromHeightmap(bala.position)){
                            BalaManager.KillBala(bala);
                    
                            SistemaParticulasExplosao.inserirExplosao(bala.position, 50, 0.03f, 0.01f,
                                            Terrain.getNormalFromHeightmap(bala.position), bala.position.Y, Color.Red, Color.Yellow);
                    
                }
            }
        }

        static public void CollisionTankPalmeira(Tank tank, List<Palmeira> listaPalmeiras)
        {
            
            foreach (Palmeira palmeira in listaPalmeiras)
            {
                if (tank.boundingSphere.Intersects(palmeira.boundingBox))
                {
                    tank.position = tank.positionAnterior;
                    tank.direcao += Vector3.Right;
                        
                }
            }
            
        }

    }
}
