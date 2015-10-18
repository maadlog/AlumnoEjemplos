using AlumnoEjemplo.Los_Borbotones;
using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSkeletalAnimation;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Enemy_lvl_3:Enemy_lvl_1
    {
        float prevMovSpeed;

        public override void Init()
        {
            base.Init();
            ATTACK_RANGE = 700f;
            skeletalMesh.setColor(Color.Tomato);
        }

        public override void startAttack()
        {
            prevMovSpeed = MOVEMENT_SPEED;
            MOVEMENT_SPEED = 0;
            skeletalMesh.playAnimation("Arrojar", false, 100);
            attackDelay = ATTACK_DELAY;
            attacking = true;
            Matrix poss = Matrix.Translation(new Vector3(posicionActual.M41, posicionActual.M42, posicionActual.M43));
            GameManager.Instance.dispararProyectil(poss, vectorDireccion);
        }

        protected override void onAnimationEnds(TgcSkeletalMesh mesh)
        {
            if (attacking)
            {
                MOVEMENT_SPEED = prevMovSpeed;
                skeletalMesh.playAnimation("Caminando");
                attacking = false;
                attacked = false;
            }
        }
    }
}
