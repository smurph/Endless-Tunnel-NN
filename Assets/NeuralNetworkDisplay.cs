using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LineRenderer))]
public class NeuralNetworkDisplay : MonoBehaviour
{

    public GameObject Node;

    private LineRenderer _lineRenderer;
    

    // Use this for initialization
    void Start () {
        var nodes = GameObject.FindGameObjectsWithTag("NeuralNetNode");
        _lineRenderer = GetComponent<LineRenderer>();
        
        Assert.AreNotEqual(0, nodes.Length, "Didn't find enough nodes");
        Assert.IsNotNull(_lineRenderer, "Didn't find a line renderer");
        
    }
	
	// Update is called once per frame
	void Update () {
        
        var nodes = GameObject.FindGameObjectsWithTag("NeuralNetNode");
        _lineRenderer.SetPositions(nodes.Select(n => n.transform.position).ToArray());
        
    }
}
