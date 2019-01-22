using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachScript : MonoBehaviour {
    bool attached = false;
	
	void Update () {
        GameObject _player = GameObject.Find("player");
        if(_player != null && attached==false)
        {
            attached = true;
            _player.AddComponent<Rotate>();
        }
	}
}
