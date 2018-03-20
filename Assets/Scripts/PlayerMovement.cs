using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private GameManager _manager { get { return GameManager.Instance; } }

    public bool ShowDebugInfo;

    public GameObject ShipDebugPage;

    public Text RaycastUp;
    public Text RaycastUpRight;
    public Text RaycastRight;
    public Text RaycastDownRight;
    public Text RaycastDown;

    // Update is called once per frame
    void Update () {
        if (!_manager.IsRunning) return;
        transform.Translate(new Vector3(0, Input.GetAxis("Vertical") * Time.deltaTime * _manager.ShipSpeed));

        ReportDistancesToDebugPage();
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameEnd += OnGameEnd;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameEnd -= OnGameEnd;
    }

    private void OnGameEnd()
    {
        if (ShipDebugPage)
        {
            ShipDebugPage.SetActive(false);
        }
    }

    private void OnGameStart()
    {
        if (ShowDebugInfo && ShipDebugPage)
        {
            ShipDebugPage.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "wall")
        {
            _manager.GameOver();

            transform.position = new Vector3(transform.position.x, 0);
        }
    }

    private void ReportDistancesToDebugPage()
    {
        if (!ShowDebugInfo || !ShipDebugPage) return;

        var distances = GetRaycastDistances();
        RaycastUp.text = Math.Round(distances[0], 3).ToString();
        RaycastUpRight.text = Math.Round(distances[1], 3).ToString();
        RaycastRight.text = Math.Round(distances[2], 3).ToString();
        RaycastDownRight.text = Math.Round(distances[3], 3).ToString();
        RaycastDown.text = Math.Round(distances[4], 3).ToString();
    }

    private float[] GetRaycastDistances()
    {
        var distances = new List<float>();
        var origin = new Vector2(transform.position.x + 0.4f, transform.position.y);
        var raycastDistance = 22f;

        var upRight = new Vector2(1, 1);
        var downRight = new Vector2(1, -1);

        distances.Add(GetRayCastDistance(new Vector2(transform.position.x, transform.position.y + 0.2f), Vector2.up, raycastDistance));
        distances.Add(GetRayCastDistance(origin, upRight, raycastDistance));
        distances.Add(GetRayCastDistance(origin, Vector2.right, raycastDistance));
        distances.Add(GetRayCastDistance(origin, downRight, raycastDistance));
        distances.Add(GetRayCastDistance(new Vector2(transform.position.x, transform.position.y - 0.2f), Vector2.down, raycastDistance));

        return distances.ToArray();
    }

    private float GetRayCastDistance(Vector2 origin, Vector2 dir, float raycastDistance)
    {
        var wallMask = 1 << LayerMask.NameToLayer("Walls");
        var ray = Physics2D.Raycast(origin, dir, raycastDistance, wallMask);
        if (ray.collider)
        {
            Debug.DrawRay(origin, transform.InverseTransformDirection(ray.point - origin), Color.red, Time.deltaTime);
            return ray.distance;
        }

        Debug.DrawRay(origin, dir * raycastDistance, Color.red, Time.deltaTime);
        return raycastDistance;
    }
}
