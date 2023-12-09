using UnityEngine;

namespace Vket2023Winter.Circle0000.Bezier
{
    public class Bezier : MonoBehaviour
    {
        [SerializeField] Transform _pointRoot;

        Transform[] _points { get; set; }

        Vector3[] _subPoints;

        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            _points = new Transform[_pointRoot.childCount];
            for (int i = 0; i < _pointRoot.childCount; i++)
            {
                _points[i] = _pointRoot.GetChild(i);
            }
        }

        void OnDrawGizmos()
        {
            DrawPoints();
            DrawCoaster();
        }

        void DrawPoints()
        {
            if (_points == null)
            {
                return;
            }

            if (_subPoints == null || _subPoints.Length != _points.Length)
            {
                _subPoints = new Vector3[_points.Length];
            }

            for (int i = 0; i < _points.Length; i++)
            {
                int j = (i + 1) % _points.Length;
                _subPoints[i] = (_points[i].position + _points[j].position) * 0.5f;
            }

            Vector3 prePosition = _subPoints[0];
            var isStartPoint = true;
            for (int i = 0; i < _points.Length; i++)
            {
                for (float t = 0; t < 1; t += 0.1f)
                {
                    if (isStartPoint)
                    {
                        isStartPoint = false;
                        continue;
                    }

                    var p = CalcPosition(i, t);
                    SetPointLineGismoClolor(t);
                    Gizmos.DrawLine(prePosition, p);
                    prePosition = p;
                }
            }
            Gizmos.DrawLine(prePosition, CalcPosition(0, 0));
        }

        Vector3 CalcPosition(int i, float t)
        {
            i %= _points.Length;
            int j = (i + 1) % _points.Length;
            float t1 = 1 - t;
            return t1 * t1 * _subPoints[i] + 2 * t1 * t * _points[j].position + t * t * _subPoints[j];
        }

        void SetPointLineGismoClolor(float t)
        {
            Gizmos.color = GetColorByHsv((int)(t * 360));
        }

        Color32 GetColorByHsv(int h)
        {
            float s = 1, v = 1;
            Color32 c = new Color32();
            int i = (int)(h / 60f);
            float f = h / 60f - i;
            byte p1 = (byte)(v * (1 - s) * 255);
            byte p2 = (byte)(v * (1 - s * f) * 255);
            byte p3 = (byte)(v * (1 - s * (1 - f)) * 255);
            byte vi = (byte)(v * 255);
            byte r = 0, g = 0, b = 0;
            switch (i)
            {
                case 0: r = vi; g = p3; b = p1; break;
                case 1: r = p2; g = vi; b = p1; break;
                case 2: r = p1; g = vi; b = p3; break;
                case 3: r = p1; g = p2; b = vi; break;
                case 4: r = p3; g = p1; b = vi; break;
                case 5: r = vi; g = p1; b = p2; break;
                default: break;
            }
            c.a = 255;
            c.r = r;
            c.g = g;
            c.b = b;
            return c;
        }

        float _currentPointTime;
        int _currentPointIdx;

        void DrawCoaster()
        {
            if (_points == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(CalcPosition(_currentPointIdx, _currentPointTime), 0.05f);

            _currentPointTime += Time.deltaTime;
            if (1 <= _currentPointTime)
            {
                _currentPointIdx = (_currentPointIdx + Mathf.FloorToInt(_currentPointTime)) % _points.Length;
                _currentPointTime %= 1;
            }
        }

        float[] pointLength;

        float CalcOrGetLength(int i)
        {
            if (pointLength == null)
            {
                pointLength = new float[_points.Length];
                if (pointLength[i] <= 0)
                {
                    pointLength[i] = CalcLength(i);
                }
            }

            return pointLength[i];
        }

        float CalcLength(int i)
        {
            int divCount = 10;
            float length = 0;
            Vector3 prePosition = Vector3.zero;
            for (int j = 0; j < divCount; j++)
            {
                float t = j / (float)divCount;
                var p = CalcPosition(i, t);
                if (j != 0)
                {
                    length += (p - prePosition).magnitude;
                }
                prePosition = p;
            }
            return length;
        }
    }
}
