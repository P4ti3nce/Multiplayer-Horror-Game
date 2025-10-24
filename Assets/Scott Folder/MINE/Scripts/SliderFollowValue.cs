using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class SliderFollowValue : MonoBehaviour
{
    public Component targetComponent;         // Drag the component in the inspector
    public string targetFieldName;            // Name of the float field or property
    private FieldInfo fieldInfo;
    private PropertyInfo propertyInfo;
    private Slider slider;

    private float startingValue;
    private bool isValid;

    void Start()
    {
        slider = GetComponent<Slider>();

        if (targetComponent == null || string.IsNullOrEmpty(targetFieldName))
        {
            Debug.LogWarning("SliderFollowValue: Missing targetComponent or targetFieldName");
            return;
        }

        // Try to find a field or property with the given name
        var type = targetComponent.GetType();
        fieldInfo = type.GetField(targetFieldName);
        propertyInfo = type.GetProperty(targetFieldName);

        if (fieldInfo != null && fieldInfo.FieldType == typeof(float))
        {
            startingValue = (float)fieldInfo.GetValue(targetComponent);
            isValid = true;
        }
        else if (propertyInfo != null && propertyInfo.PropertyType == typeof(float))
        {
            startingValue = (float)propertyInfo.GetValue(targetComponent);
            isValid = true;
        }
        else
        {
            Debug.LogWarning($"SliderFollowValue: Field or Property '{targetFieldName}' not found or not a float on {targetComponent.name}");
            isValid = false;
        }

        if (isValid)
        {
            slider.maxValue = startingValue;
        }
    }

    void Update()
    {
        if (!isValid) return;

        float value = 0;

        if (fieldInfo != null)
            value = (float)fieldInfo.GetValue(targetComponent);
        else if (propertyInfo != null)
            value = (float)propertyInfo.GetValue(targetComponent);

        slider.value = value == 0 ? slider.maxValue : value;
    }
}
