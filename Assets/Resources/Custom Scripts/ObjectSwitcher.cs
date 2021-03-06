﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

class Model_Show_Name
{
    public GameObject Model;
    public string ShowName;
}

public class ObjectSwitcher : MonoBehaviour
{
    private List<FinalObjects> _finalObjectsList;
    public Button changeObjectButton;
    private int _switchCount = 0;
    public Text modelShowText;
    private List<Model_Show_Name> _modelShowNames = new List<Model_Show_Name>();

    // Start is called before the first frame update
    void Start()
    {
        changeObjectButton.enabled = false;
        changeObjectButton.onClick.AddListener(SwitchObject);
    }

    void populateModelsArray(object[] args)
    {
        Transform parent = args[0] as Transform;
        _finalObjectsList = args[1] as List<FinalObjects>;
        Debug.Log("Populating Models Array for " + parent.childCount + " children...");

//        foreach (FinalObjects finalObjects in _finalObjectsList)
//        {
//            Model_Show_Name obj = new Model_Show_Name();
//            obj.Model = parent.transform.Find(GetFullModelName(finalObjects.Model.name)).gameObject;
//            obj.ShowName = finalObjects.ModelShowText;
//            _modelShowNames.Add(obj);
//        }
        
//        changeObjectButton.GetComponent<Image>().color = Color.green;
        changeObjectButton.enabled = true;
    }

    string GetFullModelName(String name)
    {
        return name + "(Clone)";
    }

//    private void SwitchObject()
//    {
//        Debug.Log("Switch Count: " + _switchCount);
//        var pos = 0;
//        Debug.Log("Switching Object...");
//        if(_switchCount == 0)
//        {
//            pos = _switchCount;
//            _switchCount++;
//        }
//        else
//        {
//            if(_switchCount >= _modelShowNames.Count)
//            {
//                _modelShowNames[_modelShowNames.Count - 1].Model.SetActive(false);
//                Destroy(_modelShowNames[_modelShowNames.Count - 1].Model);
//                _switchCount = 0;
//                pos = _switchCount;
//            }
//            else
//            {              
//                pos = _switchCount;
//                _modelShowNames[pos - 1].Model.SetActive(false);
//                Destroy(_modelShowNames[pos - 1].Model);
//            }
//            _switchCount++;
//        }
//        _modelShowNames[pos].Model.SetActive(true);
//        modelShowText.text = _modelShowNames[pos].ShowName;
//    }

    private void SwitchObject()
    {
        Debug.Log("Switch Count: " + _switchCount);
        var pos = 0;
        Debug.Log("Switching Object...");
        if(_switchCount == 0)
        {
            pos = _switchCount;
            _switchCount++;
        }
        else
        {
            if(_switchCount >= _finalObjectsList.Count)
            {
//                _modelShowNames[_modelShowNames.Count - 1].Model.SetActive(false);
                DestroyImmediate(_finalObjectsList[_finalObjectsList.Count - 1].Parent.transform
                    .Find(GetFullModelName(_finalObjectsList[_finalObjectsList.Count - 1].Model.name)).gameObject, true);
                _switchCount = 0;
                pos = _switchCount;
            }
            else
            {              
                pos = _switchCount;
//                _modelShowNames[pos - 1].Model.SetActive(false);
//                DestroyImmediate(_finalObjectsList[pos - 1].Model, true);
                DestroyImmediate(_finalObjectsList[pos - 1].Parent.transform
                    .Find(GetFullModelName(_finalObjectsList[pos - 1].Model.name)).gameObject, true);
            }
            _switchCount++;
        }
//        _modelShowNames[pos].Model.SetActive(true);
        Instantiate(_finalObjectsList[pos].Model, _finalObjectsList[pos].Parent);
        _finalObjectsList[pos].Parent.transform
            .Find(GetFullModelName(_finalObjectsList[pos].Model.name)).gameObject.SetActive(true);
//        modelShowText.text = _modelShowNames[pos].ShowName;
    }
}
