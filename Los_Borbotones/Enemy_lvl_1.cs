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
using TgcViewer.Utils.Sound;


namespace AlumnoEjemplo.Los_Borbotones 
{

    class Enemy_lvl_1:Enemy
    {

        public TgcSkeletalMesh skeletalMesh;
        public event TgcViewer.Utils.TgcSkeletalAnimation.TgcSkeletalMesh.AnimationEndsHandler AnimationEnd;
        override
            public void Init(){
            //seteamos atributos particulares del robot
                health = 100;
                score = 1;
             Device d3dDevice = GuiController.Instance.D3dDevice;
             MESH_SCALE = 0.5f;
             
             attackDamage = 25;
            //cargamos el mesh
            //Despues de agregar el skeletalMesh dejamos de renderizar este mesh, pero igual lo utilizamos para calcular muchas cosas
             this.mesh = GameManager.Instance.ModeloRobot.clone("robot");

             giroInicial = Matrix.RotationY(-(float)Math.PI / 2);

            
            //carga de animaciones
             TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();

             skeletalMesh = skeletalLoader.loadMeshAndAnimationsFromFile(
                 GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml",
                 new string[] { 
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Caminando-TgcSkeletalAnim.xml",
                   GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Patear-TgcSkeletalAnim.xml",
                   GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Arrojar-TgcSkeletalAnim.xml",
                });
            
             skeletalMesh.playAnimation("Caminando", true);
             skeletalMesh.AnimationEnds += this.onAnimationEnds;
            //realizamos el init() comun a todos los enemigos
             base.Init();
            //Creamos boundingBox nuevas para la cabeza, pecho y piernas del robot
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

            //carga de sonido
            SonidoMovimiento = new Tgc3dSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio\\Robot\\servomotor.wav", getPosicionActual());
            SonidoMovimiento.MinDistance = 70f;
            SonidoMovimiento.play(true);
            
            
            //setBaseEffect();

        }
        override
        public void Update(float elapsedTime)
        {
            //realizamos los update comunes a todos los enemigos
            base.Update(elapsedTime);
            //actualizamos el skeletalmesh
            this.skeletalMesh.Transform = MatOrientarObjeto * posicionActual * Traslacion;
        }
        public override void Render(float elapsedTime)
        {
            //setBaseEffectValues(elapsedTime);
            //renderizamos el skeletalmesh
            skeletalMesh.animateAndRender();
            //se puede habilitar el renderizado de los boundingbox
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
        public override void dispose()
        {
            skeletalMesh.dispose();
            base.dispose();
        }

        public override void attack(float elapsedTime)
        {
            //Ataque de los robots
            if (attacking && !attacked) 
            {
                GameManager.Instance.player1.recibirAtaque(attackDamage, elapsedTime);
                attacked = true;
            }
            else if(!attacking)
            {
                posicionActual = posicionAnterior;
                posicionActualHeadshot = posicionAnteriorHeadshot;
                posicionActualChest = posicionAnteriorChest;
                posicionActualLegs = posicionAnteriorLegs;
            }
        }

        public override void startAttack()
        {
            MOVEMENT_SPEED *= 3;
            skeletalMesh.playAnimation("Patear", false);
            attackDelay = ATTACK_DELAY;
            attacking = true;
        }

        protected virtual void onAnimationEnds(TgcSkeletalMesh mesh)
        {
            if (attacking)
            {
                MOVEMENT_SPEED = MOVEMENT_SPEED / 3;
                skeletalMesh.playAnimation("Caminando");
                attacking = false;
                attacked = false;
            }
        }
    }
}
