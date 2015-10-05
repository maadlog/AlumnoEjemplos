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
using TgcViewer.Utils.Shaders;


namespace AlumnoEjemplos.Los_Borbotones 
{

    class Enemy_lvl_1:Enemy
    {

        TgcSkeletalMesh skeletalMesh;
        override
            public void Init(){
                health = 100;
                score = 1;
             Device d3dDevice = GuiController.Instance.D3dDevice;
             MESH_SCALE = 0.5f;
             attackDamage = 50;
             TgcSceneLoader loader = new TgcSceneLoader();
             TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Robot\\Robot-TgcScene.xml");
             this.mesh = scene.Meshes[0];
             giroInicial = Matrix.RotationY(-(float)Math.PI / 2);
            //carga de animaciones
             TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
             skeletalMesh = skeletalLoader.loadMeshAndAnimationsFromFile(
                 GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml",
                 new string[] { 
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Caminando-TgcSkeletalAnim.xml",
                   
                });
             skeletalMesh.playAnimation("Caminando", true);
             base.Init();
             HEADSHOT_BOUNDINGBOX = this.mesh.BoundingBox.clone();
             CHEST_BOUNDINGBOX = this.mesh.BoundingBox.clone();
             LEGS_BOUNDINGBOX = this.mesh.BoundingBox.clone();
            Matrix escalabox = Matrix.Scaling(new Vector3(0.43f,0.3f,0.43f));
            Matrix traslationbox = Matrix.Translation(new Vector3(0,90f,0));
            HEADSHOT_BOUNDINGBOX.transform(escalabox * traslationbox);
            posicionActualHeadshot = escalabox * traslationbox * posicionActual;
           Matrix escalabox2 = Matrix.Scaling(new Vector3(0.6f, 0.3f, 0.6f));
            Matrix traslationbox2 = Matrix.Translation(new Vector3(0, 50f, 0));
            CHEST_BOUNDINGBOX.transform(escalabox2 * traslationbox2);
            posicionActualChest = escalabox2 * traslationbox2 * posicionActual;
            Matrix escalabox3 = Matrix.Scaling(new Vector3(0.4f, 0.38f, 0.4f));
            Matrix traslationbox3 = Matrix.Translation(new Vector3(0, 0f, 0));
            LEGS_BOUNDINGBOX.transform(escalabox3 * traslationbox3);
            posicionActualLegs = escalabox3 * traslationbox3 * posicionActual;
            skeletalMesh.AutoTransformEnable = false;

            setBaseEffect();

        }
        override
        public void Update(float elapsedTime)
        {
            base.Update(elapsedTime);
            this.skeletalMesh.Transform = MatOrientarObjeto * posicionActual * Traslacion;
        }
        public override void Render(float elapsedTime)
        {
            setBaseEffectValues(elapsedTime);

            skeletalMesh.animateAndRender();
            if (GameManager.Instance.drawBoundingBoxes)
            {
                this.mesh.BoundingBox.render();
                this.HEADSHOT_BOUNDINGBOX.render();
                this.CHEST_BOUNDINGBOX.render();
                this.LEGS_BOUNDINGBOX.render();
            }
        }

        /*
        public override void setBaseEffect()
        {
            skeletalMesh.Effect = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Shaders\\enemyBasic.fx");
            skeletalMesh.Technique = "HealthDependentShading";
        }

        public override void setBaseEffectValues(float elapsedTime)
        {
            skeletalMesh.Effect.SetValue("health", this.health);
            skeletalMesh.Effect.SetValue("g_time", elapsedTime);
        }
        */

    }
}
