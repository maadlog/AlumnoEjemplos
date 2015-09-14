using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Enemy_lvl_2:Enemy
    {
        override
           public void Init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MESH_SCALE = 0.1f;
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\StarWars-Speeder\\StarWars-Speeder-TgcScene.xml");
            this.mesh = scene.Meshes[0];
            SPAWN_HEIGHT = 20f;
            giroInicial = Matrix.RotationY(0);

            base.Init();

            
        }
    }
}
