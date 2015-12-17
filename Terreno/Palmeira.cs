using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terreno
{
    public class Palmeira
    {

        Model model;
        Vector3 position;
        public BoundingBox boundingBox;
        Matrix world;

        public Palmeira(Model model, Vector3 position)
        {
            this.model = model;
            this.position = position;
            this.world = Matrix.CreateScale(0.003f) * Matrix.CreateTranslation(position);
            this.boundingBox = CreateBoundingBox(this.model, world);
            
        }

        //https://gamedev.stackexchange.com/questions/2438/how-do-i-create-bounding-boxes-with-xna-4-0
        private BoundingBox CreateBoundingBox(Model model, Matrix worldTransform)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldTransform);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            // Create and return bounding box
            return new BoundingBox(min, max);
        }

        public void Draw(BasicEffect efeito)
        {
            // Draw the model.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                    effect.World = world;
                    effect.View = Camera.View;
                    effect.Projection = Camera.Projection;

                    effect.EnableDefaultLighting();
                    effect.DirectionalLight0.Direction = efeito.DirectionalLight0.Direction;                 
                    effect.DirectionalLight0.Enabled = true;

                }
                mesh.Draw();
            }

            //DEBUG
            //desenha a boundingBox da palmeira
            //DebugShapeRenderer.AddBoundingBox(boundingBox, Color.Red);
        }

    }
}
