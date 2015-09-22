using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcSceneLoader;


namespace AlumnoEjemplos.Los_Borbotones
{
    abstract class Enemy : GameObject
    {
        public float health;
        public float score;
        public float MOVEMENT_SPEED = 50f;
        public float SPAWN_RADIUS= 200f;
        public Matrix posicionActual;
        public Vector3 Normal;
        public float MESH_SCALE;
        Vector3 vectorDireccion;
        Vector3 vectorDireccionRotacion;
        Device d3dDevice = GuiController.Instance.D3dDevice;
        public float SPAWN_HEIGHT = 0;
        public  Matrix giroInicial;
        public TgcBoundingBox HEADSHOT_BOUNDINGBOX;
        public Matrix posicionActualHeadshot;
        public Matrix Traslacion;
        public Matrix MatOrientarObjeto;
        Matrix posicionAnterior;
        Matrix posicionAnteriorHeadshot;
        Vector3 vectorDireccionAnterior;
        Vector3 vectorDireccionRotacionAnterior;

        public override void Init()   
        {
            mesh.AutoTransformEnable = false;

            mesh.Transform = CreatorMatrixPosition();

            mesh.BoundingBox.transform(CreatorMatrixPosition());

            Matrix matt = Matrix.Translation(new Vector3(mesh.Transform.M41, mesh.Transform.M42, mesh.Transform.M43));
            Matrix matScale = Matrix.Scaling(MESH_SCALE, MESH_SCALE, MESH_SCALE);

            this.posicionActual = matScale * giroInicial * matt;

            setBaseEffect();

        }

        public override void Update(float elapsedTime)
        {
            Vector3 vectorPosActual = new Vector3(posicionActual.M41, posicionActual.M42, posicionActual.M43);

            vectorDireccion = (CustomFpsCamera.Instance.Position - vectorPosActual);
            vectorDireccionRotacion = new Vector3(vectorDireccion.X, 0, vectorDireccion.Z);
            vectorDireccionRotacion.Normalize();

            vectorDireccion.Normalize();

            updateMovementMatrix(elapsedTime, vectorDireccion);

            //Colision de enemigos con vegetacion, hecho para que no se queden trabados con o sin "ayuda" del player
            foreach (TgcMesh obstaculo in GameManager.Instance.Vegetation.Meshes)
            {
                TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, obstaculo.BoundingBox);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    posicionActual = posicionAnterior;
                    posicionActualHeadshot = posicionAnteriorHeadshot;
                    vectorDireccionRotacion = vectorDireccionRotacionAnterior;
                    vectorDireccion = vectorDireccionAnterior;
                    Vector3 dirX = new Vector3(vectorDireccion.X, 0, 0);
                    dirX.Normalize();
                    updateMovementMatrix(elapsedTime, dirX);

                    TgcCollisionUtils.BoxBoxResult resultX = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, obstaculo.BoundingBox);
                    if (resultX == TgcCollisionUtils.BoxBoxResult.Adentro || resultX == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        posicionActual = posicionAnterior;
                        posicionActualHeadshot = posicionAnteriorHeadshot;
                        Vector3 dirZ = new Vector3(0, 0, vectorDireccion.Z);
                        dirZ.Normalize();
                        updateMovementMatrix(elapsedTime, dirZ);

                        TgcCollisionUtils.BoxBoxResult resultZ = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, obstaculo.BoundingBox);
                        if (resultZ == TgcCollisionUtils.BoxBoxResult.Adentro || resultZ == TgcCollisionUtils.BoxBoxResult.Atravesando)
                        {
                            posicionActual = posicionAnterior;
                            posicionActual = posicionAnteriorHeadshot;
                        }
                    }
                    break;
                }
            }

            posicionAnterior = posicionActual;
            posicionAnteriorHeadshot = posicionActualHeadshot;
            vectorDireccionRotacionAnterior = vectorDireccionRotacion;
            vectorDireccionAnterior = vectorDireccion;
        }

        private Matrix calcularMatrizOrientacion(Vector3 v)
        {
            Matrix m_mWorld = new Matrix();
            Vector3 n = new Vector3(0, -1, 0);
            Vector3 w = Vector3.Cross(n, v);

            m_mWorld.M11 = v.X;
            m_mWorld.M12 = v.Y;
            m_mWorld.M13 = v.Z;
            m_mWorld.M14 = 0;

            m_mWorld.M21 = 0; 
            m_mWorld.M22 = 1;
            m_mWorld.M23 = 0;
            m_mWorld.M24 = 0;

            m_mWorld.M31 = w.X;
            m_mWorld.M32 = w.Y;
            m_mWorld.M33 = w.Z;
            m_mWorld.M34 = 0;

            m_mWorld.M41 = 0;
            m_mWorld.M42 = 0;
            m_mWorld.M43 = 0;
            m_mWorld.M44 = 1;

            return m_mWorld;
        }
        private Matrix CreatorMatrixPosition()
        {
            Random random = new Random();
            float ANGLE = random.Next(0, 360) / (int)Math.PI;

            Matrix fpsPos = Matrix.Translation(CustomFpsCamera.Instance.Position);

            Matrix radio = Matrix.Translation(this.SPAWN_RADIUS, SPAWN_HEIGHT, 0);

            Matrix escala = Matrix.Scaling(MESH_SCALE, MESH_SCALE, MESH_SCALE);

            Matrix giro = Matrix.RotationY(ANGLE);



            Matrix Resultado = escala * radio * giro * fpsPos;

            Normal = (CustomFpsCamera.Instance.Position - (new Vector3(Resultado.M41, 0, Resultado.M43)));
            // Normal = new Vector3(1, 0, 0);
            Normal.Normalize();
            return Resultado;


        }
        public override void Render(float elapsedTime)
        {

            setBaseEffectValues(elapsedTime);

            this.mesh.render();

            if (GameManager.Instance.drawBoundingBoxes)
            {
                this.mesh.BoundingBox.render();
                this.HEADSHOT_BOUNDINGBOX.render();
            }
            
        }

        public void updateMovementMatrix(float elapsedTime, Vector3 Direccion)
        {
            Vector3 vectorPosActual = new Vector3(posicionActual.M41, posicionActual.M42, posicionActual.M43);
            
            float y;
            GameManager.Instance.interpoledHeight(vectorPosActual.X, vectorPosActual.Z, out y);
            float headOffsetY = posicionActualHeadshot.M42 - posicionActual.M42;

            posicionActual.M42 = y;
            posicionActualHeadshot.M42 = headOffsetY + y;

            MatOrientarObjeto = calcularMatrizOrientacion(vectorDireccionRotacion);

            Traslacion = Matrix.Translation(Direccion * MOVEMENT_SPEED * elapsedTime);

            Matrix transform = MatOrientarObjeto * posicionActual * Traslacion;
            this.mesh.Transform = transform;
           /*
            transform.M11 = 1;
            transform.M12 = 0;
            transform.M13 = 0;
            transform.M21 = 0;
            transform.M22 = 1;
            transform.M23 = 0;
            transform.M31 = 0;
            transform.M32 = 0;
            transform.M33 = 1;
            */
            this.mesh.BoundingBox.transform(transform);

            posicionActual = posicionActual * Traslacion;
            this.HEADSHOT_BOUNDINGBOX.transform(MatOrientarObjeto * posicionActualHeadshot * Traslacion);
            posicionActualHeadshot = posicionActualHeadshot * Traslacion;
        }
        
        public virtual void setBaseEffect()
        {
            mesh.Effect = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Shaders\\enemyBasic.fx");
            mesh.Technique = "HealthDependentShading";
        }

        public virtual void setBaseEffectValues(float elapsedTime)
        {
            mesh.Effect.SetValue("health", this.health);
            mesh.Effect.SetValue("g_time", elapsedTime);
        }

    }
}
