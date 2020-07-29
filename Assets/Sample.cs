using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Sample : MonoBehaviour
{

    // Start is called before the first frame update
    async void Start()
    {
        var cube = await Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Cube.prefab").Task;
        Instantiate(cube);
        Debug.Log("Cube create complete.");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
