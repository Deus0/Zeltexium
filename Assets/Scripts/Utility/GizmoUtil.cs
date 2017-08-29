using UnityEngine;

namespace Zeltex.Util
{
    public class GizmoUtil : MonoBehaviour
    {
        public static void DrawCube(Vector3 Position, Vector3 Size)
        {
            DrawCube(Position, Size, Quaternion.identity);
        }
        public static void DrawCube(Vector3 Position, Vector3 Size, Quaternion MyRotation)
        {
            bool IsDebugCentre = false;
            Vector3 PointSize = 0.01f * (new Vector3(1, 1, 1));
            Vector3 Min = -Size / 2f;
            Vector3 Max = Size / 2f;
            Vector3 Point1 = Position + MyRotation * new Vector3(Min.x, Max.y, Max.z);
            Vector3 Point2 = Position + MyRotation * new Vector3(Min.x, Max.y, Min.z);
            Vector3 Point3 = Position + MyRotation * new Vector3(Min.x, Min.y, Min.z);
            Vector3 Point4 = Position + MyRotation * new Vector3(Min.x, Min.y, Max.z);
            Vector3 Point5 = Position + MyRotation * new Vector3(Max.x, Min.y, Min.z);
            Vector3 Point6 = Position + MyRotation * new Vector3(Max.x, Min.y, Max.z);
            Vector3 Point7 = Position + MyRotation * new Vector3(Max.x, Max.y, Max.z);
            Vector3 Point8 = Position + MyRotation * new Vector3(Max.x, Max.y, Min.z);

            if (IsDebugCentre)
            {
                Gizmos.color = Color.white; // debug cube points
                Gizmos.DrawCube(Position, PointSize);
                Gizmos.DrawLine(Position, Point1);
                Gizmos.DrawLine(Position, Point2);
                Gizmos.DrawLine(Position, Point3);
                Gizmos.DrawLine(Position, Point4);
                Gizmos.DrawLine(Position, Point5);
                Gizmos.DrawLine(Position, Point6);
                Gizmos.DrawLine(Position, Point7);
                Gizmos.DrawLine(Position, Point8);
            }
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(Point1, PointSize);
            Gizmos.DrawCube(Point2, PointSize);
            Gizmos.DrawCube(Point3, PointSize);
            Gizmos.DrawCube(Point4, PointSize);
            Gizmos.DrawCube(Point5, PointSize);
            Gizmos.DrawCube(Point6, PointSize);
            Gizmos.DrawCube(Point7, PointSize);
            Gizmos.DrawCube(Point8, PointSize);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Point1, Point2);
            Gizmos.DrawLine(Point3, Point4);
            Gizmos.DrawLine(Point1, Point4);
            Gizmos.DrawLine(Point2, Point3);

            Gizmos.DrawLine(Point5, Point6);
            Gizmos.DrawLine(Point7, Point8);
            Gizmos.DrawLine(Point5, Point8);
            Gizmos.DrawLine(Point6, Point7);

            Gizmos.DrawLine(Point1, Point2);
            Gizmos.DrawLine(Point8, Point7);
            Gizmos.DrawLine(Point1, Point7);
            Gizmos.DrawLine(Point2, Point8);

            Gizmos.DrawLine(Point5, Point6);
            Gizmos.DrawLine(Point4, Point3);
            Gizmos.DrawLine(Point5, Point3);
            Gizmos.DrawLine(Point6, Point4);
        }
    }
}