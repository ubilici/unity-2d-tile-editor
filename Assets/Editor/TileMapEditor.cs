using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TileMap))]
public class TileMapEditor : Editor
{
    public TileMap map;
    TileBrush brush;
    Vector3 mouseHitPos;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();

        var oldSize = map.mapSize;
        map.mapSize = EditorGUILayout.Vector2Field("Map Size:", map.mapSize);

        if (map.mapSize != oldSize)
        {
            UpdateCalculations();
        }

        map.texture2D = (Texture2D)EditorGUILayout.ObjectField("Texture2D:", map.texture2D, typeof(Texture2D), false);

        if (map.texture2D == null)
        {
            EditorGUILayout.HelpBox("You have not selected a 2D texture yet.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField("Tile Size:", map.tileSize.x + "x" + map.tileSize.y);
            EditorGUILayout.LabelField("Grid Size in Units:", map.gridSize.x + "x" + map.gridSize.y);
            EditorGUILayout.LabelField("Pixels to Units:", map.pixelsToUnits.ToString());
            UpdateBrush(map.currentTileBrush);
        }

        EditorGUILayout.EndVertical();
    }

    void OnEnable()
    {
        map = target as TileMap;
        Tools.current = Tool.View;

        if (map.texture2D != null)
        {
            UpdateCalculations();
            NewBrush();
        }

    }

    void OnDisable()
    {
        DestroyBrush();
    }

    void OnSceneGUI()
    {
        if (brush != null)
        {
            UpdateHitPosition();
            MoveBrush();
        }
    }

    void UpdateCalculations()
    {
        var path = AssetDatabase.GetAssetPath(map.texture2D);
        map.spriteRefrences = AssetDatabase.LoadAllAssetsAtPath(path);

        var sprite = (Sprite)map.spriteRefrences[1];
        var width = sprite.textureRect.width;
        var height = sprite.textureRect.height;

        map.tileSize = new Vector2(width, height);
        map.pixelsToUnits = (int)(sprite.rect.width / sprite.bounds.size.x);
        map.gridSize = new Vector2((width / map.pixelsToUnits) * map.mapSize.x, (height / map.pixelsToUnits) * map.mapSize.y);

    }

    void CreateBrush()
    {
        var sprite = map.currentTileBrush;
        if (sprite != null)
        {
            GameObject go = new GameObject("Brush");
            go.transform.SetParent(map.transform);

            brush = go.AddComponent<TileBrush>();
            brush.renderer2D = go.AddComponent<SpriteRenderer>();

            var pixelsToUnits = map.pixelsToUnits;
            brush.brushSize = new Vector2(sprite.textureRect.width / pixelsToUnits, sprite.textureRect.height / pixelsToUnits);

            brush.UpdateBrush(sprite);
        }
    }

    void NewBrush()
    {
        if (brush == null)
        {
            CreateBrush();
        }
    }

    void DestroyBrush()
    {
        if (brush != null)
        {
            DestroyImmediate(brush.gameObject);
        }
    }

    public void UpdateBrush(Sprite sprite)
    {
        if (brush != null)
        {
            brush.UpdateBrush(sprite);
        }
    }

    void UpdateHitPosition()
    {
        var p = new Plane(map.transform.TransformDirection(Vector3.forward), Vector3.zero);
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        var hit = Vector3.zero;
        var dist = 0f;

        if(p.Raycast(ray, out dist)){
            hit = ray.origin + ray.direction.normalized * dist;

            mouseHitPos = map.transform.InverseTransformPoint(hit);
        }
    }

    void MoveBrush()
    {
        var tileSize = map.tileSize.x / map.pixelsToUnits;

        var x = Mathf.Floor(mouseHitPos.x / tileSize) * tileSize;
        var y = Mathf.Floor(mouseHitPos.y / tileSize) * tileSize;

        var row = x / tileSize;
        var column = Mathf.Abs(y / tileSize) - 1;

        var id = (int)((column * map.mapSize.x) + row);

        brush.tileID = id;

        x += map.transform.position.x + tileSize / 2;
        y += map.transform.position.y + tileSize / 2;

        brush.transform.position = new Vector3(x, y, map.transform.position.z);
    }

}
