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
        

        override
            public void Init(){
             Device d3dDevice = GuiController.Instance.D3dDevice;
             MESH_SCALE = 0.1f;
             TgcSceneLoader loader = new TgcSceneLoader();
             TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Robot\\Robot-TgcScene.xml");
             this.mesh = scene.Meshes[0];

             base.Init();
            
                   
            }
       
    }
}
