using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEffect : MonoBehaviour, ICardEffect
{
    public int Amount;

    public void Apply(){
        Debug.LogFormat("Dealt {0} damage", Amount);
    }
}
