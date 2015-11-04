using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Pasto
    {
        public List<string> texture_pastos = new List<string>();
        public List<TgcPlaneWall> partePasto = new List<TgcPlaneWall>();
        public Matrix m_mRotacion = new Matrix();

        public void crearPasto(Device d3dDevice, int pastoSecuense, Vector3 origen)
        {
            string textureName = "pasto1.png";
            switch(pastoSecuense)
            {
                case 0:
                    textureName = "pasto3.png";
                    break;
                case 1:
                    textureName = "pasto2.png";
                    break;
                case 2:
                    textureName = "pasto1.png";
                    break;
            }

            string texturePath = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Mapas\\Textures\\" + textureName;
            TgcTexture pasto_texture = TgcTexture.createTexture(d3dDevice, texturePath);

            //Crear pared
            TgcPlaneWall pastoWall = new TgcPlaneWall();
            pastoWall.AlphaBlendEnable = true;
            pastoWall.setTexture(pasto_texture);

            TgcPlaneWall.Orientations or = TgcPlaneWall.Orientations.XYplane;

            //Aplicar valores en pared
            pastoWall.Origin = origen;
            pastoWall.Size = new Vector3(36, 27, 36);
            pastoWall.Orientation = or;
            pastoWall.AutoAdjustUv = false;
            pastoWall.UTile = 1;
            pastoWall.VTile = 1;

            partePasto.Add(pastoWall);
        }

        public void renderPasto(float tLeftMoved, float tRightMoved, int parte)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            d3dDevice.RenderState.AlphaTestEnable = true;
            d3dDevice.RenderState.AlphaBlendEnable = true;

            TgcPlaneWall pastoWall = partePasto[parte];
            texturesManager.shaderSet(pastoWall.Effect, "texDiffuseMap", pastoWall.Texture);
            texturesManager.clear(1);
            GuiController.Instance.Shaders.setShaderMatrixIdentity(pastoWall.Effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionTextured;
            pastoWall.Effect.Technique = pastoWall.Technique;

            //Render con shader
            pastoWall.Effect.Begin(0);
            pastoWall.Effect.BeginPass(0);

            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 2, actualizarPasto(tLeftMoved, tRightMoved, parte, pastoWall));

            pastoWall.Effect.EndPass();
            pastoWall.Effect.End();

            d3dDevice.RenderState.AlphaTestEnable = false;
            d3dDevice.RenderState.AlphaBlendEnable = false;
        }

        public void calcularOrientacion(Vector3 direccion, Vector3 centro, Vector3 normal)
        {
            float angle = FastMath.Acos(Vector3.Dot(normal, direccion));
            Vector3 rotationY = Vector3.Cross(normal, direccion);
            rotationY.Normalize();
            Matrix Move = Matrix.Translation(centro);
            Move.Invert();
            m_mRotacion = Move * Matrix.RotationAxis(rotationY, angle) * Matrix.Translation(centro);
        }

        public CustomVertex.PositionTextured[] actualizarPasto(float tLeftMoved, float tRightMoved, int parte, TgcPlaneWall pastoWall)
        {
            float autoWidth;
            float autoHeight;

            //Calcular los 4 corners de la pared
            Vector3 bLeft, tLeft, bRight, tRight, Center;
            bLeft = pastoWall.Origin;
            tLeft = new Vector3(pastoWall.Origin.X + pastoWall.Size.X, pastoWall.Origin.Y, pastoWall.Origin.Z);
            bRight = new Vector3(pastoWall.Origin.X - tLeftMoved, pastoWall.Origin.Y + pastoWall.Size.Y, pastoWall.Origin.Z);
            tRight = new Vector3(pastoWall.Origin.X + pastoWall.Size.X - tRightMoved, pastoWall.Origin.Y + pastoWall.Size.Y, pastoWall.Origin.Z);
            Center = new Vector3(pastoWall.Origin.X + (pastoWall.Size.X / 2), pastoWall.Origin.Y + (pastoWall.Size.Y / 2), pastoWall.Origin.Z);           

            autoWidth = (pastoWall.Size.X / pastoWall.Texture.Width);
            autoHeight = (pastoWall.Size.Y / pastoWall.Texture.Height);

            //Auto ajustar UV
            if (pastoWall.AutoAdjustUv)
            {
                pastoWall.UTile = autoHeight;
                pastoWall.VTile = autoWidth;
            }
            float offsetU = pastoWall.UVOffset.X;
            float offsetV = pastoWall.UVOffset.Y;

            //Rotation
            if (parte == 0)
            {
                /*if (pasto.Origin.X > 0) origen.X = pasto.Origin.X + pasto.Size.X / 2;
                else origen.X = pasto.Origin.X - pasto.Size.X / 2;*/
                Vector3 v1, v2;
                v1 = tLeft - tRight;
                v2 = bLeft - tRight;
                Vector3 normal = Vector3.Cross(v1, v2);
                normal.Normalize();

                Vector3 direccion;
                direccion.X = CustomFpsCamera.Instance.Position.X - Center.X;
                direccion.Y = 0;
                direccion.Z = CustomFpsCamera.Instance.Position.Z - Center.Z;
                direccion.Normalize();

                calcularOrientacion(direccion, Center, normal);
            }

            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[6];
            //Primer triangulo
            vertices[0] = new CustomVertex.PositionTextured(bLeft, offsetU + pastoWall.UTile, offsetV + pastoWall.VTile);
            vertices[1] = new CustomVertex.PositionTextured(tLeft, offsetU, offsetV + pastoWall.VTile);
            vertices[2] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);

            //Segundo triangulo
            vertices[3] = new CustomVertex.PositionTextured(bLeft, offsetU + pastoWall.UTile, offsetV + pastoWall.VTile);
            vertices[4] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);
            vertices[5] = new CustomVertex.PositionTextured(bRight, offsetU + pastoWall.UTile, offsetV);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position = Vector3.TransformCoordinate(vertices[i].Position, m_mRotacion);
            }

            //BoundingBox
            pastoWall.BoundingBox.setExtremes(bLeft, tRight);

            return vertices;
        }

        public void dispose()
        {
            foreach (TgcPlaneWall pastoWall in partePasto)
            {
                pastoWall.dispose();
            }
        }
    }    
}
