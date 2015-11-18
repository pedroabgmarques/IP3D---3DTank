using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{
    static class Terrain
    {

        //Array de vértices
        static public VertexPositionNormalTexture[] vertexes;

        //Array de índices
        static private int[] indexes;

        //Array de texels
        static Color[] texels;

        //Buffers
        static private VertexBuffer vertexBuffer;
        static private IndexBuffer indexBuffer;

        //Dimensões do terreno
        static public int altura;

        static public void GenerateTerrain(GraphicsDevice graphics, Texture2D heightmap)
        {

            //Gerar texels a partir do heightmap
            texels = new Color[heightmap.Width * heightmap.Width];
            heightmap.GetData<Color>(texels);

            altura = heightmap.Height;
            vertexes = new VertexPositionNormalTexture[altura * altura];

            //Gerar vértices
            int x = 0, z = 0;
            for (int j = 0; j < altura / 2; j++) //Criamos duas colunas de vértices de cada vez
            {
                for (int i = 0; i < altura * 2; i++)
                {
                    //Calcular coordenadas da textura
                    int u, v;
                    u = (x % 2 == 0) ? 0 : 1;
                    v = (z % 2 == 0) ? 0 : 1;

                    //Escalas:
                    //bigaltura (512 * 512): 0.2f;
                    //altura (128 * 128): 0.05f;
                    float scale = 0.035f;
                    vertexes[(2 * j * altura) + i] = new VertexPositionNormalTexture(
                        new Vector3(x, texels[(z * altura + x)].R * scale, z), 
                        Vector3.Zero, 
                        new Vector2(u, v));
                    z++;
                    if (z >= altura)
                    {
                        //Criámos uma faixa vertical de vértices, passar para a outra faixa
                        x++;
                        z = 0;
                    }

                }
                
            }

            //Gerar índices
            indexes = new int[(altura * 2) * (altura - 1)];

            for (int i = 0; i < indexes.Length / 2; i++)
            {
                indexes[2 * i] = (int)i;
                indexes[2 * i + 1] = (int)(i + altura);
            }

            //Calcular normais
            CalcularNormaisFoleiros();

            //Passar informação para o GPU
            vertexBuffer = new VertexBuffer(graphics, 
                typeof(VertexPositionNormalTexture), vertexes.Length, 
                BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertexes);
            
            indexBuffer = new IndexBuffer(graphics, typeof(int), indexes.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indexes);
        }

        //É necessário fazer outro método que percorra o array de vértices e seja mais eficiente
        static private void CalcularNormaisFoleiros()
        {
            //Cria as normais do interior do terreno
            for (int i = altura + 1; i < vertexes.Count() - altura - 2; i++)
            {
                Vector3 v1 = Vector3.Zero;
                Vector3 v2 = Vector3.Zero;
                Vector3 v3 = Vector3.Zero;
                Vector3 v4 = Vector3.Zero;
                Vector3 v5 = Vector3.Zero;
                Vector3 v6 = Vector3.Zero;
                Vector3 v7 = Vector3.Zero;
                Vector3 v8 = Vector3.Zero;
                Vector3 v9 = Vector3.Zero;

                v1 = vertexes[i].Position;
                v2 = vertexes[i + 1].Position;
                v3 = vertexes[i + 1 - altura].Position;
                v4 = vertexes[i - altura].Position;
                v5 = vertexes[i - 1 - altura].Position;
                v6 = vertexes[i - 1].Position;
                v7 = vertexes[i - 1 + altura].Position;
                v8 = vertexes[i + altura].Position;
                v9 = vertexes[i + 1 + altura].Position;

                Vector3 vt1 = v2 - v1;
                Vector3 vt2 = v3 - v1;
                Vector3 vt3 = v4 - v1;
                Vector3 vt4 = v5 - v1;
                Vector3 vt5 = v6 - v1;
                Vector3 vt6 = v7 - v1;
                Vector3 vt7 = v8 - v1;
                Vector3 vt8 = v9 - v1;





                Vector3 normal = Vector3.Cross(vt2, vt1);
                normal.Normalize();
                Vector3 normal1 = Vector3.Cross(vt3, vt2);
                normal1.Normalize();
                Vector3 normal2 = Vector3.Cross(vt4, vt3);
                normal2.Normalize();
                Vector3 normal3 = Vector3.Cross(vt5, vt4);
                normal3.Normalize();
                Vector3 normal4 = Vector3.Cross(vt6, vt5);
                normal4.Normalize();
                Vector3 normal5 = Vector3.Cross(vt7, vt6);
                normal5.Normalize();
                Vector3 normal6 = Vector3.Cross(vt8, vt7);
                normal6.Normalize();
                Vector3 normal7 = Vector3.Cross(vt1, vt8);
                normal7.Normalize();

                Vector3 normal8 = (normal + normal1 + normal2 + normal3 + normal4 + normal5 + normal6 + normal7) / 8;
                vertexes[i].Normal = normal8;
            }
            //Criar Normais para a primeira coluna, sem contar com os cantos           
            for (int z = altura; z < vertexes.Count() - altura; z = z + altura)
            {
                Vector3 v1 = Vector3.Zero;
                Vector3 v2 = Vector3.Zero;
                Vector3 v3 = Vector3.Zero;
                Vector3 v4 = Vector3.Zero;
                Vector3 v5 = Vector3.Zero;
                Vector3 v6 = Vector3.Zero;

                v1 = vertexes[z].Position;
                v2 = vertexes[z - altura].Position;
                v3 = vertexes[z + altura].Position;
                v4 = vertexes[z + 1].Position;
                v5 = vertexes[z - altura + 1].Position;
                v6 = vertexes[z + altura + 1].Position;

                Vector3 vt1 = v2 - v1;
                Vector3 vt2 = v5 - v1;
                Vector3 vt3 = v4 - v1;
                Vector3 vt4 = v6 - v1;
                Vector3 vt5 = v3 - v1;

                Vector3 normal = Vector3.Cross(vt2, vt1);
                normal.Normalize();
                Vector3 normal1 = Vector3.Cross(vt3, vt2);
                normal1.Normalize();
                Vector3 normal2 = Vector3.Cross(vt4, vt3);
                normal2.Normalize();
                Vector3 normal3 = Vector3.Cross(vt5, vt4);
                normal3.Normalize();
                Vector3 normal4 = (normal + normal1 + normal2 + normal3) / 4;

                vertexes[z].Normal = -normal4;
            }

            //Criar Normais para a última coluna, sem contar com os cantos           
            for (int z = altura * 2 - 1; z < vertexes.Count() - altura; z = z + altura)
            {
                Vector3 v1 = Vector3.Zero;
                Vector3 v2 = Vector3.Zero;
                Vector3 v3 = Vector3.Zero;
                Vector3 v4 = Vector3.Zero;
                Vector3 v5 = Vector3.Zero;
                Vector3 v6 = Vector3.Zero;

                v1 = vertexes[z].Position;
                v2 = vertexes[z - altura].Position;
                v3 = vertexes[z + altura].Position;
                v4 = vertexes[z - 1].Position;
                v5 = vertexes[z - altura - 1].Position;
                v6 = vertexes[z + altura - 1].Position;

                Vector3 vt1 = v2 - v1;
                Vector3 vt2 = v5 - v1;
                Vector3 vt3 = v4 - v1;
                Vector3 vt4 = v6 - v1;
                Vector3 vt5 = v3 - v1;

                Vector3 normal = Vector3.Cross(vt2, vt1);
                normal.Normalize();
                Vector3 normal1 = Vector3.Cross(vt3, vt2);
                normal1.Normalize();
                Vector3 normal2 = Vector3.Cross(vt4, vt3);
                normal2.Normalize();
                Vector3 normal3 = Vector3.Cross(vt5, vt4);
                normal3.Normalize();
                Vector3 normal4 = (normal + normal1 + normal2 + normal3) / 4;

                vertexes[z].Normal = normal4;
            }

            //Criar normais para a primeira linha, sem contar com os cantos
            for (int x = 1; x < altura - 1; x++)
            {
                Vector3 v1 = Vector3.Zero;
                Vector3 v2 = Vector3.Zero;
                Vector3 v3 = Vector3.Zero;
                Vector3 v4 = Vector3.Zero;
                Vector3 v5 = Vector3.Zero;
                Vector3 v6 = Vector3.Zero;

                v1 = vertexes[x].Position;
                v2 = vertexes[x - 1].Position;
                v3 = vertexes[x + 1].Position;
                v4 = vertexes[x + altura].Position;
                v5 = vertexes[x - 1 + altura].Position;
                v6 = vertexes[x + altura + 1].Position;

                Vector3 vt1 = v2 - v1;
                Vector3 vt2 = v5 - v1;
                Vector3 vt3 = v4 - v1;
                Vector3 vt4 = v6 - v1;
                Vector3 vt5 = v3 - v1;

                Vector3 normal = Vector3.Cross(vt2, vt1);
                normal.Normalize();
                Vector3 normal1 = Vector3.Cross(vt3, vt2);
                normal1.Normalize();
                Vector3 normal2 = Vector3.Cross(vt4, vt3);
                normal2.Normalize();
                Vector3 normal3 = Vector3.Cross(vt5, vt4);
                normal3.Normalize();
                Vector3 normal4 = (normal + normal1 + normal2 + normal3) / 4;

                vertexes[x].Normal = normal4;

            }

            //Criar normais para a última linha, sem contar com os cantos
            for (int x = vertexes.Count() - altura + 1; x < vertexes.Count() - 1; x++)
            {
                Vector3 v1 = Vector3.Zero;
                Vector3 v2 = Vector3.Zero;
                Vector3 v3 = Vector3.Zero;
                Vector3 v4 = Vector3.Zero;
                Vector3 v5 = Vector3.Zero;
                Vector3 v6 = Vector3.Zero;

                v1 = vertexes[x].Position;
                v2 = vertexes[x - 1].Position;
                v3 = vertexes[x + 1].Position;
                v4 = vertexes[x - altura].Position;
                v5 = vertexes[x - 1 - altura].Position;
                v6 = vertexes[x - altura + 1].Position;

                Vector3 vt1 = v2 - v1;
                Vector3 vt2 = v5 - v1;
                Vector3 vt3 = v4 - v1;
                Vector3 vt4 = v6 - v1;
                Vector3 vt5 = v3 - v1;

                Vector3 normal = Vector3.Cross(vt2, vt1);
                normal.Normalize();
                Vector3 normal1 = Vector3.Cross(vt3, vt2);
                normal1.Normalize();
                Vector3 normal2 = Vector3.Cross(vt4, vt3);
                normal2.Normalize();
                Vector3 normal3 = Vector3.Cross(vt5, vt4);
                normal3.Normalize();
                Vector3 normal4 = (normal + normal1 + normal2 + normal3) / 4;

                vertexes[x].Normal = -normal4;
            }



            //Cria a normal do vértice superior esquerdo(vertexes[0])
            for (int i = 0; i < 1; i++)
            {
                Vector3 v1 = Vector3.Zero;
                Vector3 v2 = Vector3.Zero;
                Vector3 v3 = Vector3.Zero;
                Vector3 v4 = Vector3.Zero;

                v1 = vertexes[i].Position;
                v2 = vertexes[i + 1].Position;
                v3 = vertexes[i + 1 + altura].Position;
                v4 = vertexes[i + altura].Position;


                Vector3 vt1 = v2 - v1;
                Vector3 vt2 = v3 - v1;
                Vector3 vt3 = v4 - v1;

                Vector3 normal = Vector3.Cross(vt2, vt1);
                normal.Normalize();
                Vector3 normal1 = Vector3.Cross(vt3, vt2);
                normal1.Normalize();
                Vector3 normal2 = (normal + normal1) / 2;

                vertexes[i].Normal = -normal2;

            }




            //Cria a normal do vértice superior direito ( vertexes[599])
            for (int i = altura - 1; i < altura; i++)
            {
                Vector3 v1 = Vector3.Zero;
                Vector3 v2 = Vector3.Zero;
                Vector3 v3 = Vector3.Zero;
                Vector3 v4 = Vector3.Zero;

                v1 = vertexes[i].Position;
                v2 = vertexes[i - 1].Position;
                v3 = vertexes[i - 1 + altura].Position;
                v4 = vertexes[i + altura].Position;


                Vector3 vt1 = v2 - v1;
                Vector3 vt2 = v3 - v1;
                Vector3 vt3 = v4 - v1;

                Vector3 normal = Vector3.Cross(vt2, vt1);
                normal.Normalize();
                Vector3 normal1 = Vector3.Cross(vt3, vt2);
                normal1.Normalize();
                Vector3 normal2 = (normal + normal1) / 2;

                vertexes[i].Normal = normal2;

            }


            //Cria a normal do vértice inferior esquerdo (vertexes[359400])
            for (int i = vertexes.Count() - altura; i < vertexes.Count() - altura + 1; i++)
            {
                Vector3 v1 = Vector3.Zero;
                Vector3 v2 = Vector3.Zero;
                Vector3 v3 = Vector3.Zero;
                Vector3 v4 = Vector3.Zero;

                v1 = vertexes[i].Position;
                v2 = vertexes[i + 1].Position;
                v3 = vertexes[i + 1 - altura].Position;
                v4 = vertexes[i - altura].Position;


                Vector3 vt1 = v2 - v1;
                Vector3 vt2 = v3 - v1;
                Vector3 vt3 = v4 - v1;

                Vector3 normal = Vector3.Cross(vt2, vt1);
                normal.Normalize();
                Vector3 normal1 = Vector3.Cross(vt3, vt2);
                normal1.Normalize();
                Vector3 normal2 = (normal + normal1) / 2;

                vertexes[i].Normal = normal2;

            }

            //Cria a normal do vértice inferior direito (vertexes[359999])
            for (int i = vertexes.Count() - 1; i < vertexes.Count(); i++)
            {
                Vector3 v1 = Vector3.Zero;
                Vector3 v2 = Vector3.Zero;
                Vector3 v3 = Vector3.Zero;
                Vector3 v4 = Vector3.Zero;

                v1 = vertexes[i].Position;
                v2 = vertexes[i - 1].Position;
                v3 = vertexes[i - 1 - altura].Position;
                v4 = vertexes[i - altura].Position;


                Vector3 vt1 = v2 - v1;
                Vector3 vt2 = v3 - v1;
                Vector3 vt3 = v4 - v1;

                Vector3 normal = Vector3.Cross(vt2, vt1);
                normal.Normalize();
                Vector3 normal1 = Vector3.Cross(vt3, vt2);
                normal1.Normalize();
                Vector3 normal2 = (normal + normal1) / 2;

                vertexes[i].Normal = -normal2;

            }
        }

    static public void Draw(GraphicsDevice graphics, BasicEffect efeito)
        {
            //World, View, Projection
            efeito.World = Matrix.Identity; //* Matrix.CreateTranslation(new Vector3(- (float)(altura-1) / 2, -altura/6, - (float)(altura-1) / 2));
            efeito.View = Camera.View;
            efeito.Projection = Camera.Projection;

            //DEBUG
            //Desenhar normais
            if (Camera.drawNormals)
            {
                DebugShapeRenderer.SetWorld(efeito.World);
                for (int i = 0; i < vertexes.Length; i++)
                {
                    DebugShapeRenderer.AddLine(vertexes[i].Position, 
                        vertexes[i].Position + vertexes[i].Normal, 
                        Color.Red);
                }
            }
            
            

            //Definir os buffers a utilizar
            graphics.SetVertexBuffer(vertexBuffer);
            graphics.Indices = indexBuffer;

            //Ativa o anisotropic filtering
            SamplerState sampler = new SamplerState();
            sampler.Filter = TextureFilter.Anisotropic;
            sampler.MaxAnisotropy = 16;
            graphics.SamplerStates[0] = sampler;

            // Commit the changes to basic effect so it knows you made modifications  
            efeito.CurrentTechnique.Passes[0].Apply();

            //Desenhar o terreno, uma strip de cada vez
            for (int i = 0; i < altura - 1; i++)
            {
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, 
                    vertexes, 
                    i*altura, 
                    altura * 2, 
                    indexes, 
                    0, 
                    altura*2 - 2);
            }
                
        }

    }
}
