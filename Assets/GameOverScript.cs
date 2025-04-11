using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameOverScript : MonoBehaviour
{
    public TextMeshProUGUI topperScore;
    public TextMeshProUGUI secondScore;
    public TextMeshProUGUI thirdScore;
    public  GameObject collisionEffectPrefab;

    public  GameObject CollisionEffectPrefab
    {
        get { return collisionEffectPrefab; }
        set { collisionEffectPrefab = value; }
    }

    public void Update()
    {
        // Create a list of all scores with labels
        List<(string name, float score)> scores = new List<(string, float)>
        {
            ("Player", GameData.PlayerScore),
            ("Enemy1", GameData.Enemy1Score),
            ("Enemy2", GameData.Enemy2Score),
            ("Enemy3", GameData.Enemy3Score),
            ("Enemy4", GameData.Enemy4Score)
        };

        // Sort by score descending
        var top3 = scores.OrderByDescending(s => s.score).Take(3).ToList();

        // Display top 3 with labels and % (2 decimal places)
        topperScore.text = $"{top3[0].score:F2}%";
        secondScore.text = $"{top3[1].score:F2}%";
        thirdScore.text = $"{top3[2].score:F2}%";
    }
}
