using System.Collections.Generic;
using UnityEngine;

public class GenerationHealthManager : MonoBehaviour
{
    public static GenerationHealthManager Instance { get; set; }

    public GameObject Prefab;
    public GameObject Background;

    private Dictionary<Color, GameObject> _ships;

	// Use this for initialization
	void Start () {
        Instance = this;
        ResetShips();
	}

    public void ResetShips()
    {
        if (_ships != null)
        {
            foreach(var ship in _ships)
            {
                Destroy(ship.Value);
            }
        }

        _ships = new Dictionary<Color, GameObject>();
        Background.SetActive(false);
    }

    public void AddShip(Color color)
    {
        var obj = Instantiate(Prefab);
        var renderer = obj.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        renderer.color = color;

        obj.transform.SetParent(gameObject.transform);
        ShiftNewShipObject(obj, _ships.Count);
        _ships.Add(color, obj);

        if (!Background.activeSelf) Background.SetActive(true);
    }

    public void ShipDied(Color color)
    {
        _ships[color].SetActive(false);
    }

    private void ShiftNewShipObject(GameObject obj, int n)
    {
        float shiftFactor = 25.0f;
        
        obj.transform.localPosition = new Vector3(-266f + (shiftFactor * n), 171, 0);
        obj.transform.localScale = new Vector3(5, 5, 0);
    }
}
