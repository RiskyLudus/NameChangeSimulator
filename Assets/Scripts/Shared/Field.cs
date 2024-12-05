using System;
using NameChangeSimulator.Shared;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Field
{
    public string Name = string.Empty;
    public StateData ParentStateData;
    public string Value
    {
        get => _value;
        set
        {
            if (value == TriggerLink)
            {
                if (LinkedFieldNames == null) return;
                foreach (var linkedField in LinkedFieldNames)
                {
                    foreach (var field in ParentStateData.fields)
                    {
                        if (field.Name == linkedField)
                        {
                            field.Value = LinkedValueToSet;
                        }
                    }
                    
                }
            }

            _value = value;
        }
    }
    
    private string _value = string.Empty;

    public bool IsCheck = false;
    public bool IsText = false;
    
    public string[] LinkedFieldNames = null;
    public string TriggerLink = string.Empty;
    public string LinkedValueToSet = string.Empty;
}
