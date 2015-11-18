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
using System.Drawing;


namespace AlumnoEjemplos.Los_Borbotones
{

    class Enemy_lvl_1 : Enemy
    {

        public TgcSkeletalMesh skeletalMesh;
        public float angulo = 0f;
        public event TgcViewer.Utils.TgcSkeletalAnimation.TgcSkeletalMesh.AnimationEndsHandler AnimationEnd;
        GotaEmitter blood;

        override
            public void Init()
        {
            //seteamos atributos particulares del robot
            health = 100;
            score = 1;
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MESH_SCALE = 0.5f;
            tiempoMuerte = 5f;
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
            
            skeletalMesh.Effect = GameManager.Instance.skeletalEnvMap;
            //skeletalMesh.Technique = "SkeletalEnvMap";

            skeletalMesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.White));
            skeletalMesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(this.getPosicionActual()));
            skeletalMesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(CustomFpsCamera.Instance.getPosition()));
            skeletalMesh.Effect.SetValue("lightIntensity", 20);
            skeletalMesh.Effect.SetValue("lightAttenuation", 0.35f);
            skeletalMesh.Effect.SetValue("reflection", 0.25f);

            //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
            skeletalMesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
            skeletalMesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
            skeletalMesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
            skeletalMesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
            skeletalMesh.Effect.SetValue("materialSpecularExp", 9);

            skeletalMesh.Effect.SetValue("texCubeMap", GameManager.Instance.cubeMap);
            
            //realizamos el init() comun a todos los enemigos
            base.Init();
            //Creamos boundingBox nuevas para la cabeza, pecho y piernas del robot
            HEADSHOT_BOUNDINGBOX = this.mesh.BoundingBox.clone();
            CHEST_BOUNDINGBOX = this.mesh.BoundingBox.clone();
            LEGS_BOUNDINGBOX = this.mesh.BoundingBox.clone();
            Matrix escalabox = Matrix.Scaling(new Vector3(0.43f, 0.3f, 0.43f));
            Matrix traslationbox = Matrix.Translation(new Vector3(0, 90f, 0));
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
            if (muerto)
            {
                Matrix tr = Matrix.Translation(new Vector3(0, 40, 0));
                vectorDireccionRotacion.Normalize();
                Vector3 Direc = Vector3.Cross(this.direccionMuerto, new Vector3(0, 1, 0));
                Direc.Normalize();
                Matrix ro = Matrix.RotationX((float)Math.PI / 2);
                if (tiempoDesdeMuerto <= 1)
                {
                    angulo = angulo + (float)Math.PI / 2 * elapsedTime;
                }
                else { angulo = (float)Math.PI / 2; }
                ro = Matrix.RotationAxis(Direc, angulo);
                Matrix MatOrientarMuer = MatOrientarMuerto * Matrix.RotationY(-(float)Math.PI / 2);
                Matrix po = Matrix.Translation(posicionActual.M41, posicionActual.M42, posicionActual.M43);
                Matrix sc = Matrix.Scaling(MESH_SCALE, MESH_SCALE, MESH_SCALE);
                this.skeletalMesh.Transform = sc * MatOrientarMuer * ro * po;
                this.skeletalMesh.stopAnimation();

            }
        }
        public override void Render(float elapsedTime)
        {
            skeletalMesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(this.getPosicionActual() + this.vectorDireccion * 100));
            skeletalMesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(CustomFpsCamera.Instance.getPosition()));

            //setBaseEffectValues(elapsedTime);
            //renderizamos el skeletalmesh
            if (muerto)
            {
                skeletalMesh.render();
            }
            else { skeletalMesh.animateAndRender(); }

            //se puede habilitar el renderizado de los boundingbox
            if (GameManager.Instance.drawBoundingBoxes)
            {
                this.mesh.BoundingBox.render();
                this.HEADSHOT_BOUNDINGBOX.render();
                this.CHEST_BOUNDINGBOX.render();
                this.LEGS_BOUNDINGBOX.render();
            }

            if (blood != null)
            {
                SystemState_Particulas.Instance.SetRenderState();
                blood.Render(elapsedTime);
                SystemState_Particulas.Instance.SetRenderState_Zero();
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
                GameManager.Instance.player1.recibirAtaque(attackDamage);
                attacked = true;
            }
            else if (!attacking)
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
        public override void sangrar(Vector3 dir, float yOffset)
        {
            int cantidad = 10;//Parametrizable
            Vector3 origen = new Vector3(posicionActual.M41, posicionActual.M42 + yOffset, posicionActual.M43);//Parametrizable
            //float speed = 0f;//Parametrizable
            //float divergence = 0f;//Parametrizable
            //Vector3 velocidad = new Vector3(divergence, speed, divergence);
            dir.Y = 1;
            dir.Normalize();
            Vector3 velocidad = dir;
            Vector3 aceleracion = new Vector3(0, -18.8f, 0);
            float max = 100f, aRRecorrer = yOffset;//Parametrizables
            int alpha = 255;
            float sizeSpeed = 2f;
            blood = new GotaEmitter(cantidad, origen, velocidad, aceleracion, max, aRRecorrer, Color.DarkRed, alpha, 0.02f, 0.005f, sizeSpeed, 100f, 10f);
            blood.Init();
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
