using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{
    static public class CollisionDetector
    {

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
                            bala.alive = false;

                            //Se for o tanque do jogador, escolher um novo tanque para o jogador
                            if (tanque.isAtivo())
                            {
                                tanque.desativarTanque();
                                Tank tankPlayer1 = tanques[random.Next(0, tanques.Count)];
                                tankPlayer1.ativarTanque();
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

        static public void CollisionBalaTerrain(List<Bala> listaBalas)
        {
            foreach (Bala bala in listaBalas)
            {
                if(bala.position.Y <= Camera.getAlturaFromHeightmap(bala.position)){
                    bala.alive = false;
                }
            }
        }

    }
}
