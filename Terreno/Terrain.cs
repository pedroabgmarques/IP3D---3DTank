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
            texels = new Color[heightmap.Width * heightmap.Height];
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
                    //bigHeightmap (512 * 512): 0.2f;
                    //heightmap (128 * 128): 0.05f;
                    float scale = 0.05f;
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

            //Gerar normals
            //Código desenvolvido a partir do livro "XNA 3.0 Game Programming Recipes"
            for (int i = 2; i < indexes.Length; i++)
            {

                VertexPositionNormalTexture verticeI = vertexes[indexes[i]];
                VertexPositionNormalTexture verticeIMenosUm = vertexes[indexes[i - 1]];
                VertexPositionNormalTexture verticeIMenosDois = vertexes[indexes[i - 2]];

                Vector3 vector1 = verticeIMenosUm.Position - verticeI.Position;
                Vector3 vector2 = verticeIMenosDois.Position - verticeI.Position;
                Vector3 normal = Vector3.Cross(vector1, vector2);
                normal.Normalize();

                if (!float.IsNaN(normal.X))
                {
                    vertexes[indexes[i]].Normal = normal;
                    vertexes[indexes[i - 1]].Normal += normal;
                    vertexes[indexes[i - 2]].Normal += normal;
                }
                else
                {
                    vertexes[indexes[i]].Normal = Vector3.Up;
                }
            }

            for (int i = 0; i < vertexes.Length; i++)
            {
                if (vertexes[i].Normal != Vector3.Zero)
                {
                    //Se o normal for Vector3.Zero, não se pode normalizar
                    vertexes[i].Normal.Normalize();
                }
                else
                {
                    //Se o normal for Vector3.Zero, passa a Vector3.Up
                    vertexes[i].Normal = Vector3.Up;
                }
                //Alguns normais ficam invertidos?
                if (vertexes[i].Normal == Vector3.Down) vertexes[i].Normal = Vector3.Up;
                if (vertexes[i].Normal.Y < 0) vertexes[i].Normal.Y = -vertexes[i].Normal.Y;

            }

            //Passar informação para o GPU
            vertexBuffer = new VertexBuffer(graphics, 
                typeof(VertexPositionNormalTexture), vertexes.Length, 
                BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertexes);
            
            indexBuffer = new IndexBuffer(graphics, typeof(int), indexes.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indexes);
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
