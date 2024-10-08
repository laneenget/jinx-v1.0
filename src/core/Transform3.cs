/* Manages transformations for an object in 3D space. Handles
position, rotation, and scaling of an object and maintains the
relationships between parent and child objects in a trans-
formation hierarchy */

using System;
using System.Collections.Generic;
using Jinx.Src.Math;

namespace Jinx.Src.Core {
    public class Transform3 {

        public List<Transform3> children { get; private set; }

        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 scale; { get; sets; }
        public bool visible { get; set; }

        public bool autoUpdateMatrix { get; set; }
        public Matrix4 matrix { get; private set; }
        public Matrix4 worldMatrix { get; private set; }

        public Vector3 worldPosition { get; private set; }
        public Quaternion worldRotation { get; private set; }
        public Vector3 worldScale { get; private set; }

        public Transform3 parent { get; set; };

        public Transform3() {

            this.children = new List<Transform3>();
            this.position = Vector3.ZERO;
            this.rotation = Quaternion.IDENTITY;
            this.scale = new Vector3(1, 1, 1);
            this.visible = true;

            this.autoUpdateMatrix = true;
            this.matrix = Matrix4.IDENTITY;
            this.worldMatrix = Matrix4.IDENTITY;

            this.worldPosition = Vector3.ZERO;
            this.worldRotation = Quaternion.IDENTITY;
            this.worldScale = Vector3.ONE;

            this.parent = null;
        }

        public void Draw(Transform3 parent, Camera camera, LightManager lightManager) {

            if(!this.visble) {
                return;
            }

            foreach (Transform3 elem in this.children) {
                elem.Draw(this, camera, lightManager);
            }
        }

        public void ComputeWorldTransform() {
            
            if (this.autoUpdateMatrix) {
                this.matrix = Matrix4.CreateScale(this.scale) *
                            Matrix4.CreateFromQuaternion(this.rotation) *
                            Matrix4.CreateTranslation(this.position); 
            }

            if (this.parent != null) {
                this.worldMatrix = this.parent.worldMatrix * this.matrix;
            } else {
                this.worldMatrix = this.matrix;
            }

            Matrix4.Decompose(this.worldMatrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            this.worldPosition = translation;
            this.worldRotation = rotation;
            this.worldScale = scale;

            foreach (Transform3 elem in this.children) {
                elem.ComputeWorldTransform();
            }
        }

        public void Add(Transform3 child) {

            this.children.Add(child);
            child.parent = this;
        }

        public bool Remove() {
            
            if (this.parent == null) {
                return false;
            }

            return this.parent.RemoveChild(this) != null;
        }

        public Transform3 RemoveChild(Transform3 child) {

            int index = this.children.IndexOf(child);

            if (index == -1) {
                return null;
            } else {
                Transform3 removedElement = this.children[index];
                this.children.RemoveAt(index);
                removedElement.parent = null;
                return removedElement;
            }
        }

        public void SetLights(LightManager lightManager) {

            foreach (Transform3 elem in this.children) {
                elem.SetLights(lightManager);
            }
        }

        public void Translate(Vector3 translation) {
            this.position += Vector3.Transform(translation, this.rotation);
        }

        public void TranslateX(float distance) {
            this.position += Vector3.Transform(new Vector3(distance, 0, 0), this.rotation);
        }

        public void TranslateY(float distance) {
            this.position += Vector3.Transform(new Vector3(0, distance, 0), this.rotation);
        }

        public void TranslateZ(float distance) {
            this.position += Vector3.Transform(new Vector3(0, 0, distance), this.rotation);
        }

        public void LookAt(Vector3 target, Vector3 up) {
            Matrix4 rotationMatrix = Matrix4.CreateLookAt(this.position, target, up);
            this.rotation = Quaternion.CreateFromRotationmatrix(rotationMatrix);
        }
    }
}