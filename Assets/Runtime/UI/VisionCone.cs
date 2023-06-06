using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Runtime.UI {
    public class VisionCone : MonoBehaviour {
        public string[] tags; // 障碍物Tag

        [Range(0.6f, 4)] public float range = 3; // 视角距离

        [Range(5, 50)] public int samples = 12; // 取样点

        public float maxDegree = 20f, minDegree = -20f; // 视角上下限

        [Range(-10, 20)] public float sightDegree; // 视角动态旋转

        private Coroutine _visionCR; // 视角动态旋转CR

        private void Start() {
            StartRotating();
        }

        public void StartRotating() {
            StopRotating();
            _visionCR = StartCoroutine(VisionCR());
        }

        public void StopRotating() {
            if (_visionCR != null) {
                StopCoroutine(_visionCR);
            }
        }


        #region DrawGizmos

        private void OnDrawGizmos() {
            Gizmos.color = Color.white;
            var mesh = new Mesh();
            var vertices = new Vector3[samples + 1];
            var triangles = new int[samples * 3];
            var position = transform.position;
            vertices[0] = position;
            var x = (maxDegree - minDegree) / samples;
            var y = sightDegree;
            for (var i = 1; i <= samples; i++) {
                var vec = Quaternion.Euler(0, 0, y + (minDegree + x * i)) * Vector3.right * range;
                var hit2D = Physics2D.Raycast(vertices[0], vec);
                if (hit2D.collider) {
                    if (tags.Contains(hit2D.collider.tag) && hit2D.distance < range) vertices[i] = hit2D.point;
                    else vertices[i] = vertices[0] + vec;
                }
                else {
                    vertices[i] = vertices[0] + vec;
                }

                triangles[i * 3 - 3] = 0;
                triangles[i * 3 - 2] = samples + 1 - i;
                triangles[i * 3 - 1] = samples - i;
                Gizmos.DrawLine(vertices[0], vertices[i]);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            Gizmos.DrawMesh(mesh);
        }

        private IEnumerator VisionCR() {
            var epsilon = 0.2f;
            while (true) { // temporarily true for now
                sightDegree += epsilon;
                if (Math.Abs(sightDegree - 20f) < 0.2f || Math.Abs(sightDegree + 10f) < 0.2f) {
                    epsilon = -epsilon;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        #endregion
    }
}