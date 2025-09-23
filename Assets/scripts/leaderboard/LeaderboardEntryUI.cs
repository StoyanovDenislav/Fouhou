/*using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI scoreText;
    public Image backgroundImage;
    public Image rankIcon;
    
    [Header("Styling")]
    public Color normalBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color topThreeBackgroundColor = new Color(0.3f, 0.25f, 0.1f, 0.9f);
    public Color unrankedBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
    public Color placeholderBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
    
    [Header("Rank Icons (Optional)")]
    public Sprite goldMedalSprite;
    public Sprite silverMedalSprite;
    public Sprite bronzeMedalSprite;

    public void SetupEntry(LeaderboardEntry entry, int displayRank)
    {
        // Set rank
        if (rankText)
        {
            if (entry.isRanked && entry.ranking > 0)
            {
                rankText.text = $"#{entry.ranking}";
                rankText.color = Color.white;
            }
            else
            {
                rankText.text = "—";
                rankText.color = Color.gray;
            }
        }

        // Set username
        if (usernameText)
        {
            usernameText.text = entry.username;
            usernameText.color = entry.isRanked ? Color.white : Color.gray;
        }

        // Set score
        if (scoreText)
        {
            scoreText.text = FormatScore(entry.score);
            scoreText.color = entry.isRanked ? Color.white : Color.gray;
        }

        // Set background and icon
        SetBackgroundColor(entry);
        SetRankIcon(entry);

        Debug.Log($"🏆 Setup entry: {entry.username} - Rank {entry.ranking} - Score {entry.score}");
    }
    
    public void SetupPlaceholder(int position)
    {
        // Set placeholder data
        if (rankText)
        {
            rankText.text = $"#{position}";
            rankText.color = Color.gray;
        }

        if (usernameText)
        {
            usernameText.text = "---";
            usernameText.color = Color.gray;
        }

        if (scoreText)
        {
            scoreText.text = "---";
            scoreText.color = Color.gray;
        }

        // Set placeholder styling
        if (backgroundImage)
        {
            backgroundImage.color = placeholderBackgroundColor;
        }

        if (rankIcon)
        {
            rankIcon.gameObject.SetActive(false);
        }

        Debug.Log($"📋 Setup placeholder for position {position}");
    }
    
    void SetBackgroundColor(LeaderboardEntry entry)
    {
        if (backgroundImage)
        {
            if (!entry.isRanked)
            {
                backgroundImage.color = unrankedBackgroundColor;
            }
            else if (entry.ranking <= 3)
            {
                backgroundImage.color = topThreeBackgroundColor;
            }
            else
            {
                backgroundImage.color = normalBackgroundColor;
            }
        }
    }
    
    void SetRankIcon(LeaderboardEntry entry)
    {
        if (rankIcon && entry.isRanked)
        {
            switch (entry.ranking)
            {
                case 1:
                    rankIcon.sprite = goldMedalSprite;
                    rankIcon.gameObject.SetActive(goldMedalSprite != null);
                    break;
                case 2:
                    rankIcon.sprite = silverMedalSprite;
                    rankIcon.gameObject.SetActive(silverMedalSprite != null);
                    break;
                case 3:
                    rankIcon.sprite = bronzeMedalSprite;
                    rankIcon.gameObject.SetActive(bronzeMedalSprite != null);
                    break;
                default:
                    rankIcon.gameObject.SetActive(false);
                    break;
            }
        }
        else if (rankIcon)
        {
            rankIcon.gameObject.SetActive(false);
        }
    }

    string FormatScore(int score)
    {
        if (score >= 1000000)
        {
            return $"{score / 1000000f:F1}M";
        }
        else if (score >= 1000)
        {
            return $"{score / 1000f:F1}K";
        }
        else
        {
            return score.ToString("N0");
        }
    }
}*/