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
    public class MuzzleFlash
    {
        public List<string> textures_flash = new List<string>();
        public TgcPlaneWall muzzleFlash;
        Vector3 BLoffset, TLoffset, BRoffset, TRoffset;

        public void crearMuzzle()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            textures_flash.Add("flash1.png");
            textures_flash.Add("flash2.png");
            textures_flash.Add("flash3.png");

            string texturePath = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\" + textures_flash[0];
            TgcTexture pasto_texture = TgcTexture.createTexture(d3dDevice, texturePath);

            //Crear pared
            muzzleFlash = new TgcPlaneWall();
            muzzleFlash.AlphaBlendEnable = true;
            muzzleFlash.setTexture(pasto_texture);

            //Aplicar valores en pared
            //muzzleFlash.Origin = new Vector3(-50, 917.8f, 4.1f);
            muzzleFlash.Size = new Vector3(14, 14, 14);
            muzzleFlash.AutoAdjustUv = false;
            muzzleFlash.UTile = 1;
            muzzleFlash.VTile = 1;

            //Offsets
            BLoffset = new Vector3(-100, -17.7f, 14.7f);
            TLoffset = new Vector3(-100, -17.7f, 14.7f + muzzleFlash.Size.Z);
            BRoffset = new Vector3(-100, -17.7f + muzzleFlash.Size.Y, 14.7f);
            TRoffset = new Vector3(-100, -17.7f + muzzleFlash.Size.Y, 14.7f + muzzleFlash.Size.Z);
        }

        public void renderFlash()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            d3dDevice.RenderState.AlphaTestEnable = true;
            d3dDevice.RenderState.AlphaBlendEnable = true;

            texturesManager.shaderSet(muzzleFlash.Effect, "texDiffuseMap", muzzleFlash.Texture);
            texturesManager.clear(1);
            GuiController.Instance.Shaders.setShaderMatrixIdentity(muzzleFlash.Effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionTextured;
            muzzleFlash.Effect.Technique = muzzleFlash.Technique;

            //Render con shader
            muzzleFlash.Effect.Begin(0);
            muzzleFlash.Effect.BeginPass(0);

            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 2, actualizarFlash());

            muzzleFlash.Effect.EndPass();
            muzzleFlash.Effect.End();

            d3dDevice.RenderState.AlphaTestEnable = false;
            d3dDevice.RenderState.AlphaBlendEnable = false;
        }

        public Matrix calcularOrientacion(Vector3 WEAPON_OFFSET)
        {

            return Matrix.Translation(WEAPON_OFFSET);
        }

        public CustomVertex.PositionTextured[] actualizarFlash()
        {
            float autoWidth;
            float autoHeight;
            Vector3 origen = CustomFpsCamera.Instance.Position;
            origen.Y -= muzzleFlash.Size.Y / 2;
            origen.Z -= muzzleFlash.Size.Z / 2;
            muzzleFlash.Origin = origen;
            
            //Calcular los 4 corners de la pared
            Vector3 bLeft, tLeft, bRight, tRight;
            bLeft = muzzleFlash.Origin;
            tLeft = new Vector3(muzzleFlash.Origin.X, muzzleFlash.Origin.Y, muzzleFlash.Origin.Z + muzzleFlash.Size.Z);
            bRight = new Vector3(muzzleFlash.Origin.X, muzzleFlash.Origin.Y + muzzleFlash.Size.Y, muzzleFlash.Origin.Z);
            tRight = new Vector3(muzzleFlash.Origin.X, muzzleFlash.Origin.Y + muzzleFlash.Size.Y, muzzleFlash.Origin.Z + muzzleFlash.Size.Z);                                   
            
            autoWidth = (muzzleFlash.Size.Y / muzzleFlash.Texture.Width);
            autoHeight = (muzzleFlash.Size.Z / muzzleFlash.Texture.Height);

            //Auto ajustar UV
            if (muzzleFlash.AutoAdjustUv)
            {
                muzzleFlash.UTile = autoHeight;
                muzzleFlash.VTile = autoWidth;
            }
            float offsetU = muzzleFlash.UVOffset.X;
            float offsetV = muzzleFlash.UVOffset.Y;
            
            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[6];
            //Primer triangulo
            vertices[0] = new CustomVertex.PositionTextured(bLeft, offsetU + muzzleFlash.UTile, offsetV + muzzleFlash.VTile);
            vertices[1] = new CustomVertex.PositionTextured(tLeft, offsetU, offsetV + muzzleFlash.VTile);
            vertices[2] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);

            //Segundo triangulo
            vertices[3] = new CustomVertex.PositionTextured(bLeft, offsetU + muzzleFlash.UTile, offsetV + muzzleFlash.VTile);
            vertices[4] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);
            vertices[5] = new CustomVertex.PositionTextured(bRight, offsetU + muzzleFlash.UTile, offsetV);

            //Transformar triangulos
            vertices[0].Position = Vector3.TransformCoordinate(vertices[0].Position, calcularOrientacion(BLoffset));
            vertices[1].Position = Vector3.TransformCoordinate(vertices[1].Position, calcularOrientacion(TLoffset));
            vertices[2].Position = Vector3.TransformCoordinate(vertices[2].Position, calcularOrientacion(TRoffset));
            vertices[3].Position = Vector3.TransformCoordinate(vertices[3].Position, calcularOrientacion(BLoffset));
            vertices[4].Position = Vector3.TransformCoordinate(vertices[4].Position, calcularOrientacion(TRoffset));
            vertices[5].Position = Vector3.TransformCoordinate(vertices[5].Position, calcularOrientacion(BRoffset));
            

            //BoundingBox
            muzzleFlash.BoundingBox.setExtremes(bLeft, tRight);

            return vertices;
        }

        public void dispose()
        {
            muzzleFlash.dispose();
        }
    }
}