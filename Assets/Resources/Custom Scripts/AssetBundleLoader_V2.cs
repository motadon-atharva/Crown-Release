using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class Models
{
    public Model[] body;
}

[Serializable]
public class Model
{
    public string model_url;
    public string prefab_name;
    public string model_show_text;
}

public class ModelsParents
{
    public AssetBundle Bundle;
    public string Name;
    public Transform Parent;
    public string ModelShowText;
}

public class FinalObjects
{
    public GameObject Model;
    public Transform Parent;
    public string ModelShowText;
}

public class AssetBundleLoader_V2 : MonoBehaviour
{
    //iOS
    private string asset_url =
        "https://api.backendless.com/4BF8614F-72BC-4B4B-83B4-6B7EC005E492/FB5E05E2-E656-40A1-8D84-E424E3F78476/data/oitios_2019_2_18f1?sortBy=model_show_text%20asc";

    //Android
//    private string asset_url = "https://api.backendless.com/4BF8614F-72BC-4B4B-83B4-6B7EC005E492/FB5E05E2-E656-40A1-8D84-E424E3F78476/data/one_image_target_android?sortBy=model_show_text%20asc";
    private Transform parent;
    private GameObject parentGameObject;
    private string jsonResult;
    private int switchCount = 0;
    private AssetBundle bundle;
    private FinalObjects finalObjects;
    
//    public Slider slider;
//    public Text informationBox;
    public GameObject buttonPanel;
    public Button changeObjectButton;
    public Text modelShowText;
    public GameObject grayPanel;


    // Start is called before the first frame update
    void Start()
    {
        grayPanel.SetActive(false);
        buttonPanel.SetActive(true);
        parentGameObject = GameObject.Find("ControllerObject");
        if (parentGameObject != null)
        {
            Debug.Log("Got Parent GameObject: " + parentGameObject.name);
            parent = parentGameObject.transform;
        }

        StartCoroutine(RestClient.Instance.Get(asset_url));
        changeObjectButton.onClick.AddListener(LoadAssetToView);
    }
    
    public IEnumerator SetJsonResult(string json)
    {
        jsonResult = json;
        Debug.Log("Json Result: " + jsonResult);
        yield return null;
    }

    void LoadAssetToView()
    {
        grayPanel.SetActive(true);
//        buttonPanel.SetActive(false);
        Models jsonObject = JsonUtility.FromJson<Models>(jsonResult);

        var pos = 0;
        if (switchCount == 0)
        {
            pos = switchCount;
            switchCount++;
        }
        else
        {
            if (switchCount >= jsonObject.body.Length)
            {
                //Unload asset bundle
                AssetBundle.Destroy(finalObjects.Parent.transform
                    .Find(GetFullModelName(finalObjects.Model.name)).gameObject);
                bundle.Unload(true);
                switchCount = 0;
                pos = switchCount;
            }
            else
            {
                //Unload asset bundle
                AssetBundle.Destroy(finalObjects.Parent.transform
                    .Find(GetFullModelName(finalObjects.Model.name)).gameObject);
                bundle.Unload(true);
                pos = switchCount;
            }

            switchCount++;
        }
        
        //Load model at 'pos'
        StartCoroutine(DownloadModelOrGetFromCache(pos, jsonObject));
    }

    IEnumerator DownloadModelOrGetFromCache(int pos, Models modelsList)
    {
        Debug.Log(pos);
        // Wait for the Caching system to be ready
        while (!Caching.ready)
        {
            yield return null;
        }

        float maxProgress = 1;
        float progress = 0;

        // get current bundle hash from server, random value added to avoid caching
        UnityWebRequest www =
            UnityWebRequest.Get(modelsList.body[pos].model_url + ".manifest?r=" + (Random.value * 9999999));
        Debug.Log("Loading manifest:" + modelsList.body[pos].model_url + ".manifest");

        // wait for load to finish
        yield return www.SendWebRequest();

        // if received error, exit
        if (www.isNetworkError == true)
        {
            Debug.LogError("www error: " + www.error);
            www.Dispose();
            www = null;
            yield break;
        }

        // create empty hash string
        Hash128 hashString = (default(Hash128)); // new Hash128(0, 0, 0, 0);

        // check if received data contains 'ManifestFileVersion'
        if (www.downloadHandler.text.Contains("ManifestFileVersion"))
        {
            // extract hash string from the received data, TODO should add some error checking here
            var hashRow = www.downloadHandler.text.ToString().Split("\n".ToCharArray())[5];
            hashString = Hash128.Parse(hashRow.Split(':')[1].Trim());

            if (hashString.isValid == true)
            {
                // we can check if there is cached version or not
                if (Caching.IsVersionCached(modelsList.body[pos].model_url, hashString) == true)
                {
                    Debug.Log("Bundle with this hash is already cached!");
                }
                else
                {
                    Debug.Log("No cached version founded for this hash..");
                }
            }
            else
            {
                // invalid loaded hash, just try loading latest bundle
                Debug.LogError("Invalid hash:" + hashString);
                yield break;
            }
        }
        else
        {
            Debug.LogError("Manifest doesn't contain string 'ManifestFileVersion': " + modelsList.body[pos].model_url +
                           ".manifest");
            yield break;
        }


        // now download the actual bundle, with hashString parameter it uses cached version if available
        www = UnityWebRequestAssetBundle.GetAssetBundle(
            modelsList.body[pos].model_url + "?r=" + (Random.value * 9999999), hashString, 0);

        // wait for load to finish
        www.SendWebRequest();

        if (www.error != null)
        {
            Debug.LogError("www error: " + www.error);
            www.Dispose();
            www = null;
            yield break;
        }
        else
        {
            //To remember the last progress
            float lastProgress = progress;
            //informationBox.text = "Downloading resources...";
            while (!www.isDone)
            {
                //Calculate the current progress
                progress = lastProgress + www.downloadProgress;
                //Get a percentage
                float progressPercentage = (progress / maxProgress) * 100;
                Debug.Log("Downloaded: " + progressPercentage + "%");
                yield return new WaitForSeconds(0.1f);
                //slider.value = Mathf.Clamp01(progress / maxProgress);
            }

            bundle = DownloadHandlerAssetBundle.GetContent(www);
            Debug.Log("Download Completed.");

            //Add the model and parent combination to list for later instantiation
            ModelsParents modelsParents = new ModelsParents();
            //Downloaded Asset Bundle
            modelsParents.Bundle = bundle;
            //Asset Bundle Name
            modelsParents.Name = modelsList.body[pos].prefab_name;
            //GameObject for the model to be placed in
            modelsParents.Parent = parent;
            //Model Show Text
            modelsParents.ModelShowText = modelsList.body[pos].model_show_text;

            StartCoroutine(CreateFinalObject(modelsParents));
        }
    }

    IEnumerator CreateFinalObject(ModelsParents modelsParents)
    {
        AssetBundleRequest request = modelsParents.Bundle.LoadAssetAsync(modelsParents.Name);
        yield return request;
        GameObject gameObject = request.asset as GameObject;
        
        finalObjects = new FinalObjects();
        finalObjects.Model = gameObject;
        finalObjects.Parent = modelsParents.Parent;
        finalObjects.ModelShowText = modelsParents.ModelShowText;
        finalObjects.Model.SetActive(false);
        
        //Show the Model & Text
        Instantiate(finalObjects.Model, finalObjects.Parent);
        finalObjects.Parent.transform
            .Find(GetFullModelName(finalObjects.Model.name)).gameObject.SetActive(true);
        modelShowText.text = finalObjects.ModelShowText;
        grayPanel.SetActive(false);
        buttonPanel.SetActive(true);
    }
    
    string GetFullModelName(String name)
    {
        return name + "(Clone)";
    }
}