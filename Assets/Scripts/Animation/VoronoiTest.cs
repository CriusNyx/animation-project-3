using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using UnityEngine;

public class VoronoiTest : MonoBehaviour
{
    List<VoronoiPoint> list = new List<VoronoiPoint>();
    List<(Vector3 a, Vector3 b)> linesLastFrame;

    void Start()
    {
        var def = new RepeatSpawn(SpawnPattern.InterleavedFan(30f, 12, 0.1f, SpawnPattern.Single(0f)), 20, 0.2f);
        foreach(var spawn in def.GetSpawnPattern())
        {
            Vector3 offset = spawn.offset;
            Vector3 direction = Quaternion.Euler(0f, 0f, spawn.unitAngle) * Vector3.right;
            Vector3 pos = new Vector3(offset.x, offset.y) + direction * spawn.delay;

            list.Add(new VoronoiPoint(pos, direction, 0f, 0f));
        }
        //for (int i = 0; i < 1000; i++)
        //{
        //    float theta = Random.value * 360f;
        //    float r = Mathf.Pow(Random.value, 2f) * 10f;

        //    float x = Random.value * 10000f;
        //    float y = Random.value * 10000f;

        //    Vector2 pos = Quaternion.Euler(0f, 0f, theta) * Vector3.right * r;

        //    list.Add(new VoronoiPoint(pos, x, y));
        //}
    }

    private void FixedUpdate()
    {
        var polygon = new TriangleNet.Geometry.Polygon();

        linesLastFrame = new List<(Vector3 a, Vector3 b)>();

        foreach (var point in list)
        {
            polygon.Add(new TriangleNet.Geometry.Vertex(point.position.x, point.position.y));
            linesLastFrame.Add((point.position, point.position + Vector3.back * 0.1f));
        }

        var mesh = polygon.Triangulate(new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true }) as TriangleNet.Mesh;

        var voronoi = new TriangleNet.Voronoi.StandardVoronoi(mesh);

        foreach (var e in voronoi.Edges)
        {
            var origin = voronoi.Vertices[e.P0];
            var nextOrigin = voronoi.Vertices[e.P1];
            Vector2 a = new Vector2((float)origin.x, (float)origin.y);
            Vector2 b = new Vector2((float)nextOrigin.x, (float)nextOrigin.y);
            linesLastFrame.Add((a, b));
        }

        foreach(var point in list)
        {
            point.UpdatePosition();
        }
    }

    private void Update()
    {
        foreach (var (a, b) in linesLastFrame)
        {
            Debug.DrawLine(a, b);
        }
    }

    private class VoronoiPoint
    {
        private static float PMass = 10f;

        public Vector3 position;
        public Vector3 velocity;
        public float voronoiSeedX;
        public float voronoiSeedY;

        public VoronoiPoint(Vector3 position, Vector3 velocity, float voronoiSeedX, float voronoiSeedY)
        {
            this.position = position;
            this.velocity = velocity;
            this.voronoiSeedX = voronoiSeedX;
            this.voronoiSeedY = voronoiSeedY;
        }

        public void UpdatePosition()
        {
            //Vector3 acceleration = -position.normalized * PMass / position.sqrMagnitude;
            //velocity += acceleration * Time.fixedDeltaTime;
            position = Quaternion.Euler(0f, 0f, 10f * Time.deltaTime * position.sqrMagnitude + 10f * Time.deltaTime) * position;
        }
    }
}