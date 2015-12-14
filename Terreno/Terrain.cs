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

        static SamplerState sampler;

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
                    //altura (128 * 128): 0.04f;
                    float scale = 0.04f;
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
            CalcularNormais();

            //Passar informação para o GPU
            vertexBuffer = new VertexBuffer(graphics, 
                typeof(VertexPositionNormalTexture), vertexes.Length, 
                BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertexes);
            
            indexBuffer = new IndexBuffer(graphics, typeof(int), indexes.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indexes);

            //Definir os buffers a utilizar
            graphics.SetVertexBuffer(vertexBuffer);
            graphics.Indices = indexBuffer;

            //Ativa o anisotropic filtering
            sampler = new SamplerState();
            sampler.Filter = TextureFilter.Anisotropic;
            sampler.MaxAnisotropy = 4;
        }

        static private void CalcularNormais()
        {
            //Cria as normais do interior do terreno
            for (int i = altura + 1; i < vertexes.Count() - altura - 1; i++)
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



            //Cria a normal do vértice superior esquerdo
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

            //Cria a normal do vértice superior direito
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

            //Cria a normal do vértice inferior esquerdo
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

            //Cria a normal do vértice inferior direito
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

        static public float getAlturaFromHeightmap(Vector3 position)
        {
            //Posição arredondada para baixo da camara
            int xTank, zTank;
            xTank = (int)position.X;
            zTank = (int)position.Z;

            //Os 4 vértices que rodeiam a posição da camara
            Vector2 pontoA, pontoB, pontoC, pontoD;
            pontoA = new Vector2(xTank, zTank);
            pontoB = new Vector2(xTank + 1, zTank);
            pontoC = new Vector2(xTank, zTank + 1);
            pontoD = new Vector2(xTank + 1, zTank + 1);

            if (position.X > 0 && position.X < Terrain.altura
                        && position.Z > 0 && position.Z < Terrain.altura)
            {

                //Recolher a altura de cada um dos 4 vértices à volta do tanque a partir do heightmap
                float Ya, Yb, Yc, Yd;
                Ya = Terrain.vertexes[(int)pontoA.X * Terrain.altura + (int)pontoA.Y].Position.Y;
                Yb = Terrain.vertexes[(int)pontoB.X * Terrain.altura + (int)pontoB.Y].Position.Y;
                Yc = Terrain.vertexes[(int)pontoC.X * Terrain.altura + (int)pontoC.Y].Position.Y;
                Yd = Terrain.vertexes[(int)pontoD.X * Terrain.altura + (int)pontoD.Y].Position.Y;

                //Interpolação bilenear (dada nas aulas)
                float Yab = (1 - (position.X - pontoA.X)) * Ya + (position.X - pontoA.X) * Yb;
                float Ycd = (1 - (position.X - pontoC.X)) * Yc + (position.X - pontoC.X) * Yd;
                float Y = (1 - (position.Z - pontoA.Y)) * Yab + (position.Z - pontoA.Y) * Ycd;

                //Devolver a altura + um offset
                return Y + 0.01f;
            }
            else
            {
                return -1;
            }
        }

        static public Vector3 getNormalFromHeightmap(Vector3 position)
        {
            //Posição arredondada para baixo da camara
            int xTank, zTank;
            xTank = (int)position.X;
            zTank = (int)position.Z;

            //Os 4 vértices que rodeiam a posição da camara
            Vector2 pontoA, pontoB, pontoC, pontoD;
            pontoA = new Vector2(xTank, zTank);
            pontoB = new Vector2(xTank + 1, zTank);
            pontoC = new Vector2(xTank, zTank + 1);
            pontoD = new Vector2(xTank + 1, zTank + 1);


            Vector3 Ya, Yb, Yc, Yd;
            Ya = Terrain.vertexes[(int)pontoA.X * Terrain.altura + (int)pontoA.Y].Normal;
            Yb = Terrain.vertexes[(int)pontoB.X * Terrain.altura + (int)pontoB.Y].Normal;
            Yc = Terrain.vertexes[(int)pontoC.X * Terrain.altura + (int)pontoC.Y].Normal;
            Yd = Terrain.vertexes[(int)pontoD.X * Terrain.altura + (int)pontoD.Y].Normal;

            //Interpolação bilenear (dada nas aulas)
            Vector3 Yab = (1 - (position.X - pontoA.X)) * Ya + (position.X - pontoA.X) * Yb;
            Vector3 Ycd = (1 - (position.X - pontoC.X)) * Yc + (position.X - pontoC.X) * Yd;
            Vector3 Y = (1 - (position.Z - pontoA.Y)) * Yab + (position.Z - pontoA.Y) * Ycd;

            //Devolver normal
            return Y;
        }

        static public void Draw(GraphicsDevice graphics, BasicEffect efeito)
        {
            //World, View, Projection
            efeito.World = Matrix.Identity; //* Matrix.CreateTranslation(new Vector3(- (float)(altura-1) / 2, -altura/6, - (float)(altura-1) / 2));
            efeito.View = Camera.View;
            efeito.Projection = Camera.Projection;

            DebugShapeRenderer.SetWorld(efeito.World);

            graphics.SamplerStates[0] = sampler;

            //DEBUG
            //Desenhar normais
            
            /*
            if (Camera.drawNormals)
            {
                for (int i = 0; i < vertexes.Length; i++)
                {
                    DebugShapeRenderer.AddLine(vertexes[i].Position, 
                        vertexes[i].Position + vertexes[i].Normal, 
                        Color.Red);
                }
            }
            */
            
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
