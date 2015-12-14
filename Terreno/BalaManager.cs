using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{
    static public class BalaManager
    {
        static private List<Bala> listaBalasVivas;
        static private List<Bala> listaBalasMortas;
        static private List<Bala> listaBalasRemover;

        static public int getNBalasVivas()
        {
            return listaBalasVivas.Count;
        }

        static public int getNBalasMortas()
        {
            return listaBalasMortas.Count;
        }

        static private Bala bala;

        static public List<Bala> getListaBalas()
        {
            return listaBalasVivas;
        }

        static public void Initialize(ContentManager content)
        {
            listaBalasRemover = new List<Bala>(100);
            listaBalasVivas = new List<Bala>(200);
            listaBalasMortas = new List<Bala>(200);
            for (int i = 0; i < 200; i++)
            {
                listaBalasMortas.Add(new Bala(content));
            }
        }

        static public void ShootBala(Tank tanqueQueDisparou, float desvioAleatorio)
        {
            bala = listaBalasMortas.First();
            listaBalasVivas.Add(bala);
            listaBalasMortas.Remove(bala);
            bala.RestartBala(tanqueQueDisparou, desvioAleatorio);
        }

        static public void KillBala(Bala bala)
        {
            bala.KillBala();
        }

        static public void RemoveDeadBalas()
        {
            foreach (Bala bala in listaBalasVivas)
            {
                if (!bala.alive)
                {
                    listaBalasRemover.Add(bala);
                }
            }
            foreach (Bala bala in listaBalasRemover)
            {
                listaBalasMortas.Add(bala);
                listaBalasVivas.Remove(bala);
            }
            listaBalasRemover.Clear();
        }
    }
}
