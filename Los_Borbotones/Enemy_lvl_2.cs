using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Enemy_lvl_2:Enemy
    {
        override
           public void Init()
        {
            health = 150;
            score = 3;
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MESH_SCALE = 0.5f;
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\StarWars-Speeder\\StarWars-Speeder-TgcScene.xml");
            this.mesh = scene.Meshes[0];
            SPAWN_HEIGHT = 20f;
            giroInicial = Matrix.RotationY(0);

            base.Init();

            setBaseEffect();

            HEADSHOT_BOUNDINGBOX = new TgcBoundingBox();
        }
    }
}
