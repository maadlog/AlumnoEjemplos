using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcSkeletalAnimation;


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
             giroInicial = Matrix.RotationY(-(float)Math.PI / 2);
            //carga de animaciones
            /* TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
             personaje = skeletalLoader.loadMeshAndAnimationsFromFile(
                 GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\" + "BasicHuman-TgcSkeletalMesh.xml",
                 new string[] { 
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Walk-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "StandBy-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Jump-TgcSkeletalAnim.xml"
                });*/
             
             base.Init();
             HEADSHOT_BOUNDINGBOX = this.mesh.BoundingBox.clone();
            Matrix escalabox = Matrix.Scaling(new Vector3(0.5f,0.3f,0.5f));
            Matrix traslationbox = Matrix.Translation(new Vector3(0,90f,0));
            HEADSHOT_BOUNDINGBOX.transform(escalabox * traslationbox);
            posicionactualHeadshot = escalabox * traslationbox * posicionActual;
            
                   
            }
        override
        public void Update(float elapsedTime)
        {
            base.Update(elapsedTime);
            
        }
        public override void Render(float elapsedTime)
        {
            base.Render(elapsedTime);
            
        }
    }
}
