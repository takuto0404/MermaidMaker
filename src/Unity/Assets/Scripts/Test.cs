using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Jamaica
{
    public static class UGUILineRenderer
    {
        public static Vector2[] Positions = { }; 
        private static float _weight;
        private static Vector2[] _tokyo;

        public static void SetPositions(Vector2[] newPositions)
        {
            var processedPosition =
                newPositions.Select(position => new Vector2(position.x - Screen.width / 2f, position.y - Screen.height / 2f)).ToArray();
            Positions = processedPosition;
        }

        private static void OnPopulateMesh(VertexHelper vh)
        {
            if (Positions.Length >= 2)
            {
                // （１）過去の頂点を削除
                vh.Clear();

                for (var i = 0; i < Positions.Length - 1; i++)
                {
                    var position1 = Positions[i];
                    var position2 = Positions[i + 1];
                    // （２）垂直ベクトルの計算
                    var pos1To2 = position2 - position1;
                    var verticalVector = CalculateVerticalVector(pos1To2);

                    // （３）左下、左上のベクトルを計算
                    var pos1Top = position1 + verticalVector * -_weight / 2;
                    var pos1Bottom = position1 + verticalVector * _weight / 2;
                    var pos2Top = position2 + verticalVector * -_weight / 2;
                    var pos2Bottom = position2 + verticalVector * _weight / 2;

                    // （４）頂点を頂点リストに追加
                    AddVert(vh, pos1Top);
                    AddVert(vh, pos1Bottom);
                    AddVert(vh, pos2Top);
                    AddVert(vh, pos2Bottom);


                    var indexBuffer = i * 4;

                    // （５）頂点リストを元にメッシュを貼る
                    vh.AddTriangle(0 + indexBuffer, 1 + indexBuffer, 2 + indexBuffer);
                    vh.AddTriangle(1 + indexBuffer, 2 + indexBuffer, 3 + indexBuffer);
                }
            }
        }

        private static void AddVert(VertexHelper vh, Vector2 pos)
        {
            var vert = UIVertex.simpleVert;
            vert.position = pos;
            vh.AddVert(vert);
        }

        private static Vector2 CalculateVerticalVector(Vector2 vec)
        {
            // 0除算の防止
            if (vec.y == 0)
            {
                return Vector2.up;
            }

            {
                var verticalVector = new Vector2(1.0f, -vec.x / vec.y);
                return verticalVector.normalized;
            }
        }
    }
}