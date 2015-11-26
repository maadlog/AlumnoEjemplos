using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Sound;
using System.Drawing;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Enemy_lvl_2:Enemy
    {
        public Explosion explo;
        override
           public void Init()
        {
            //seteamos atributos particulares de las naves
            health = 50;
            score = 2;
            tiempoMuerte = 5f;
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MESH_SCALE = 0.5f;
            attackDamage = 50;
            MOVEMENT_SPEED = 225f;
            //cargamos el mesh
            //las naves no tienen skeletalMesh
            this.mesh = GameManager.Instance.ModeloNave.clone("Nave");


            SPAWN_HEIGHT = 1000f;
            giroInicial = Matrix.RotationY(0);

            //realizamos el init() comun a todos los enemigos
            base.Init();

            mesh.Effect = GameManager.Instance.envMap;
            mesh.Technique = "SimpleEnvironmentMapTechnique";
            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(new Vector3(0,1400,0)));
            mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(CustomFpsCamera.Instance.getPosition()));
            mesh.Effect.SetValue("lightIntensity", 0.3f);
            mesh.Effect.SetValue("lightAttenuation", 1.0f);
            mesh.Effect.SetValue("reflection", 0.65f);

            //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
            mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
            mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialSpecularExp", 9);

            mesh.Effect.SetValue("texCubeMap", GameManager.Instance.cubeMap);

            //creamos las boundingbox
            //a pesar de que las naves no tienen legs ni head, le seteamos boxes "vacias" para no tener problemas con Excepciones de null
            HEADSHOT_BOUNDINGBOX = new TgcBoundingBox();
            CHEST_BOUNDINGBOX = this.mesh.BoundingBox.clone();
            LEGS_BOUNDINGBOX = new TgcBoundingBox();
            //carga de sonido
            SonidoMovimiento = new Tgc3dSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio\\Robot\\ufoHum.wav", getPosicionActual());
            SonidoMovimiento.MinDistance = 130f;
            SonidoMovimiento.play(true);
            
        }
        public override void morirse()
        {
            base.morirse();
            explo = new Explosion();
            explo.posicion = getPosicionActual();
            explo.init();
        }
        public override void Render(float elapsedTime)
        {
           // mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(this.getPosicionActual() + this.vectorDireccion * 100));
            mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(CustomFpsCamera.Instance.getPosition()));

            if (!muerto)
            {
                base.Render(elapsedTime);
            }
        }

        public override void renderParticulas(float elapsedTime)
        {
            if (muerto)
            {
                explo.render(elapsedTime);
            }
        }

        override public void updateMovementMatrix(float elapsedTime, Vector3 Direccion)
        {
            //redefinimos esta funcion para las naves porque no queremos que hagan el nivelado con el piso
            //salvo por eso funciona igual que el updatemovementmatrix de Enemy

            Vector3 vectorPosActual = new Vector3(posicionActual.M41, posicionActual.M42, posicionActual.M43);

            //vectorDireccionRotacion = new Vector3(vectorDireccion.X, vectorDireccion.Y, vectorDireccion.Z);
            //vectorDireccionRotacion.Normalize();

            float y;
            GameManager.Instance.interpoledHeight(vectorPosActual.X, vectorPosActual.Z, out y);
            //float headOffsetY = posicionActualHeadshot.M42 - posicionActual.M42;

            
            if (vectorPosActual.Y < y)
            {
                posicionActual.M42 = y;
            }
            
            //posicionActualHeadshot.M42 = headOffsetY + y;

            MatOrientarObjeto = calcularMatrizOrientacion(vectorDireccionRotacion);

            Traslacion = Matrix.Translation(Direccion * MOVEMENT_SPEED * elapsedTime);

            Matrix transform = MatOrientarObjeto * posicionActual * Traslacion;
            this.mesh.Transform = transform;

            this.mesh.BoundingBox.transform(transform);
            this.CHEST_BOUNDINGBOX.transform(transform);

            posicionActual = posicionActual * Traslacion;
            //this.HEADSHOT_BOUNDINGBOX.transform(MatOrientarObjeto * posicionActualHeadshot * Traslacion);
            //posicionActualHeadshot = posicionActualHeadshot * Traslacion;
        }
    }
}
