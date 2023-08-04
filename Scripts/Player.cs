using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {
    
    private static Player s_Instance = null;

    public bool IsLogin { get; set; } = false;
    private Player() { }
    
    public static Player Instance {
        get {
            if (s_Instance == null) 
                s_Instance = new Player();
            return s_Instance;
        }
    }

    
}
