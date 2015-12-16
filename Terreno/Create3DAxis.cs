using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{
    static class Create3DAxis
    {
        /// <summary>
        /// Array para guardar os vértices e respetivas cores
        /// </summary>
        static private VertexPositionColor[] vertexList;

        /// <summary>
        /// Gera a geometria
        /// </summary>
        /// <param name="graphics"></param>
        static public void Initialize(GraphicsDevice graphics)
        {
            vertexList = new VertexPositionColor[6];
            int size = 2;

            //Eixo dos XX
            vertexList[0] = new VertexPositionColor(new Vector3(-size, 0, 0), Color.Red);
            vertexList[1] = new VertexPositionColor(new Vector3(size, 0, 0), Color.Red);

            //Eixo dos YY
            vertexList[2] = new VertexPositionColor(new Vector3(0, -size, 0), Color.Green);
            vertexList[3] = new VertexPositionColor(new Vector3(0, size, 0), Color.Green);

            //Eixo dos ZZ
            vertexList[4] = new VertexPositionColor(new Vector3(0, 0, -size), Color.Blue);
            vertexList[5] = new VertexPositionColor(new Vector3(0, 0, size), Color.Blue);
        }

        /// <summary>
        /// Desenha a geometria
        /// </summary>
        /// <param name="graphics">Instância de graphicsDevice</param>
        static public void Draw(GraphicsDevice graphics, BasicEffect efeito, Matrix world)
        {
            //World, View, Projection
            efeito.World = world;
            efeito.View = Camera.View;
            efeito.Projection = Camera.Projection;

            //Iluminação
            efeito.VertexColorEnabled = true;

            //Fog
            efeito.FogEnabled = true;
            efeito.FogColor = Vector3.Zero;
            efeito.FogStart = Camera.nearPlane;
            efeito.FogEnd = Camera.farPlane;

            foreach (EffectPass pass in efeito.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserPrimitives(PrimitiveType.LineList, vertexList, 0, 3);
            }
        }
    }
}
