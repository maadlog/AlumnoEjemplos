using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Player1:GameObject
    {
        float WEAPON_ORIENTATION_Y = 0.5f;
        Vector3 WEAPON_OFFSET;

        public override void Init()
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Armas\\Canon\\Canon.max-TgcScene.xml");
            mesh = scene.Meshes[0];
            mesh.Scale = new Vector3(0.5f, 0.5f, 0.5f);

            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            GuiController.Instance.FpsCamera.Enable = true;
            //Configurar posicion y hacia donde se mira
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 20, 0), new Vector3(0, 0, -1000));
            mesh.Position = new Vector3(-5, 5, 0);
            mesh.AutoTransformEnable = false;
        }

        public override void Update(float elapsedTime)
        {
            WEAPON_OFFSET = (Vector3)GuiController.Instance.Modifiers["weaponOffset"];
            WEAPON_ORIENTATION_Y = (float)GuiController.Instance.Modifiers["weaponRotation"];

            mesh.Transform = getMeshTransform(elapsedTime); 
        }

        public override void Render(float elapsedTime)
        {
            mesh.render();
            GuiController.Instance.D3dDevice.Transform.World = Matrix.Identity;
        }

        public Matrix getMeshTransform(float elapsedTime)
        {
            Matrix fpsMatrix = GuiController.Instance.FpsCamera.ViewMatrix;
            Matrix fpsMatrixInv = Matrix.Invert(GuiController.Instance.FpsCamera.ViewMatrix);

            Vector3 lookat = GuiController.Instance.FpsCamera.LookAt;
            Vector3 pos = GuiController.Instance.FpsCamera.Position;
            Vector3 vecResult = lookat - pos;
            vecResult.Normalize();
            float anguloX = Vector3.Dot(vecResult, new Vector3(1,0,0));
            float anguloY = Vector3.Dot(pos, new Vector3(0, 1, 0));

            Matrix origin = Matrix.Translation(new Vector3(0, 0, 0));
            Matrix weaponOffset = Matrix.Translation(WEAPON_OFFSET);
            Matrix weaponScale = Matrix.Scaling(0.5f, 0.5f, 0.5f);
            Matrix weaponRotationY = Matrix.RotationY(WEAPON_ORIENTATION_Y);


            return weaponScale * weaponRotationY * weaponOffset * fpsMatrixInv;
        }
    }
}
