using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class DamageNumberSetup : EditorWindow
{
    [MenuItem("Tools/Setup Damage Number System")]
    public static void SetupDamageNumberSystem()
    {
        // Find or create DamageNumberController
        DamageNumberController controller = FindFirstObjectByType<DamageNumberController>();
        if (controller == null)
        {
            GameObject controllerObj = new GameObject("DamageNumberController");
            controller = controllerObj.AddComponent<DamageNumberController>();
            Debug.Log("Created DamageNumberController");
        }

        // Create damage number parent
        Transform damageParent = controller.transform.Find("DamageNumbers");
        if (damageParent == null)
        {
            GameObject damageParentObj = new GameObject("DamageNumbers");
            damageParentObj.transform.SetParent(controller.transform);
            controller.damageNumberParent = damageParentObj.transform;
            Debug.Log("Created DamageNumbers parent");
        }

        // Load damage number prefab
        GameObject damageNumberPrefab = Resources.Load<GameObject>("DamageNumber");
        if (damageNumberPrefab == null)
        {
            Debug.LogError("DamageNumber prefab not found in Resources folder!");
            return;
        }

        controller.damageNumberPrefab = damageNumberPrefab;
        Debug.Log("Assigned DamageNumber prefab to controller");

        // Select the controller in the hierarchy
        Selection.activeGameObject = controller.gameObject;
        EditorGUIUtility.PingObject(controller.gameObject);

        Debug.Log("Damage Number System setup complete!");
    }
}
#endif
