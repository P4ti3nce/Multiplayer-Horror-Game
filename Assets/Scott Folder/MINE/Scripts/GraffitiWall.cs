using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GraffitiWall : MonoBehaviour
{
    public BoxCollider boxCollider;  // Assign your BoxCollider in Inspector
    public TextMeshPro textPrefab;   // Assign your TextTMP prefab in Inspector

    private List<TextMeshPro> spawnedTexts = new List<TextMeshPro>();

    // Spawn numbers given a list of digits, duplicates allowed.
    public void SpawnNumbers(List<int> digitsToSpawn)
    {
        ClearTexts();

        List<Vector3> usedPositions = new List<Vector3>();
        float minDistance = 0.5f; // tweak as needed to prevent overlap

        foreach (int digit in digitsToSpawn)
        {
            Vector3 localPos;
            int attempts = 0;
            const int maxAttempts = 20;

            do
            {
                localPos = GetRandomLocalPositionInCollider();
                attempts++;
            } while (usedPositions.Exists(p => Vector3.Distance(p, localPos) < minDistance) && attempts < maxAttempts);

            usedPositions.Add(localPos);

            Vector3 worldPos = transform.TransformPoint(localPos);

            TextMeshPro newText = Instantiate(textPrefab, worldPos, Quaternion.identity, transform);
            newText.text = digit.ToString();

            float randomTilt = Random.Range(-15f, 15f);
            Quaternion baseRotation = Quaternion.Euler(0f, -90f, 0f);
            Quaternion tiltRotation = Quaternion.Euler(0f, 0f, randomTilt);
            newText.transform.localRotation = baseRotation * tiltRotation;

            spawnedTexts.Add(newText);
        }
    }

    private Vector3 GetRandomLocalPositionInCollider()
    {
        Vector3 size = boxCollider.size;
        Vector3 center = boxCollider.center;

        float y = Random.Range(center.y - size.y / 2f, center.y + size.y / 2f);
        float z = Random.Range(center.z - size.z / 2f, center.z + size.z / 2f);

        // X is 0 because the wall is flat
        return new Vector3(center.x, y, z);
    }



    public void ClearTexts()
    {
        foreach (var text in spawnedTexts)
        {
            if (text != null)
                Destroy(text.gameObject);
        }
        spawnedTexts.Clear();
    }
}
