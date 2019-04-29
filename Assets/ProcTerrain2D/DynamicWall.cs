namespace Assets.Scripts.ProcTerrain2D
{
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    class DynamicWall : MonoBehaviour
    {
        public bool IsUpperWall { get; set; }

        public Transform UpperLeft { get; set; }

        public Transform LowerRight { get; set; }

        public float Clip { get; set; }

        public int SegmentCount {get; set;}

        private List<int> _edgeVectorIndices;

        public void InitMesh()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            _edgeVectorIndices = new List<int>();

            float width = LowerRight.position.x - UpperLeft.position.x;

            for (int i = 0; i < SegmentCount; i++)
            {
                var x = UpperLeft.position.x + (((float)i / SegmentCount) * width);

                if (IsUpperWall)
                {
                    var v = new Vector3(x, Clip);

                    vertices.Add(new Vector3(x, UpperLeft.position.y)); // upper
                    vertices.Add(v); // lower
                    _edgeVectorIndices.Add(vertices.Count - 1);
                }
                else
                {
                    var v = new Vector3(x, -Clip);

                    vertices.Add(v); // upper
                    _edgeVectorIndices.Add(vertices.Count - 1);
                    vertices.Add(new Vector3(x, LowerRight.position.y)); // lower
                }

                normals.Add(Vector3.back);
                normals.Add(Vector3.back);

                if (i == 0) continue;

                var count = vertices.Count;
                triangles.AddRange(new List<int>() { count - 4, count - 3, count - 1 });
                triangles.AddRange(new List<int>() { count - 4, count - 1, count - 2 });
            }

            Mesh m = new Mesh();
            m.vertices = vertices.ToArray();
            m.triangles = triangles.ToArray();
            m.normals = normals.ToArray();

            GetComponent<MeshFilter>().mesh = m;
            RedrawCollider();
        }

        public void UpdateMesh(float verticalOffset)
        {
            var offsets = PerlinSurfaceGenerator.Instance.Offsets;
            var filter = GetComponent<MeshFilter>();
            var vertices = filter.mesh.vertices;

            for (int i = 0; i < offsets.Count; i++)
            {
                float y = (verticalOffset / 2) + offsets[i];

                if (!IsUpperWall) y -= verticalOffset;

                y = Mathf.Clamp(y, -Clip, Clip);

                vertices[_edgeVectorIndices[i]] = new Vector3(vertices[_edgeVectorIndices[i]].x, y);
            }

            filter.mesh.vertices = vertices;
            filter.mesh.UploadMeshData(false);

            RedrawCollider();
        }

        private void RedrawCollider()
        {
            var collider = GetComponent<PolygonCollider2D>();

            var mesh = GetComponent<MeshFilter>().mesh;

            var newPoints = new List<Vector2>();
            if (IsUpperWall)
            {
                newPoints.Add(new Vector2(mesh.vertices[0].x, mesh.vertices[0].y));
            }
            else
            {
                newPoints.Add(new Vector2(mesh.vertices[1].x, mesh.vertices[1].y));
            }

            foreach (var i in _edgeVectorIndices)
            {
                newPoints.Add(new Vector2(mesh.vertices[i].x, mesh.vertices[i].y));
            }

            if (IsUpperWall)
            {
                newPoints.Add(new Vector2(mesh.vertices[mesh.vertexCount - 2].x, mesh.vertices[mesh.vertexCount - 2].y));
            }
            else
            {
                newPoints.Add(new Vector2(mesh.vertices[mesh.vertexCount - 1].x, mesh.vertices[mesh.vertexCount - 1].y));
            }

            collider.points = newPoints.ToArray();
        }
    }
}
