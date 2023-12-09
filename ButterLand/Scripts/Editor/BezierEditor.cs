using UnityEngine;
using UnityEditor;

namespace Vket2023Winter.Circle0000.Bezier
{
    [CustomEditor(typeof(Bezier))]
    public class BezierEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Bezier myTarget = (Bezier)target;

            myTarget.Initialize();
        }
    }
}
