using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;


namespace AlumnoEjemplos.Los_Borbotones 
{
    class Enemy_lvl_1:Enemy
    {
        float MESH_SCALE = 0.1f;

        override
            public void Init(){
             Device d3dDevice = GuiController.Instance.D3dDevice;
             this.SPAWN_RADIUS = 200f;
             TgcSceneLoader loader = new TgcSceneLoader();
             TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Robot\\Robot-TgcScene.xml");
             this.mesh = scene.Meshes[0];

             mesh.AutoTransformEnable = false;

             mesh.Transform = CreatorMatrixPosition();

             mesh.BoundingBox.transform(CreatorMatrixPosition());

            Matrix matt= Matrix.Translation(new Vector3(mesh.Transform.M41,mesh.Transform.M42,mesh.Transform.M43));
            Matrix matScale = Matrix.Scaling(MESH_SCALE, MESH_SCALE, MESH_SCALE);

            Matrix giroInicial = Matrix.RotationY(-(float)Math.PI / 2);
             this.posicionActual = matScale* giroInicial * matt;
                   
            }
        private Matrix CreatorMatrixPosition()
        {
            Random random = new Random();
            this.ANGLE = random.Next(0, 360) / (int)Math.PI;

            Matrix fpsPos = Matrix.Translation(CustomFpsCamera.Instance.Position);

            Matrix radio = Matrix.Translation(this.SPAWN_RADIUS, 0, 0);

            Matrix escala = Matrix.Scaling(MESH_SCALE, MESH_SCALE, MESH_SCALE);

            Matrix giro = Matrix.RotationY(ANGLE);

            

            Matrix Resultado = escala * radio * giro * fpsPos;

            Normal = (CustomFpsCamera.Instance.Position - (new Vector3(Resultado.M41, 0, Resultado.M43)));
           // Normal = new Vector3(1, 0, 0);
            Normal.Normalize();
            return  Resultado;


        }
    }
}
