using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
  public void CloseUI(GameObject obj)
    {
        obj.SetActive(false);
    }
}
