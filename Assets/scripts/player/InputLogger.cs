using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class InputEvent
{
    public float timestamp;
    public string inputType;
    public bool isPressed; // true for press, false for release
    public Vector3 playerPosition;
    public float gameTime;
    
    public InputEvent(float time, string input, bool pressed, Vector3 position, float gameTime)
    {
        timestamp = time;
        inputType = input;
        isPressed = pressed;
        playerPosition = position;
        this.gameTime = gameTime;
    }
    
    // Convert to CSV format
    public string ToCsv()
    {
        return $"{timestamp:F3},{inputType},{isPressed},{playerPosition.x:F3},{playerPosition.y:F3},{playerPosition.z:F3},{gameTime:F3}";
    }
    
    // Convert to JSON-like format
    public string ToJson()
    {
        return $"{{\"timestamp\":{timestamp:F3},\"input\":\"{inputType}\",\"pressed\":{isPressed.ToString().ToLower()},\"position\":[{playerPosition.x:F3},{playerPosition.y:F3},{playerPosition.z:F3}],\"gameTime\":{gameTime:F3}}}";
    }
}

public class InputLogger : MonoBehaviour
{
    [Header("Logging Settings")]
    public bool enableLogging = true;
    public bool logToFile = true;
    public bool logToConsole = false;
    public string logFileName = "input_log";
    public LogFormat logFormat = LogFormat.CSV;
    
    [Header("What to Log")]
    public bool logMovementInputs = true;
    public bool logPositionChanges = true;
    public bool logOnlyInputChanges = true; // Only log press/release, not continuous holding
    
    [Header("File Settings")]
    public bool timestampFileName = true;
    public bool includeSessionInfo = true;
    
    public enum LogFormat
    {
        CSV,
        JSON,
        TXT
    }
    
    private List<InputEvent> inputEvents = new List<InputEvent>();
    private string logFilePath;
    private float sessionStartTime;
    private string sessionId;
    
    // Track previous input states to detect changes
    private bool prevW, prevA, prevS, prevD;
    private bool prevUp, prevDown, prevLeft, prevRight;
    
    // Reference to player for position logging
    private Transform playerTransform;
    
    void Start()
    {
        if (!enableLogging) return;
        
        // Find player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Initialize session
        sessionStartTime = Time.time;
        sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        // Create log file path
        CreateLogFile();
        
        Debug.Log($"📝 Input logging started. File: {logFilePath}");
    }
    
    void Update()
    {
        if (!enableLogging) return;
        
        LogInputs();
    }
    
    private void LogInputs()
    {
        float currentTime = Time.time - sessionStartTime;
        Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;
        float gameTime = Time.time;
        
        if (logMovementInputs)
        {
            // WASD Keys
            CheckInputChange("W", KeyCode.W, ref prevW, currentTime, playerPos, gameTime);
            CheckInputChange("A", KeyCode.A, ref prevA, currentTime, playerPos, gameTime);
            CheckInputChange("S", KeyCode.S, ref prevS, currentTime, playerPos, gameTime);
            CheckInputChange("D", KeyCode.D, ref prevD, currentTime, playerPos, gameTime);
            
            // Arrow Keys
            CheckInputChange("Up", KeyCode.UpArrow, ref prevUp, currentTime, playerPos, gameTime);
            CheckInputChange("Down", KeyCode.DownArrow, ref prevDown, currentTime, playerPos, gameTime);
            CheckInputChange("Left", KeyCode.LeftArrow, ref prevLeft, currentTime, playerPos, gameTime);
            CheckInputChange("Right", KeyCode.RightArrow, ref prevRight, currentTime, playerPos, gameTime);
        }
    }
    
    private void CheckInputChange(string inputName, KeyCode key, ref bool prevState, float time, Vector3 pos, float gameTime)
    {
        bool currentState = Input.GetKey(key);
        
        if (logOnlyInputChanges)
        {
            // Only log when state changes
            if (currentState != prevState)
            {
                LogInput(inputName, currentState, time, pos, gameTime);
                prevState = currentState;
            }
        }
        else
        {
            // Log every frame while pressed
            if (currentState)
            {
                LogInput(inputName, true, time, pos, gameTime);
            }
        }
    }
    
    private void LogInput(string inputType, bool isPressed, float timestamp, Vector3 position, float gameTime)
    {
        InputEvent inputEvent = new InputEvent(timestamp, inputType, isPressed, position, gameTime);
        inputEvents.Add(inputEvent);
        
        if (logToConsole)
        {
            Debug.Log($"🎮 {inputType} {(isPressed ? "PRESSED" : "RELEASED")} at {timestamp:F3}s | Pos: {position}");
        }
        
        if (logToFile)
        {
            WriteToFile(inputEvent);
        }
    }
    
    private void CreateLogFile()
    {
        string fileName = logFileName;
        
        if (timestampFileName)
        {
            fileName += $"_{sessionId}";
        }
        
        string extension = logFormat switch
        {
            LogFormat.CSV => ".csv",
            LogFormat.JSON => ".json",
            LogFormat.TXT => ".txt",
            _ => ".txt"
        };
        
        fileName += extension;
        
        // Create logs directory if it doesn't exist
        string logsDir = Path.Combine(Application.persistentDataPath, "InputLogs");
        if (!Directory.Exists(logsDir))
        {
            Directory.CreateDirectory(logsDir);
        }
        
        logFilePath = Path.Combine(logsDir, fileName);
        
        // Write header
        if (logFormat == LogFormat.CSV)
        {
            WriteHeader();
        }
        else if (logFormat == LogFormat.JSON)
        {
            File.WriteAllText(logFilePath, "[\n");
        }
        
        if (includeSessionInfo)
        {
            WriteSessionInfo();
        }
    }
    
    private void WriteHeader()
    {
        if (logFormat == LogFormat.CSV)
        {
            string header = "Timestamp,Input,Pressed,PosX,PosY,PosZ,GameTime\n";
            File.WriteAllText(logFilePath, header);
        }
    }
    
    private void WriteSessionInfo()
    {
        string sessionInfo = $"# Session: {sessionId}\n" +
                           $"# Start Time: {DateTime.Now}\n" +
                           $"# Unity Version: {Application.unityVersion}\n" +
                           $"# Game: {Application.productName}\n" +
                           $"# Platform: {Application.platform}\n" +
                           "# ===================================\n";
        
        if (logFormat == LogFormat.CSV || logFormat == LogFormat.TXT)
        {
            File.AppendAllText(logFilePath, sessionInfo);
        }
    }
    
    private void WriteToFile(InputEvent inputEvent)
    {
        try
        {
            string logLine = logFormat switch
            {
                LogFormat.CSV => inputEvent.ToCsv() + "\n",
                LogFormat.JSON => (inputEvents.Count > 1 ? "," : "") + inputEvent.ToJson() + "\n",
                LogFormat.TXT => $"[{inputEvent.timestamp:F3}] {inputEvent.inputType} {(inputEvent.isPressed ? "PRESS" : "RELEASE")} at {inputEvent.playerPosition}\n",
                _ => inputEvent.ToString() + "\n"
            };
            
            File.AppendAllText(logFilePath, logLine);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to write to log file: {e.Message}");
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveSession();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveSession();
        }
    }
    
    void OnDestroy()
    {
        SaveSession();
    }
    
    private void SaveSession()
    {
        if (!enableLogging || inputEvents.Count == 0) return;
        
        if (logFormat == LogFormat.JSON)
        {
            File.AppendAllText(logFilePath, "]");
        }
        
        // Write summary
        string summary = $"\n# Session Summary:\n" +
                        $"# Total Events: {inputEvents.Count}\n" +
                        $"# Session Duration: {Time.time - sessionStartTime:F2}s\n" +
                        $"# End Time: {DateTime.Now}\n";
        
        File.AppendAllText(logFilePath, summary);
        
        Debug.Log($"📁 Input log saved: {logFilePath} ({inputEvents.Count} events)");
    }
    
    // Public methods for external access
    public List<InputEvent> GetInputEvents() => new List<InputEvent>(inputEvents);
    
    public void ClearLog()
    {
        inputEvents.Clear();
        if (File.Exists(logFilePath))
        {
            CreateLogFile();
        }
    }
    
    public string GetLogFilePath() => logFilePath;
    
    [ContextMenu("Open Log Folder")]
    public void OpenLogFolder()
    {
        string logsDir = Path.Combine(Application.persistentDataPath, "InputLogs");
        Application.OpenURL("file://" + logsDir);
    }
    
    [ContextMenu("Print Log Stats")]
    public void PrintLogStats()
    {
        Debug.Log($"📊 Input Log Stats:\n" +
                 $"Total Events: {inputEvents.Count}\n" +
                 $"Session Duration: {Time.time - sessionStartTime:F2}s\n" +
                 $"Log File: {logFilePath}");
    }
}