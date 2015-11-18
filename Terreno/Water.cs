using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{

    //y = rEsfera * sin(Phi) -> Phi é o angulo que gera os paralelos
    //raio paralelo = rEsfera * cos(Phi)
    //x = raio paralelo * cos(Theta)
    //z = raio paralelo * sen(Theta)
    //heightmap: mapear raio da esfera para as alturas do heightmap

    static public class Water
    {

        static public VertexPositionNormalTexture[] vertexes;

        //Array de índices
        static private int[] indexes;

        //Buffers
        static private VertexBuffer vertexBuffer;
        static private IndexBuffer indexBuffer;

        static private int width;

        static public void GenerateWater(GraphicsDevice graphics, int largura)
        {
            width = largura / 4;

            vertexes = new VertexPositionNormalTexture[width * width];
            indexes = new int[(width * 2) * (width - 1)];

            //Gerar vértices
            int x = 0, z = 0;
            for (int j = 0; j < width / 2; j++) //Criamos duas colunas de vértices de cada vez
            {
                for (int i = 0; i < width * 2; i++)
                {
                    //Calcular coordenadas da textura
                    int u, v;
                    u = (x % 2 == 0) ? 0 : 1;
                    v = (z % 2 == 0) ? 0 : 1;

                    vertexes[(2 * j * width) + i] = new VertexPositionNormalTexture(new Vector3(x * 4, 2.0001f, z * 4), Vector3.Up, new Vector2(u, v));

                    z++;
                    if (z >= width)
                    {
                        //Criámos uma faixa vertical de vértices, passar para a outra faixa
                        x++;
                        z = 0;
                    }

                }

            }

            //Gerar Índices
            for (int j = 0; j < width - 1; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    indexes[2 * j * width + 2 * i] = (j * width + i);
                    indexes[2 * j * width + 2 * i + 1] = (j * width + i + width);
                }
            }

            vertexBuffer = new VertexBuffer(graphics, typeof(VertexPositionNormalTexture), vertexes.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertexes);

            indexBuffer = new IndexBuffer(graphics, typeof(int), indexes.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indexes);
        }

        static public void Draw(GraphicsDevice graphics, BasicEffect efeito, BasicEffect efeitoDeepWater)
        {
            //World, View, Projection
            efeito.World = Matrix.Identity; //* Matrix.CreateTranslation(new Vector3(- (float)(altura-1) / 2, -altura/6, - (float)(altura-1) / 2));
            efeito.View = Camera.View;
            efeito.Projection = Camera.Projection;

            //DEBUG
            //Desenhar normais
            //if (Camera.drawNormals)
            //{
            //    DebugShapeRenderer.SetWorld(efeito.World);
            //    for (int i = 0; i < vertexes.Length; i++)
            //    {
            //        DebugShapeRenderer.AddLine(vertexes[i].Position, vertexes[i].Position + vertexes[i].Normal, Color.Red);
            //    }
            //}

            // Define os filtros desejados
            SamplerState sampler = new SamplerState();
            sampler.Filter = TextureFilter.Anisotropic;
            sampler.MaxAnisotropy = 16;
            graphics.SamplerStates[0] = sampler;

            // Commit the changes to basic effect so it knows you made modifications  
            efeito.CurrentTechnique.Passes[0].Apply();

            //Desenhar
            for (int i = 0; i < width - 1; i++)
            {
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertexes, i * width, 
                    width * 2, indexes, 0, width * 2 - 2);
            }

            //Levantar todos os vértices
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i].Position.Y = vertexes[i].Position.Y + 0.21f;
            }

            //Desenhar novamente
            for (int i = 0; i < width - 1; i++)
            {
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertexes, i * width, 
                    width * 2, indexes, 0, width * 2 - 2);
            }

            //Baixar todos os vértices
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i].Position.Y = vertexes[i].Position.Y - 0.21f;
            }

        }

    }
}
