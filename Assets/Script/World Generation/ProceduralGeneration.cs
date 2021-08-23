using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DeadReckoning.Procedural
{
    [System.Serializable]
    public class ProceduralSettings
    {
        public Color pineTrunkColor = Color.Lerp(Color.green, Color.red, 0.5f);

        public float pineTrunkWidth = 0.25f;
        public float pineTrunkHeight = 0.25f;
        public float pineLayerReductionRate = 0.75f;
        public float pineLayerWidth = 0.25f;
        public float pineLayerHeight = 0.25f;
        public float pineHeightVariation = 0.05f;
        public float pineWidthtVariation = 0.05f;

    }

    public class ProceduralGeneration
    {
        #region Relative Movement & Axes
        public static Vector3[] FindRelativeAxes(Vector3 v)
        {
            Vector3 forward = v.normalized;

            Vector3 up;

            if (forward == Vector3.forward || forward == Vector3.right)
            {
                up = Vector3.up;
            }
            else if (forward == Vector3.back || forward == Vector3.left)
            {
                up = Vector3.down;
            }
            else if (forward == Vector3.down)
            {
                up = Vector3.right;
            }
            else if (forward == Vector3.up)
            {
                up = Vector3.left;
            }
            else
            {
                up = Vector3.Project(Vector3.up, forward);
                up -= Vector3.up;
                up = up.normalized;
            }

            Vector3 right = -Vector3.Cross(forward, up).normalized;

            return new Vector3[] { forward, up, right };
        }

        public static Vector3 RelativeMovement(Vector3 p, float xMod, float yMod, float zMod, Vector3 forward, Vector3 up, Vector3 right)
        {
            return new Vector3(((p.x * xMod) * right.x) + ((p.y * yMod) * up.x) + ((p.z * zMod) * forward.x), ((p.x * xMod) * right.y) + ((p.y * yMod) * up.y) + ((p.z * zMod) * forward.y), ((p.x * xMod) * right.z) + ((p.y * yMod) * up.z) + ((p.z * zMod) * forward.z));
        }
        #endregion

        public class ProceduralObject
        {
            public Vector3[] vertices;
            public int[] triangles;
            public Color[] colors;
            public Vector2[] uv3;
        }

        public class Tree : ProceduralObject
        {
            public enum TreeType { pine, oak, cathedralFig }

            public Tree (ProceduralSettings settings, Vector3 stumpBase, TreeType type, Color canopyColor)
            {
                if (type == TreeType.pine)
                {
                    GeneratePine(settings, stumpBase, canopyColor);
                }
            }

            public void GeneratePine(ProceduralSettings settings, Vector3 stumpBase, Color canopyColor)
            {
                // Each tree will be made up of a base cube...
                // and three pyramids sitting on top of one another.
                // Cube: 8 verts, 12 triangles.
                // Pyramid: 5 verts, 6 triangles.
                // Together: 23 verts, 30 triangles.

                Vector3[] relativeAxes = FindRelativeAxes(stumpBase);

                Vector3 forward = relativeAxes[0];
                Vector3 up = relativeAxes[1];
                Vector3 right = relativeAxes[2];

                float widthVariance = UnityEngine.Random.Range(1, 1 + settings.pineWidthtVariation);
                float heightVariance = UnityEngine.Random.Range(1, 1 + settings.pineHeightVariation);

                float trunkWidth = settings.pineTrunkWidth;
                float trunkHeight = settings.pineTrunkHeight;
                float layerWidth = settings.pineLayerWidth; // * widthVariance;
                float layerHeight = settings.pineLayerHeight; // * heightVariance;
                float reducRate = settings.pineLayerReductionRate;

                vertices = new Vector3[]
                {
                    // Trunk Base
                    stumpBase + RelativeMovement(pineVertices[0], trunkWidth, trunkWidth, 1, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[1], trunkWidth, trunkWidth, 1, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[2], trunkWidth, trunkWidth, 1, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[3], trunkWidth, trunkWidth, 1, forward, up, right), // D

                    // Trunk Top
                    stumpBase + RelativeMovement(pineVertices[4], trunkWidth, trunkWidth, trunkHeight, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[5], trunkWidth, trunkWidth, trunkHeight, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[6], trunkWidth, trunkWidth, trunkHeight, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[7], trunkWidth, trunkWidth, trunkHeight, forward, up, right), // D

                    // First Pyramid
                    stumpBase + RelativeMovement(pineVertices[8], layerWidth, layerWidth, trunkHeight, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[9], layerWidth, layerWidth, trunkHeight, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[10], layerWidth, layerWidth, trunkHeight, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[11], layerWidth, layerWidth, trunkHeight, forward, up, right), // D
                    stumpBase + RelativeMovement(pineVertices[12], layerWidth, layerWidth, trunkHeight, forward, up, right), // E
                    
                    // Second Pyramid
                    stumpBase + RelativeMovement(pineVertices[13], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[14], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[15], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[16], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // D
                    stumpBase + RelativeMovement(pineVertices[17], layerWidth * reducRate, layerWidth * reducRate, layerHeight * 2, forward, up, right), // E
                    
                    // Third Pyramid
                    stumpBase + RelativeMovement(pineVertices[18], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right), // A
                    stumpBase + RelativeMovement(pineVertices[19], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right), // B
                    stumpBase + RelativeMovement(pineVertices[20], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right), // C
                    stumpBase + RelativeMovement(pineVertices[21], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right), // D
                    stumpBase + RelativeMovement(pineVertices[22], layerWidth * reducRate * reducRate, layerWidth * reducRate * reducRate, layerHeight * 3, forward, up, right)  // E

                };

                triangles = new int[]
                {
                    0, 1, 2, // Base of trunk
                    2, 1, 3,

                    1, 5, 0, // First trunk side
                    0, 5, 4,

                    3, 7, 1, // Second trunk side
                    1, 7, 5,

                    3, 2, 7, // Third trunk side
                    7, 2, 6,

                    2, 0, 6, // Fourth trunk side
                    6, 0, 4,

                    4, 5, 6, // Top of trunk
                    6, 5, 7,

                    8, 9, 10, // Base of first pyramid
                    10, 9, 11,

                    8, 12, 9, // First pyramid sides
                    9, 12, 11,
                    11, 12, 10,
                    10, 12, 8,

                    13, 14, 15, // Base of second pyramid
                    15, 14, 16,

                    13, 17, 14, // Second pyramid sides
                    14, 17, 16,
                    16, 17, 15,
                    15, 17, 13,

                    18, 19, 20, // Base of third pyramid
                    20, 19, 21,

                    18, 22, 19, // Third pyramid sides
                    19, 22, 21,
                    21, 22, 20,
                    20, 22, 18
                };

                colors = new Color[vertices.Length];

                for (int i = 0; i < 8; i++)
                {
                    colors[i] = settings.pineTrunkColor;
                }
                for (int i = 8; i < 23; i++)
                {
                    colors[i] = canopyColor;
                }
            }

            public static Vector3[] pineVertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0f), // Base of trunk.
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),

                new Vector3(-0.5f, -0.5f, 1f), // Top of trunk.
                new Vector3(-0.5f, 0.5f, 1f),
                new Vector3(0.5f, -0.5f, 1f),
                new Vector3(0.5f, -0.5f, 1f),

                new Vector3(-0.5f, -0.5f, 1f), // First pyramid.
                new Vector3(-0.5f, 0.5f, 1f),
                new Vector3(0.5f, -0.5f, 1f),
                new Vector3(0.5f, 0.5f, 1f),
                new Vector3(0, 0, 5), // tippy top #1

                new Vector3(-0.5f, -0.5f, 2f), // Second pyramid.
                new Vector3(-0.5f, 0.5f, 2f),
                new Vector3(0.5f, -0.5f, 2f),
                new Vector3(0.5f, 0.5f, 2f),
                new Vector3(0, 0, 5), // tippy top #2
                
                new Vector3(-0.5f, -0.5f, 3f), // Third pyramid.
                new Vector3(-0.5f, 0.5f, 3f),
                new Vector3(0.5f, -0.5f, 3f),
                new Vector3(0.5f, 0.5f, 3f),
                new Vector3(0, 0, 5), // tippy top #3
            };
        }

        public class RockStructure : ProceduralObject
        {
            public RockStructure(Vector3 center, Vector3[] basePoints, float height)
            {
                // We take the set of base points, create a ton of points between them, triangulate those points, then inflate them up along the y-axis...
                // depending on how close they are to the center, in order to make an irregular peak-like shape.
                
                List<Vector3> verts = new List<Vector3>(basePoints);
                List<int> tris = new List<int>();
                List<Vector2> uvs = new List<Vector2>();

                Vector3 vB = basePoints[0];
                Vector3 vC = basePoints[1];
                Vector3 vD = basePoints[2];
                Vector3 vE = basePoints[3];
                Vector3 vF = basePoints[4];
                Vector3 vG = basePoints[5];

                verts.Add(vB);
                verts.Add(vC);
                verts.Add(vD);
                verts.Add(vE);
                verts.Add(vF);
                verts.Add(vG);

                Vector3 vAP = center;
                Vector3 vBP = Vector3.Lerp(vB, vAP, Random.Range(0, 0.9f));
                Vector3 vCP = Vector3.Lerp(vC, vAP, Random.Range(0, 0.9f));
                Vector3 vDP = Vector3.Lerp(vD, vAP, Random.Range(0, 0.9f));
                Vector3 vEP = Vector3.Lerp(vE, vAP, Random.Range(0, 0.9f));
                Vector3 vFP = Vector3.Lerp(vF, vAP, Random.Range(0, 0.9f));
                Vector3 vGP = Vector3.Lerp(vG, vAP, Random.Range(0, 0.9f));

                verts.Add(vAP);
                verts.Add(vBP);
                verts.Add(vCP);
                verts.Add(vDP);
                verts.Add(vEP);
                verts.Add(vFP);
                verts.Add(vGP);

                #region Triangles x1
                // Triangle One
                tris.Add(verts.IndexOf(vC));
                tris.Add(verts.IndexOf(vB));
                tris.Add(verts.IndexOf(vCP));

                // Triangle Two
                tris.Add(verts.IndexOf(vB));
                tris.Add(verts.IndexOf(vBP));
                tris.Add(verts.IndexOf(vCP));

                // Triangle Three
                tris.Add(verts.IndexOf(vD));
                tris.Add(verts.IndexOf(vC));
                tris.Add(verts.IndexOf(vDP));

                // Triangle Four
                tris.Add(verts.IndexOf(vC));
                tris.Add(verts.IndexOf(vCP));
                tris.Add(verts.IndexOf(vDP));

                // Triangle Five
                tris.Add(verts.IndexOf(vE));
                tris.Add(verts.IndexOf(vD));
                tris.Add(verts.IndexOf(vEP));

                // Triangle Six
                tris.Add(verts.IndexOf(vD));
                tris.Add(verts.IndexOf(vDP));
                tris.Add(verts.IndexOf(vEP));

                // Triangle Seven
                tris.Add(verts.IndexOf(vF));
                tris.Add(verts.IndexOf(vE));
                tris.Add(verts.IndexOf(vFP));

                // Triangle Eight
                tris.Add(verts.IndexOf(vE));
                tris.Add(verts.IndexOf(vEP));
                tris.Add(verts.IndexOf(vFP));

                // Triangle Nine
                tris.Add(verts.IndexOf(vG));
                tris.Add(verts.IndexOf(vF));
                tris.Add(verts.IndexOf(vGP));

                // Triangle Ten
                tris.Add(verts.IndexOf(vF));
                tris.Add(verts.IndexOf(vFP));
                tris.Add(verts.IndexOf(vGP));

                // Triangle Eleven
                tris.Add(verts.IndexOf(vB));
                tris.Add(verts.IndexOf(vG));
                tris.Add(verts.IndexOf(vBP));

                // Triangle Twelve
                tris.Add(verts.IndexOf(vG));
                tris.Add(verts.IndexOf(vGP));
                tris.Add(verts.IndexOf(vBP));
                #endregion

                #region Triangles x2
                // Peak Triangle One
                tris.Add(verts.IndexOf(vBP));
                tris.Add(verts.IndexOf(vAP));
                tris.Add(verts.IndexOf(vCP));

                // Peak Triangle Two
                tris.Add(verts.IndexOf(vCP));
                tris.Add(verts.IndexOf(vAP));
                tris.Add(verts.IndexOf(vDP));

                // Peak Triangle Three
                tris.Add(verts.IndexOf(vDP));
                tris.Add(verts.IndexOf(vAP));
                tris.Add(verts.IndexOf(vEP));

                // Peak Triangle Four
                tris.Add(verts.IndexOf(vEP));
                tris.Add(verts.IndexOf(vAP));
                tris.Add(verts.IndexOf(vFP));

                // Peak Triangle Five
                tris.Add(verts.IndexOf(vFP));
                tris.Add(verts.IndexOf(vAP));
                tris.Add(verts.IndexOf(vGP));

                // Peak Triangle Six
                tris.Add(verts.IndexOf(vGP));
                tris.Add(verts.IndexOf(vAP));
                tris.Add(verts.IndexOf(vBP));
                #endregion


                verts[verts.IndexOf(vAP)] = vAP + center.normalized * height;
                verts[verts.IndexOf(vBP)] = vBP + center.normalized * Random.Range(0, height);
                verts[verts.IndexOf(vCP)] = vCP + center.normalized * Random.Range(0, height);
                verts[verts.IndexOf(vDP)] = vDP + center.normalized * Random.Range(0, height);
                verts[verts.IndexOf(vEP)] = vEP + center.normalized * Random.Range(0, height);
                verts[verts.IndexOf(vFP)] = vFP + center.normalized * Random.Range(0, height);
                verts[verts.IndexOf(vGP)] = vGP + center.normalized * Random.Range(0, height);

                vertices = verts.ToArray();
                triangles = tris.ToArray();

                for (int i = 0; i < vertices.Length; i++)
                {
                    if (i != verts.IndexOf(vAP))
                    {
                        uvs.Add(new Vector2(Random.Range(0,2), 0));
                    }
                    else
                    {
                        uvs.Add(new Vector2(1, 0));
                    }
                }

                uv3 = uvs.ToArray();
            }
        }

        #region Triangulation
        public static List<ProcTriangle> DelaunayTriangulationMethod(List<Vector3> sites)
        {
            List<ProcVert> vertices = new List<ProcVert>();

            for (int i = 0; i < sites.Count; i++)
            {
                vertices.Add(new ProcVert(sites[i]));
            }

            List<ProcTriangle> triangles = IncrementalTriangulation(vertices);

            List<HalfEdge> halfEdges = TransformFromTriangleToHalfEdge(triangles);

            int safety = 0;

            int flippedEdges = 0;

            while (true)
            {
                safety++;

                if (safety > 100000)
                {
                    Debug.Log("Delaunay Triangulation Failure.");
                    break;
                }

                bool hasFlippedEdge = false;

                for (int i = 0; i < halfEdges.Count; i++)
                {
                    HalfEdge thisEdge = halfEdges[i];

                    if (thisEdge.oppoEdge == null)
                    {
                        continue;
                    }

                    ProcVert a = thisEdge.v;
                    ProcVert b = thisEdge.nextEdge.v;
                    ProcVert c = thisEdge.prevEdge.v;
                    ProcVert d = thisEdge.oppoEdge.nextEdge.v;

                    Vector2 aPos = a.GetPos2DXZ();
                    Vector2 bPos = b.GetPos2DXZ();
                    Vector2 cPos = c.GetPos2DXZ();
                    Vector2 dPos = d.GetPos2DXZ();

                    if (IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)
                    {
                        if (IsQuadrilateralConvex(aPos, bPos, cPos, dPos))
                        {
                            if (IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f)
                            {
                                continue;
                            }

                            flippedEdges++;

                            hasFlippedEdge = true;

                            FlipEdge(thisEdge);
                        }
                    }
                }

                if (!hasFlippedEdge)
                {
                    break;
                }
            }

            return triangles;
        }

        public static List<ProcTriangle> IncrementalTriangulation(List<ProcVert> points)
        {
            List<ProcTriangle> triangles = new List<ProcTriangle>();

            points = points.OrderBy(n => n.position.x).ToList();

            ProcTriangle newTriangle = new ProcTriangle(points[0].position, points[1].position, points[2].position);

            triangles.Add(newTriangle);

            List<Edge> edges = new List<Edge>();

            edges.Add(new Edge(newTriangle.v1, newTriangle.v2));
            edges.Add(new Edge(newTriangle.v2, newTriangle.v3));
            edges.Add(new Edge(newTriangle.v3, newTriangle.v1));

            for (int i = 3; i < points.Count; i++)
            {
                Vector3 currentPoint = points[i].position;

                List<Edge> newEdges = new List<Edge>();

                for (int j = 0; j < edges.Count; j++)
                {
                    Edge currentEdge = edges[j];

                    Vector3 midPoint = (currentEdge.v1.position + currentEdge.v2.position) / 2f;

                    Edge edgeToMidpoint = new Edge(currentPoint, midPoint);

                    bool canSeeEdge = true;

                    for (int k = 0; k < edges.Count; k++)
                    {
                        if (k == j)
                        {
                            continue;
                        }

                        if (AreEdgesIntersecting(edgeToMidpoint, edges[k]))
                        {
                            canSeeEdge = false;

                            break;
                        }
                    }

                    if (canSeeEdge)
                    {
                        Edge edgeToPoint1 = new Edge(currentEdge.v1, new ProcVert(currentPoint));
                        Edge edgeToPoint2 = new Edge(currentEdge.v2, new ProcVert(currentPoint));

                        newEdges.Add(edgeToPoint1);
                        newEdges.Add(edgeToPoint2);

                        ProcTriangle newTri = new ProcTriangle(edgeToPoint1.v1, edgeToPoint1.v2, edgeToPoint2.v1);

                        triangles.Add(newTri);
                    }
                }

                for (int j = 0; j < newEdges.Count; j++)
                {
                    edges.Add(newEdges[j]);
                }
            }

            return triangles;
        }

        private static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
        {
            Vector2 l1_p1 = new Vector2(edge1.v1.position.x, edge1.v1.position.z);
            Vector2 l1_p2 = new Vector2(edge1.v2.position.x, edge1.v2.position.z);

            Vector2 l2_p1 = new Vector2(edge2.v1.position.x, edge2.v1.position.z);
            Vector2 l2_p2 = new Vector2(edge2.v2.position.x, edge2.v2.position.z);

            bool isIntersecting = AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

            return isIntersecting;
        }

        public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
        {
            bool isIntersecting = false;

            float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

            if (denominator != 0f)
            {
                float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
                float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

                if (shouldIncludeEndPoints)
                {
                    if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f)
                    {
                        isIntersecting = true;
                    }
                }
                else
                {
                    if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f)
                    {
                        isIntersecting = true;
                    }
                }

            }

            return isIntersecting;
        }

        private static void FlipEdge(HalfEdge one)
        {
            HalfEdge two = one.nextEdge;
            HalfEdge three = one.prevEdge;
            HalfEdge four = one.oppoEdge;
            HalfEdge five = one.oppoEdge.nextEdge;
            HalfEdge six = one.oppoEdge.prevEdge;

            ProcVert a = one.v;
            ProcVert b = two.v;
            ProcVert c = three.v;
            ProcVert d = five.v;

            a.halfEdge = one.nextEdge;
            c.halfEdge = one.oppoEdge.nextEdge;

            one.nextEdge = three;
            one.prevEdge = five;

            two.nextEdge = four;
            two.prevEdge = five;

            three.nextEdge = five;
            three.prevEdge = one;

            four.nextEdge = six;
            four.prevEdge = two;

            five.nextEdge = one;
            five.prevEdge = three;

            six.nextEdge = two;
            six.prevEdge = four;

            one.v = b;
            two.v = b;
            three.v = c;
            four.v = d;
            five.v = d;
            six.v = a;

            ProcTriangle t1 = one.t;
            ProcTriangle t2 = four.t;

            one.t = t1;
            three.t = t1;
            five.t = t1;

            two.t = t2;
            four.t = t2;
            six.t = t2;

            t1.v1 = b;
            t1.v2 = c;
            t1.v3 = d;

            t2.v1 = b;
            t2.v2 = d;
            t2.v3 = a;

            t1.halfEdge = three;
            t2.halfEdge = four;
        }

        public static List<HalfEdge> TransformFromTriangleToHalfEdge(List<ProcTriangle> triangles)
        {
            OrientTrianglesClockwise(triangles);

            List<HalfEdge> halfEdges = new List<HalfEdge>(triangles.Count * 3);

            for (int i = 0; i < triangles.Count; i++)
            {
                ProcTriangle t = triangles[i];

                HalfEdge he1 = new HalfEdge(t.v1);
                HalfEdge he2 = new HalfEdge(t.v2);
                HalfEdge he3 = new HalfEdge(t.v3);

                he1.nextEdge = he2;
                he2.nextEdge = he3;
                he3.nextEdge = he1;

                he1.prevEdge = he3;
                he2.prevEdge = he1;
                he3.prevEdge = he2;

                he1.v.halfEdge = he2;
                he2.v.halfEdge = he3;
                he3.v.halfEdge = he1;

                t.halfEdge = he1;

                he1.t = t;
                he2.t = t;
                he3.t = t;

                halfEdges.Add(he1);
                halfEdges.Add(he2);
                halfEdges.Add(he3);
            }

            for (int i = 0; i < halfEdges.Count; i++)
            {
                HalfEdge he = halfEdges[i];

                ProcVert goingToVertex = he.v;
                ProcVert goingFromVertex = he.prevEdge.v;

                for (int j = 0; j < halfEdges.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    HalfEdge heOpposite = halfEdges[j];

                    if (goingFromVertex.position == heOpposite.v.position && goingToVertex.position == heOpposite.prevEdge.v.position)
                    {
                        he.oppoEdge = heOpposite;

                        break;
                    }
                }
            }

            return halfEdges;
        }

        public static void OrientTrianglesClockwise(List<ProcTriangle> triangles)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                ProcTriangle tri = triangles[i];

                Vector2 v1 = new Vector2(tri.v1.position.x, tri.v1.position.z);
                Vector2 v2 = new Vector2(tri.v2.position.x, tri.v2.position.z);
                Vector2 v3 = new Vector2(tri.v3.position.x, tri.v3.position.z);

                if (!IsTriangleOrientedClockwise(v1, v2, v3))
                {
                    tri.ChangeOrientation();
                }
            }
        }

        public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            bool isClockwise = true;

            float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

            if (determinant > 0f)
            {
                isClockwise = false;
            }

            return isClockwise;
        }

        public static float IsPointInsideOutsideOrOnCircle(Vector2 aVec, Vector2 bVec, Vector2 cVec, Vector2 dVec)
        {
            float a = aVec.x - dVec.x;
            float d = bVec.x - dVec.x;
            float g = cVec.x - dVec.x;

            float b = aVec.y - dVec.y;
            float e = bVec.y - dVec.y;
            float h = cVec.y - dVec.y;

            float c = a * a + b * b;
            float f = d * d + e * e;
            float i = g * g + h * h;

            float determinant = (a * e * i) + (b * f * g) + (c * d * h) - (g * e * c) - (h * f * a) - (i * d * b);
            return determinant;
        }

        public static bool IsQuadrilateralConvex(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            bool isConvex = false;

            bool abc = IsTriangleOrientedClockwise(a, b, c);
            bool abd = IsTriangleOrientedClockwise(a, b, d);
            bool bcd = IsTriangleOrientedClockwise(b, c, d);
            bool cad = IsTriangleOrientedClockwise(c, a, d);

            if (abc && abd && bcd && !cad)
            {
                isConvex = true;
            }
            else if (abc && abd && !bcd & cad)
            {
                isConvex = true;
            }
            else if (abc && !abd && bcd & cad)
            {
                isConvex = true;
            }
            else if (!abc && !abd && !bcd & cad)
            {
                isConvex = true;
            }
            else if (!abc && !abd && bcd & !cad)
            {
                isConvex = true;
            }
            else if (!abc && abd && !bcd & !cad)
            {
                isConvex = true;
            }

            return isConvex;
        }

        public class ProcVert
        {
            public Vector3 position;
            public HalfEdge halfEdge;
            public ProcTriangle triangle;

            public ProcVert prevVertex;
            public ProcVert nextVertex;

            public bool isReflex;
            public bool isConvex;
            public bool isEar;

            public ProcVert(Vector3 position)
            {
                this.position = position;
            }

            public Vector2 GetPos2DXZ()
            {
                Vector2 pos2D = new Vector2(position.x, position.y);
                return pos2D;
            }
        }

        public class Edge
        {
            public ProcVert v1;
            public ProcVert v2;

            public bool isIntersecting = false;

            public Edge(ProcVert v1, ProcVert v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }

            public Edge(Vector3 v1, Vector3 v2)
            {
                this.v1 = new ProcVert(v1);
                this.v2 = new ProcVert(v2);
            }

            public Vector2 GetVertex2D(ProcVert v)
            {
                return new Vector2(v.position.x, v.position.y);
            }

            public void FlipEdge()
            {
                ProcVert temp = v1;
                v1 = v2;
                v2 = temp;
            }
        }

        public class HalfEdge
        {
            public ProcVert v;
            public ProcTriangle t;

            public HalfEdge nextEdge;
            public HalfEdge prevEdge;
            public HalfEdge oppoEdge;

            public HalfEdge(ProcVert v)
            {
                this.v = v;
            }
        }

        public class ProcTriangle
        {
            public ProcVert v1;
            public ProcVert v2;
            public ProcVert v3;

            public HalfEdge halfEdge;

            public ProcTriangle(ProcVert v1, ProcVert v2, ProcVert v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }

            public ProcTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                this.v1 = new ProcVert(v1);
                this.v2 = new ProcVert(v2);
                this.v3 = new ProcVert(v3);
            }

            public ProcTriangle(HalfEdge halfEdge)
            {
                this.halfEdge = halfEdge;
            }

            public void ChangeOrientation()
            {
                ProcVert temp = this.v1;

                this.v1 = this.v2;

                this.v2 = temp;
            }
        }
        #endregion
    }
}
