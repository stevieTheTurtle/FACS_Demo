using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

[Serializable]
public class ActionUnit
{
    [SerializeField] private string name;
    public string Name => name;
    [SerializeField] private int index;
    public int Index => index;
    [SerializeField] private float weight;
    public float Weight => weight;
    
    public void SetWeight(float val)
    {
        weight = val;
    }

    public ActionUnit(string name, int index, float weight)
    {
        this.name = name;
        this.index = index;
        this.weight = weight;
    }
}

[Serializable]
public class Emotion
{
    public string Name { private set; get; }
    public List<ActionUnit> ActionUnits;
    public float Weight;
    
    public void SetWeight(float val)
    {
        Weight = val;
    }
    
    public Emotion(string name, List<ActionUnit> actionUnits)
    {
        Name = name;
        ActionUnits = actionUnits;
    }
}

public class RuntimeAnimator : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    [SerializeField] private List<ActionUnit> actionUnits = new List<ActionUnit>();
    public List<ActionUnit> ActionUnits => actionUnits;
    [SerializeField] private List<Emotion> emotions = new List<Emotion>();
    public List<Emotion> Emotions => emotions;
    
    [Header("Global UI")]
    
    [SerializeField]
    [Tooltip("A global multiplier for the size and spacing of all UI elements.")]
    private float uiScale = 1.0f;
    
    [SerializeField]
    [Tooltip("The height of each individual parameter row.")]
    private float rowHeight = 50f;

    [SerializeField]
    [Tooltip("The width of the label for each parameter.")]
    private float labelWidth = 220f;

    [SerializeField]
    [Tooltip("The spacing between UI elements.")]
    private float spacing = 15f;
    
    [SerializeField]
    [Tooltip("The base font size for the UI text elements.")]
    private int fontSize = 24;

    [Header("Left Runtime UI")]

    [SerializeField]
    [Tooltip("The starting position of the left UI on the screen.")]
    private Vector2 leftUiStartPosition = new Vector2(30, 30);

    [SerializeField]
    [Tooltip("The width of the left UI area.")]
    private float leftUiWidth = 500f;
    
    
    [Header("Right Runtime UI")]

    [SerializeField]
    [Tooltip("The starting position of the right UI on the screen (X is offset from the right edge).")]
    private Vector2 rightUiStartPosition = new Vector2(30, 30);

    [SerializeField]
    [Tooltip("The width of the right UI area.")]
    private float rightUiWidth = 500f;
    
    // Custom GUIStyles for our UI elements
    private GUIStyle labelStyle;
    private GUIStyle sliderStyle;
    private GUIStyle thumbStyle;
    
    // Scroll positions for the UI panels
    private Vector2 leftUiScrollPosition;
    private Vector2 rightUiScrollPosition;


    public void SetActionUnits(List<ActionUnit> newAUs)
    {
        actionUnits = newAUs;
    }

    public void SetEmotions(List<Emotion> newEmotions)
    {
        emotions = newEmotions;
    }
    
    private void Start()
    {
        skinnedMeshRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>();

        //Retrieve actual AU BlendShapes
        for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
        {
            string bsName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
            if (bsName.Contains("AU") && ! (bsName.Contains("_L_") || bsName.Contains("_R_") ))
            {
                MatchCollection matches = Regex.Matches(bsName, "AU_..");
                
                actionUnits.Add(new ActionUnit(matches[0].Value, i, 0.0f));
                Debug.Log($"{matches[0].Value} : {i}");
            }
        }
        
        InitializeEmotions();
    }

    List<ActionUnit> GetAUsByName(string[] auNames)
    {
        List<ActionUnit> foundAUs = new List<ActionUnit>();
        
        foreach (ActionUnit au in actionUnits)
            foreach (string auName in auNames)
                if (au.Name == auName)
                    foundAUs.Add(au);

        if (foundAUs.Count > 0)
            return foundAUs;
        else
            return null;
    }
    
    private void InitializeEmotions()
    {
        emotions.Add(new Emotion("Anger", GetAUsByName(new string[4]{"AU_04", "AU_05", "AU_07", "AU_23"})));
        emotions.Add(new Emotion("Disgust", GetAUsByName(new string[3]{"AU_09", "AU_15", "AU_16"})));
        emotions.Add(new Emotion("Fear",GetAUsByName(new string[7]{"AU_01", "AU_02", "AU_04", "AU_05", "AU_07", "AU_20", "AU_26"})));
        emotions.Add(new Emotion("Happiness", GetAUsByName(new string[2]{"AU_06", "AU_12"})));
        emotions.Add(new Emotion("Sadness", GetAUsByName(new string[3]{"AU_01", "AU_04", "AU_15"})));
        emotions.Add(new Emotion("Surprise",GetAUsByName(new string[4]{"AU_01", "AU_02", "AU_05", "AU_26"})));
    }
    
    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.
    /// </summary>
    void OnGUI()
    {
        // --- Initialize GUIStyles ---
        // This is done in OnGUI as styles can't be initialized in Awake.
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            thumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        }
        
        // Update styles based on the global UI scale
        labelStyle.fontSize = (int)(fontSize * uiScale);
        labelStyle.alignment = TextAnchor.MiddleLeft;
        
        // Make the slider bar bigger
        sliderStyle.fixedHeight = 25 * uiScale;
        sliderStyle.stretchHeight = true;


        // Make the slider handle (thumb) bigger
        thumbStyle.fixedWidth = 35 * uiScale;
        thumbStyle.fixedHeight = 35 * uiScale;

        DrawLeftUI();
        DrawRightUI();
        DrawGlobalResetButton();
    }

    /// <summary>
    /// Draws the UI for the left side of the screen within a scroll view.
    /// </summary>
    private void DrawLeftUI()
    {
        // Define the visible area for the left UI panel.
        float areaHeight = Screen.height - (leftUiStartPosition.y * uiScale * 2);
        float areaWidth = leftUiWidth * uiScale;
        
        GUILayout.BeginArea(new Rect(leftUiStartPosition.x * uiScale, leftUiStartPosition.y * uiScale, areaWidth, areaHeight));

        // Start the scrollable area.
        leftUiScrollPosition = GUILayout.BeginScrollView(leftUiScrollPosition);

        // Loop through all action units and create their UI elements inside the scroll view.
        for (int i = 0; i < actionUnits.Count; i++)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(rowHeight * uiScale));

            // Display the parameter tag
            GUILayout.Label(actionUnits[i].Name, labelStyle, GUILayout.Width(labelWidth * uiScale));

            // Display the slider and update the corresponding value, using the custom styles
            actionUnits[i].SetWeight(GUILayout.HorizontalSlider(actionUnits[i].Weight, 0.0f, 100.0f, sliderStyle, thumbStyle, GUILayout.ExpandWidth(true)));
            skinnedMeshRenderer.SetBlendShapeWeight(actionUnits[i].Index,actionUnits[i].Weight);
            
            // Optional: Display the current slider value
            GUILayout.Label(actionUnits[i].Weight.ToString("F0"), labelStyle, GUILayout.Width(60 * uiScale));

            GUILayout.EndHorizontal();
            GUILayout.Space(spacing * uiScale);
        }
        
        // End the scrollable area.
        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Draws the UI for the right side of the screen within a scroll view.
    /// </summary>
    private void DrawRightUI()
    {
        // Define the visible area for the right UI panel.
        float areaHeight = Screen.height - (rightUiStartPosition.y * uiScale * 2);
        float scaledWidth = rightUiWidth * uiScale;
        float scaledStartX = rightUiStartPosition.x * uiScale;
        float startX = Screen.width - scaledWidth - scaledStartX;
        
        GUILayout.BeginArea(new Rect(startX, rightUiStartPosition.y * uiScale, scaledWidth, areaHeight));
        
        // Start the scrollable area.
        rightUiScrollPosition = GUILayout.BeginScrollView(rightUiScrollPosition);

        foreach(Emotion emotion in emotions)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(rowHeight * uiScale));

            // Display the parameter tag
            GUILayout.Label(emotion.Name, labelStyle, GUILayout.Width(labelWidth * uiScale));

            // Display the slider and update the corresponding value, using the custom styles
            emotion.Weight = GUILayout.HorizontalSlider(emotion.Weight, 0.0f, 100.0f, sliderStyle, thumbStyle, GUILayout.ExpandWidth(true));

            foreach (ActionUnit au in emotion.ActionUnits)
            {
                if (au.Weight < emotion.Weight)
                {
                    au.SetWeight(emotion.Weight);
                    skinnedMeshRenderer.SetBlendShapeWeight(au.Index,au.Weight);
                }
            }
            
            // Optional: Display the current slider value
            GUILayout.Label(emotion.Weight.ToString("F0"), labelStyle, GUILayout.Width(60 * uiScale));

            GUILayout.EndHorizontal();
            GUILayout.Space(spacing * uiScale);
        }
        
        // End the scrollable area.
        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }

    /// <summary>
    /// Draws a single, large reset button at the bottom of the screen.
    /// </summary>
    private void DrawGlobalResetButton()
    {
        float buttonWidth = 250 * uiScale;
        float buttonHeight = 60 * uiScale;
        float buttonX = (Screen.width - buttonWidth) / 2;
        float buttonY = Screen.height - buttonHeight - (30 * uiScale);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = (int)(28 * uiScale);

        if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "Reset All", buttonStyle))
        {
            // Reset left parameters
            for (int i = 0; i < actionUnits.Count; i++)
            {
                actionUnits[i].SetWeight(0.0f);
            }
            // Reset right parameters
            foreach (Emotion emotion in emotions)
            {
                emotion.SetWeight(0.0f);
            }
        }
    }
}
