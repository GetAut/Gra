﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineManagement.GLLines
{

    public class Line : MonoBehaviour, ILine
    {
        float _defaultZ;
        List<Vector2> _triangleVertices;
        List<Vector2> _vertices;
        List<Vector2> _verticesAdded;
        float _thickness;
        float _newThickness;
        Color _color;
        Color _newColor;
        Material _mat;

        bool shouldRecompute = true;
        bool parentSet = false;

        Transform _parent;

        float _circleDensity = 16;

        public Line()
        {
            _vertices = new List<Vector2>();
        }

        // Use this for initialization
        void Start()
        {
            
            _color = Color.red;
            _thickness = 1f;
            _defaultZ = 0;

            recomputeTriangles();
        }

        void recomputeTriangles()
        {
            _triangleVertices = new List<Vector2>();
            _verticesAdded = new List<Vector2>();

            foreach (var v in _vertices)
            {
                _addVertex(v);
            }

            shouldRecompute = false;
        }

        void _addVertex(Vector2 v)
        {
            _verticesAdded.Add(v);

            if (_verticesAdded.Count == 1)
            {
                return;
            }
            else if (_verticesAdded.Count == 2)
            {
                Vector2 firstP = _verticesAdded[0];
                Vector2 secondP = _verticesAdded[1];

                Vector2 fline = secondP - firstP;
                Vector2 flineN = new Vector2(-fline.y, fline.x).normalized;

                _triangleVertices.Add(firstP + _thickness * flineN);
                _triangleVertices.Add(firstP - _thickness * flineN);
                _triangleVertices.Add(secondP + _thickness * flineN);

                _triangleVertices.Add(firstP - _thickness * flineN);
                _triangleVertices.Add(secondP + _thickness * flineN);
                _triangleVertices.Add(secondP - _thickness * flineN);
            }
            else
            {
                Vector2 p0 = _verticesAdded[_verticesAdded.Count - 3];
                Vector2 p1 = _verticesAdded[_verticesAdded.Count - 2];
                Vector2 p2 = _verticesAdded[_verticesAdded.Count - 1];

                Vector2 M = calculateM(p0, p1, p2);

                Vector2 m1 = p1 + M;
                Vector2 m2 = p1 - M;

                int length = _triangleVertices.Count;

                _triangleVertices[length - 1] = m2;
                _triangleVertices[length - 2] = m1;
                _triangleVertices[length - 4] = m1;


                Vector2 line2 = (p2 - p1).normalized;
                Vector2 line2N = new Vector2(-line2.y, line2.x);

                _triangleVertices.Add(m1);
                _triangleVertices.Add(m2);
                _triangleVertices.Add(p2 + _thickness * line2N);

                _triangleVertices.Add(m2);
                _triangleVertices.Add(p2 - _thickness * line2N);
                _triangleVertices.Add(p2 + _thickness * line2N);
            }

            if (_verticesAdded.Count > 3 && v == _verticesAdded[0])
            {
                Vector2 p0 = _verticesAdded[1];
                Vector2 p1 = _verticesAdded[0];
                Vector2 p2 = _verticesAdded[_verticesAdded.Count - 2];

                Vector2 M = calculateM(p0, p1, p2);

                Vector2 m1 = p1 + M;
                Vector2 m2 = p1 - M;

                _triangleVertices[0] = m2;
                _triangleVertices[1] = m1;
                _triangleVertices[3] = m1;

                int length = _triangleVertices.Count;

                _triangleVertices[length - 1] = m1;
                _triangleVertices[length - 2] = m2;
                _triangleVertices[length - 4] = m2;
            }
        }

        Vector2 calculateM(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            Vector2 line = p1 - p0;
            Vector2 lineN = new Vector2(-line.y, line.x).normalized;

            Vector2 line2 = p2 - p1;
            //Vector2 line2N = new Vector2(-line2.y, line2.x).normalized;

            Vector2 t = (line.normalized + line2.normalized).normalized;

            Vector2 m = new Vector2(-t.y, t.x).normalized;

            float mLength = _thickness / (Vector2.Dot(m, lineN));

            return mLength * m.normalized;
        }

        // Update is called once per frame
        void Update()
        {
            if (shouldRecompute)
            {
                recomputeTriangles();
            }

            if (!parentSet && _parent)
            {
                transform.parent = _parent;
                parentSet = true;
            }

            UpdateThickness();
            UpdateColor();
            UpdateVertices();

        }

        void UpdateThickness()
        {
            if ((_newThickness != null) && _thickness != _newThickness)
            {
                _thickness = _newThickness;
                recomputeTriangles();
            }
        }

        void UpdateColor()
        {
            if ((_newColor != null) && _color != _newColor)
            {
                _color = _newColor;
            }
        }

        void UpdateVertices()
        {
            while (_verticesAdded.Count < _vertices.Count)
            {
                _addVertex(_vertices[_verticesAdded.Count]);
            }

        }

        public void OnRenderObject()
        {
            UpdateThickness();
            UpdateColor();
            

            GraphicsProvider.DrawTriangles(transform, _color, _defaultZ, _triangleVertices);

            if (_verticesAdded.Count > 0 && (_verticesAdded[0] != _verticesAdded[_verticesAdded.Count - 1] || _verticesAdded.Count == 1))
            {
                _drawCircles();
            }

            GL.PopMatrix();
        }

        void _drawCircles()
        {
            if (_verticesAdded.Count == 0)
                return;

            if (_verticesAdded.Count < 2)
            {
                Vector2 p = _verticesAdded[0];
                Vector2 v = new Vector2(0f, _thickness);

                _drawCircle(p, v, 1f);
            }
            else
            {
                Vector2 p0 = _verticesAdded[0];
                Vector2 p1 = _verticesAdded[1];

                Vector2 line = (p0 - p1).normalized;
                Vector2 n = new Vector2(-line.y, line.x).normalized;

                Vector2 p = p0;
                Vector2 v = n * _thickness;

                _drawCircle(p, v, 0.5f, -1);

                p0 = _verticesAdded[_verticesAdded.Count - 1];
                p1 = _verticesAdded[_verticesAdded.Count - 2];

                line = (p0 - p1).normalized;
                n = new Vector2(-line.y, line.x).normalized;

                p = p0;
                v = n * _thickness;

                _drawCircle(p, v, 0.5f, -1);
            }
        }

        void _drawCircle(Vector2 p, Vector2 r, float part, float direction = 1f)
        {
            Vector2 prevP = p + r;

            GL.Begin(GL.TRIANGLES);
            GL.Color(_color);

            for (float angle = 0f; angle <= (2f * Mathf.PI * part); angle += (Mathf.PI / _circleDensity))
            {
                float a = Mathf.Sign(direction) * (angle + (Mathf.PI / _circleDensity));

                Vector2 newV = new Vector2(r.x * Mathf.Cos(a) - r.y * Mathf.Sin(a),
                                            r.x * Mathf.Sin(a) + r.y * Mathf.Cos(a));
                Vector2 newP = p + newV;

                GL.Vertex3(p.x, p.y, _defaultZ);
                GL.Vertex3(prevP.x, prevP.y, _defaultZ);
                GL.Vertex3(newP.x, newP.y, _defaultZ);

                prevP = newP;
            }
            GL.End();
        }

        /*Color nextColor()
        {
            curColor = (curColor + 1) % colors.Count;
            return colors[curColor];
        }*/

        public string SortingLayer
        {
            get
            {
                return ""; //do smth
            }
            set
            {
                //to smthg
            }
        }

        public void Init(Transform canvas)
        {
            _parent = canvas;
        }

        public int VertexCount
        {
            get { return _vertices.Count; }
        }

        public Color Color
        {
            get { return _color; }
        }

        public void SetColor(Color color)
        {
            _newColor = color;
        }

        public void SetSize(float size)
        {
            _newThickness = size / 2;
        }

        public void AddVertex(Vector2 pos)
        {
            _vertices.Add(pos);

            //_addVertex(pos);
        }

        public void AddVertex(Vector3 pos)
        {
            AddVertex(new Vector2(pos.x, pos.y));
        }

        public List<Vector3> GetVerticles()
        {
            var l = new List<Vector3>();

            foreach (var v in _vertices)
            {
                l.Add(new Vector3(v.x, v.y, _defaultZ));
            }

            return l;
        }


        public List<Vector2> GetVertices2()
        {
            return _vertices;
        }
    }

    static class GraphicsProvider
    {
        static Material _mat;
        public static GraphicsProvider()
        {
            _mat = new Material(Shader.Find("Sprites/Default"));
        }

        public static void DrawTriangles(Transform transform, Color color, float z, List<Vector2> triangleVertices)
        {
            _mat.SetPass(0);

            GL.PushMatrix();

            GL.MultMatrix(transform.localToWorldMatrix);

            GL.Begin(GL.TRIANGLES);

            GL.Color(color);

            for (int i = 0; i < triangleVertices.Count - 2; i += 3)
            {
                //GL.Color(nextColor());

                for (int j = 0; j < 3; ++j)
                {
                    Vector2 v = triangleVertices[i + j];

                    GL.Vertex3(v.x, v.y, z);
                }
            }

            GL.End();
        }
    }


    public static class CameraExtensions
    {
        public static Bounds OrthographicBounds(this Camera camera)
        {
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float cameraHeight = camera.orthographicSize * 2;
            Bounds bounds = new Bounds(
                camera.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }
    }

}