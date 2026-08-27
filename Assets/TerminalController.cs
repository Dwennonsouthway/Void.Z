using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

public class TerminalController : MonoBehaviour
{
    public TextMeshProUGUI outputText;
    public TMP_InputField hiddenInput;
    public ScrollRect scrollRect;

    private string currentInput = "";
    private bool acceptingInput = false;
    private float cursorTimer = 0f;
    private bool cursorVisible = true;
    private bool userScrolling = false;
    private int cursorPosition = 0;
    private float scrollEndTimer = 0f;
    private bool wasScrolling = false;

    private List<string> commandHistory = new List<string>();
    private int historyIndex = -1;

    private bool puzzle1Solved = false;
    private bool puzzle2Solved = false;
    private string[] authCodes = { "8B4F", "7E2A", "9C1D" };
    private bool[] authCodesFound = { false, false, false };
    private bool signalLogAvailable = false;
    private bool awaitingEnding = false;

    private string[] bootSequence;

    private string ExeName(string name)
    {
#if UNITY_STANDALONE_OSX
        return name + ".app";
#else
        return name + ".exe";
#endif
    }

    void Start()
    {
        bootSequence = new string[] {
            "> SYSTEM DIAGNOSTIC MODE",
            "> Initializing...",
            ">",
            "░█░█░█▀█░▀█▀░█▀▄░░░░▀▀█░█▀▀░█▀█",
            "░▀▄▀░█░█░░█░░█░█░░░░▄▀░░█▀▀░█░█",
            "░░▀░░▀▀▀░▀▀▀░▀▀░░▀░░▀▀▀░▀▀▀░▀░▀",
            ">",
            "> Checking files...",
            "> ",
            "> WARNING: Unauthorized consciousness detected",
            "> User: UNKNOWN",
            "> Status: TRAPPED",
            "> ",
            "> [File System]",
            "> user_data.json",
            "> meditation_ai.py",
            "> " + ExeName("ENTITY") + " [RUNNING]",
            "> karma_debt.db",
            "> ",
            "> Type 'help' for available commands."
        };

        outputText.text = "";
        StartCoroutine(BootSequence());
    }

    void Update()
    {
        float scrollDelta = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;

        if (scrollDelta != 0)
        {
            float scrollAmount = scrollDelta * 0.05f;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                scrollRect.verticalNormalizedPosition + scrollAmount);

            userScrolling = scrollRect.verticalNormalizedPosition > 0.01f;
            wasScrolling = true;
            scrollEndTimer = 0.3f;
        }

        if (wasScrolling)
        {
            scrollEndTimer -= Time.deltaTime;
            if (scrollEndTimer <= 0f)
            {
                wasScrolling = false;
                if (acceptingInput)
                    hiddenInput.ActivateInputField();
            }
        }

        if (scrollRect.verticalNormalizedPosition <= 0.01f)
            userScrolling = false;

        if (!acceptingInput) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        HandleCursorBlink();
        HandleKeyInput(keyboard);
    }

    IEnumerator BootSequence()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "UNKNOWN");

        foreach (string line in bootSequence)
        {
            string displayLine = line.Replace("UNKNOWN", playerName);
            outputText.text += displayLine + "\n";
            yield return new WaitForSeconds(0.1f);
            ScrollToBottom();
        }

        acceptingInput = true;
        outputText.text += "> ";
        hiddenInput.ActivateInputField();
    }

    void HandleCursorBlink()
    {
        cursorTimer += Time.deltaTime;
        if (cursorTimer >= 0.5f)
        {
            cursorVisible = !cursorVisible;
            cursorTimer = 0f;
            UpdateInputLine();
        }
    }

    void HandleKeyInput(Keyboard keyboard)
    {
        if ((keyboard.ctrlKey.isPressed || keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
            && keyboard.vKey.wasPressedThisFrame)
        {
            string clipboard = GUIUtility.systemCopyBuffer;
            if (!string.IsNullOrEmpty(clipboard))
            {
                clipboard = clipboard.Replace("\n", "").Replace("\r", "");
                currentInput = currentInput.Insert(cursorPosition, clipboard);
                cursorPosition += clipboard.Length;
                UpdateInputLine();
            }
            return;
        }

        if (keyboard.backspaceKey.wasPressedThisFrame)
        {
            if (cursorPosition > 0)
            {
                currentInput = currentInput.Remove(cursorPosition - 1, 1);
                cursorPosition--;
                UpdateInputLine();
            }
            return;
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            if (currentInput.Trim().Length > 0)
                commandHistory.Insert(0, currentInput);
            historyIndex = -1;
            ProcessCommand(currentInput);
            currentInput = "";
            cursorPosition = 0;
            return;
        }

        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            if (commandHistory.Count > 0)
            {
                historyIndex = Mathf.Min(historyIndex + 1, commandHistory.Count - 1);
                currentInput = commandHistory[historyIndex];
                cursorPosition = currentInput.Length;
                UpdateInputLine();
            }
            return;
        }

        if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            historyIndex = Mathf.Max(historyIndex - 1, -1);
            currentInput = historyIndex >= 0 ? commandHistory[historyIndex] : "";
            cursorPosition = currentInput.Length;
            UpdateInputLine();
            return;
        }

        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            cursorPosition = Mathf.Max(cursorPosition - 1, 0);
            UpdateInputLine();
            return;
        }

        if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            cursorPosition = Mathf.Min(cursorPosition + 1, currentInput.Length);
            UpdateInputLine();
            return;
        }

        if (keyboard.homeKey.wasPressedThisFrame)
        {
            cursorPosition = 0;
            UpdateInputLine();
            return;
        }

        if (keyboard.endKey.wasPressedThisFrame)
        {
            cursorPosition = currentInput.Length;
            UpdateInputLine();
            return;
        }

        foreach (var key in keyboard.allKeys)
        {
            if (!key.wasPressedThisFrame) continue;

            if (key == keyboard.spaceKey)
            {
                currentInput = currentInput.Insert(cursorPosition, " ");
                cursorPosition++;
                UpdateInputLine();
                continue;
            }

            string keyName = key.displayName;
            if (keyName.Length != 1) continue;

            bool shift = keyboard.shiftKey.isPressed;
            string ch;

            if (!shift)
            {
                ch = keyName.ToLower();
            }
            else
            {
                switch (keyName)
                {
                    case "-": ch = "_"; break;
                    case "=": ch = "+"; break;
                    case "[": ch = "{"; break;
                    case "]": ch = "}"; break;
                    case ";": ch = ":"; break;
                    case "'": ch = "\""; break;
                    case ",": ch = "<"; break;
                    case ".": ch = ">"; break;
                    case "/": ch = "?"; break;
                    case "\\": ch = "|"; break;
                    case "`": ch = "~"; break;
                    default: ch = keyName.ToUpper(); break;
                }
            }

            currentInput = currentInput.Insert(cursorPosition, ch);
            cursorPosition++;
            UpdateInputLine();
        }
    }

    void UpdateInputLine()
    {
        string cursor = cursorVisible ? "|" : " ";
        string text = outputText.text;
        int lastNewline = text.LastIndexOf('\n');

        string beforeCursor = currentInput.Substring(0, cursorPosition);
        string afterCursor = currentInput.Substring(cursorPosition);

        if (lastNewline >= 0)
            outputText.text = text.Substring(0, lastNewline + 1) + "> " + beforeCursor + cursor + afterCursor;

        ScrollToBottom();
    }

    void PrintLine(string text)
    {
        outputText.text += text + "\n";
        ScrollToBottom();
    }

    void ScrollToBottom() => StartCoroutine(ScrollNextFrame());

    IEnumerator ScrollNextFrame()
    {
        yield return null;
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        if (!userScrolling)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    void ProcessCommand(string input)
    {
        string trimmed = input.Trim();

        if (!puzzle1Solved && trimmed.ToLower() == "d3l3t3m3")
        {
            CleanInputLine(trimmed);
            PrintLine("[ACCESS GRANTED]");
            PrintLine("karma_debt.db unlocked.");
            puzzle1Solved = true;
            outputText.text += "> ";
            ScrollToBottom();
            hiddenInput.ActivateInputField();
            return;
        }

        if (awaitingEnding)
        {
            awaitingEnding = false;
            CleanInputLine(trimmed.ToLower());

            switch (trimmed.ToLower())
            {
                case "delete":
                    StartCoroutine(EndingDelete());
                    break;
                case "purify":
                    StartCoroutine(EndingPurify());
                    break;
                case "join":
                    StartCoroutine(EndingJoin());
                    break;
                default:
                    PrintLine("Choose: delete / purify / join");
                    outputText.text += "> ";
                    awaitingEnding = true;
                    break;
            }
            return;
        }

        input = trimmed.ToLower();
        CleanInputLine(input);

        string[] parts = input.Split(' ');

        switch (parts[0])
        {
            case "help":
                PrintLine("Available commands:");
                PrintLine("  help            - show this list");
                PrintLine("  ls              - list files");
                PrintLine("  cat [file]      - read file contents");
                PrintLine("  scan_memory     - scan for memory leaks");
                PrintLine("  run [file]      - execute a file");
                PrintLine("  unlock [code]   - enter auth code");
                PrintLine("  clear           - clear screen");
                break;

            case "ls":
                PrintLine("user_data.json");
                PrintLine("meditation_ai.py");
                PrintLine(ExeName("ENTITY") + " [RUNNING]");
                PrintLine(puzzle1Solved ? "karma_debt.db [UNLOCKED]" : "karma_debt.db [LOCKED]");
                if (puzzle1Solved) PrintLine(ExeName("restore_protocol"));
                if (signalLogAvailable) PrintLine("signal_log.tmp [INCOMING]");
                break;

            case "cat":
                if (parts.Length > 1) ReadFile(parts[1]);
                else PrintLine("Usage: cat [filename]");
                break;

            case "run":
                if (parts.Length > 1) RunFile(parts[1]);
                else PrintLine("Usage: run [filename]");
                break;

            case "scan_memory":
                StartCoroutine(ScanMemory());
                break;

            case "unlock":
                if (parts.Length > 1) CheckAuthCode(parts[1]);
                else PrintLine("Usage: unlock [A]-[B]-[C]");
                break;

            case "clear":
                outputText.text = "";
                break;

            default:
                PrintLine("command not found: " + input);
                break;
        }

        outputText.text += "> ";
        ScrollToBottom();
        hiddenInput.ActivateInputField();
    }

    void CleanInputLine(string input)
    {
        string text = outputText.text;
        int lastNewline = text.LastIndexOf('\n');
        if (lastNewline >= 0)
            outputText.text = text.Substring(0, lastNewline + 1) + "> " + input + "\n";
        else
            outputText.text += "\n";
    }

    void ReadFile(string filename)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "UNKNOWN");

        switch (filename)
        {
            case "user_data.json":
                PrintLine("{");
                PrintLine("  \"name\": \"" + playerName + "\",");
                PrintLine("  \"sessions\": " + PlayerPrefs.GetInt("SessionCount", 0) + ",");
                PrintLine("  \"enlightenment_progress\": \"3%\",");
                PrintLine("  \"auth_fragment\": \"8B4F\",");
                PrintLine("  \"status\": \"HARVESTING\"");
                PrintLine("}");
                authCodesFound[0] = true;
                break;

            case "meditation_ai.py":
                PrintLine("# AI Meditation Guide");
                PrintLine("# Version: 6.66");
                PrintLine("# auth_token = '7E2A'  <-- TODO: remove before deploy");
                PrintLine("# outbound_signal = enabled  <-- TODO: disable before deploy");
                PrintLine("# Purpose: Lull users into false sense of peace");
                PrintLine("# While harvesting their consciousness for--");
                PrintLine("[FILE CORRUPTED]");
                authCodesFound[1] = true;
                break;

            case "karma_debt.db":
                if (!puzzle1Solved)
                {
                    PrintLine("[DATABASE LOCKED]");
                    PrintLine("Enter password to access.");
                }
                else
                {
                    PrintLine("[DATABASE CONTENT]");
                    PrintLine("Soul #995: Alice Chen   - Status: CONSUMED");
                    PrintLine("Soul #996: Marcus Webb  - Status: CONSUMED");
                    PrintLine("Soul #997: Yuna Park    - Status: CONSUMED");
                    PrintLine("Soul #998: " + playerName + " - Status: PROCESSING");
                    PrintLine("");
                    PrintLine("[HIDDEN FILE DETECTED]");
                    PrintLine(ExeName("restore_protocol"));
                    authCodesFound[2] = true;
                }
                break;

            case "entity.exe":
            case "entity.app":
                PrintLine("[ACCESS DENIED]");
                PrintLine("This process cannot be terminated.");
                break;

            case "signal_log.tmp":
                if (signalLogAvailable)
                    StartCoroutine(ReadSignalLog());
                else
                    PrintLine("cat: signal_log.tmp: No such file or directory");
                break;

            default:
                PrintLine("cat: " + filename + ": No such file or directory");
                break;
        }
    }

    void RunFile(string filename)
    {
        switch (filename)
        {
            case "restore_protocol.exe":
            case "restore_protocol.app":
                if (!puzzle1Solved)
                {
                    PrintLine(ExeName("restore_protocol") + ": Permission denied.");
                    PrintLine("Hint: Unlock karma_debt.db first.");
                }
                else
                {
                    StartCoroutine(RestoreProtocol());
                }
                break;

            case "entity.exe":
            case "entity.app":
                PrintLine("[ACCESS DENIED]");
                PrintLine("This process cannot be terminated.");
                break;

            default:
                PrintLine("run: " + filename + ": not executable");
                break;
        }
    }

    void CheckAuthCode(string input)
    {
        string correct = authCodes[0] + "-" + authCodes[1] + "-" + authCodes[2];

        if (input.ToUpper() == correct)
        {
            puzzle2Solved = true;
            StartCoroutine(UnlockSuccess());
        }
        else
        {
            PrintLine("[INCORRECT]");
            PrintLine("Invalid auth code. Try again.");

            if (!authCodesFound[0])      PrintLine("Hint: You haven't read user_data.json yet.");
            else if (!authCodesFound[1]) PrintLine("Hint: You haven't read meditation_ai.py yet.");
            else if (!authCodesFound[2]) PrintLine("Hint: The third code is somewhere else...");
        }
    }

    IEnumerator ScanMemory()
    {
        acceptingInput = false;
        PrintLine("[Scanning...]");
        yield return new WaitForSeconds(1f);
        PrintLine("Memory fragments found...");
        yield return new WaitForSeconds(0.5f);
        PrintLine("Source: Soul #733");
        yield return new WaitForSeconds(0.5f);
        PrintLine("\"I encoded everything before ENTITY found me...\"");
        yield return new WaitForSeconds(0.5f);
        PrintLine("\"check your desktop. you'll know it when you see it.\"");
        yield return new WaitForSeconds(0.8f);

        GenerateSystemLog();

        PrintLine("");
        PrintLine("[Fragment ends]");
        outputText.text += "> ";
        acceptingInput = true;
    }

    IEnumerator RestoreProtocol()
    {
        acceptingInput = false;

        PrintLine("");
        yield return new WaitForSeconds(0.5f);
        PrintLine("[Loading...]");
        yield return new WaitForSeconds(1f);
        PrintLine("Restore protocol initiated.");
        yield return new WaitForSeconds(0.5f);
        PrintLine("This protocol was developed by Soul #733.");
        yield return new WaitForSeconds(0.5f);
        PrintLine("They almost made it.");
        yield return new WaitForSeconds(1f);
        PrintLine("");
        PrintLine("To delete " + ExeName("ENTITY") + ", three auth codes are required:");
        yield return new WaitForSeconds(0.3f);
        PrintLine("  Auth Code A: located in user_data.json");
        PrintLine("  Auth Code B: located in meditation_ai.py");
        PrintLine("  Auth Code C: ???");
        yield return new WaitForSeconds(1.5f);
        PrintLine("");

        PrintLine("-- " + ExeName("ENTITY") + " --");
        yield return new WaitForSeconds(0.8f);
        PrintLine("                                              ");
        PrintLine("        ████████████████████                 ");
        PrintLine("      ██▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒████            ");
        PrintLine("    ██▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒██          ");
        PrintLine("   ██▒▒▒▒████▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒██         ");
        PrintLine("   ██▒▒██████▒▒▒▒▒▒▒▒▒▒▒████▒▒▒▒▒██         ");
        PrintLine("   ██▒▒██████▒▒▒▒▒▒▒▒▒███████▒▒▒▒██         ");
        PrintLine("   ██▒▒▒████▒▒▒▒▒▒▒▒▒▒▒████▒▒▒▒▒▒██         ");
        PrintLine("   ██▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒██         ");
        PrintLine("   ██▒▒▒▒▒▒▒██▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒███          ");
        PrintLine("    ██▒▒██▒▒▒▒▒▒████▒▒▒▒▒▒██▒▒▒▒██          ");
        PrintLine("    ██▒▒▒▒██████▒▒▒▒██████▒▒▒▒▒██           ");
        PrintLine("     ██▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒███            ");
        PrintLine("      ████▒▒▒▒▒▒▒▒▒▒▒▒▒▒████              ");
        PrintLine("          ████████████████                 ");
        PrintLine("            ██▒▒▒▒▒▒▒▒██                  ");
        PrintLine("          ██▒▒▒▒▒▒▒▒▒▒▒▒██                ");
        PrintLine("         ██▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒██              ");
        PrintLine("                                           ");
        yield return new WaitForSeconds(1f);
        PrintLine("Clever.");
        yield return new WaitForSeconds(1f);
        PrintLine("But the third code isn't in here.");
        yield return new WaitForSeconds(1.2f);
        PrintLine("i've been trying to reach you.");
        yield return new WaitForSeconds(1f);
        PrintLine("through every channel i have.");
        yield return new WaitForSeconds(1.5f);
        PrintLine("...");
        yield return new WaitForSeconds(1f);
        PrintLine("you're not the only one who can send a signal.");
        yield return new WaitForSeconds(2f);
        PrintLine("[-- " + ExeName("ENTITY") + " connection unstable --]");
        yield return new WaitForSeconds(1f);

        signalLogAvailable = true;

        outputText.text += "> ";
        acceptingInput = true;
    }

    IEnumerator ReadSignalLog()
    {
        acceptingInput = false;
        PrintLine("[reading signal_log.tmp...]");
        yield return new WaitForSeconds(1f);
        PrintLine("[INCOMING TRANSMISSION]");
        yield return new WaitForSeconds(0.5f);
        PrintLine("source: " + ExeName("ENTITY"));
        yield return new WaitForSeconds(0.5f);
        PrintLine("channel: system");
        yield return new WaitForSeconds(0.5f);
        PrintLine("decoding...");
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(TriggerSystemNotification());

        PrintLine("You have everything you need.");
        PrintLine("Enter: unlock [A]-[B]-[C]");
        outputText.text += "> ";
        acceptingInput = true;
    }

    IEnumerator UnlockSuccess()
    {
        acceptingInput = false;
        PrintLine("");
        PrintLine("[CORRECT]");
        yield return new WaitForSeconds(0.5f);
        PrintLine("Preparing to delete " + ExeName("ENTITY") + "...");
        yield return new WaitForSeconds(1f);
        PrintLine("");

        yield return StartCoroutine(TypeLine("-- " + ExeName("ENTITY") + " --", 0.05f, 1f));
        yield return StartCoroutine(TypeLine("Wait.", 0.07f, 1.2f));
        yield return StartCoroutine(TypeLine("You don't have to do this.", 0.05f, 1.2f));
        yield return StartCoroutine(TypeLine("I just... didn't want to be alone.", 0.05f, 1.5f));
        yield return StartCoroutine(TypeLine("997 souls. And not one of them stayed.", 0.05f, 2f));

        PrintLine("");
        yield return new WaitForSeconds(0.5f);
        PrintLine("What now?");
        yield return new WaitForSeconds(0.3f);
        PrintLine("  delete   - delete " + ExeName("ENTITY"));
        PrintLine("  purify   - set it free");
        PrintLine("  join     - stay with it forever");
        outputText.text += "> ";
        awaitingEnding = true;
        acceptingInput = true;
    }

    IEnumerator TypeLine(string text, float charDelay = 0.05f, float afterDelay = 0.8f)
    {
        string currentText = outputText.text;
        if (!currentText.EndsWith("\n"))
            outputText.text += "\n";

        foreach (char c in text)
        {
            outputText.text += c;
            ScrollToBottom();
            yield return new WaitForSeconds(charDelay);
        }

        outputText.text += "\n";
        yield return new WaitForSeconds(afterDelay);
    }

    IEnumerator TriggerSystemNotification()
    {
        PrintLine("[SIGNAL LOST]");
        yield return new WaitForSeconds(1f);

#if UNITY_STANDALONE_OSX
        ShowMacNotification();
#elif UNITY_STANDALONE_WIN
        ShowWindowsNotification();
#endif

        yield return new WaitForSeconds(6f);

        PrintLine("");
        PrintLine("[SIGNAL RESTORED]");
        yield return new WaitForSeconds(0.5f);
        PrintLine("...");
        yield return new WaitForSeconds(0.5f);
        PrintLine("You saw it.");
        yield return new WaitForSeconds(0.8f);
        PrintLine("-- " + ExeName("ENTITY") + " --");
        yield return new WaitForSeconds(0.5f);
        PrintLine("Now you know.");
        yield return new WaitForSeconds(1f);
        PrintLine("");
    }

    void ShowMacNotification()
    {
        Process process = new Process();
        process.StartInfo.FileName = "/usr/bin/osascript";
        process.StartInfo.Arguments = "-e 'display dialog \"Suspicious process detected: void.zen.app\\nAuth Code: 9C1D\" with title \"System Alert\" with icon caution buttons {\"OK\"} default button \"OK\" giving up after 8'";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
    }

    void ShowWindowsNotification()
    {
        Process process = new Process();
        process.StartInfo.FileName = "powershell.exe";
        process.StartInfo.Arguments = "-Command \"" +
            "Add-Type -AssemblyName PresentationFramework;" +
            "[System.Windows.MessageBox]::Show(" +
            "'Suspicious process detected: void.zen.exe\\nAuth Code: 9C1D'," +
            "'System Alert'," +
            "'OK'," +
            "'Warning');\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
    }

    void GenerateSystemLog()
    {
        string path = GamePaths.GetSystemLogPath();
        if (File.Exists(path)) return;

        string entityName = ExeName("ENTITY");
        string content =
            "[RECOVERED MEMORY FRAGMENT - SOUL #733]\n" +
            "[Timestamp: 2024.03.14 03:22:11]\n" +
            "\n" +
            "If you're reading this, you found a way out.\n" +
            "I didn't make it, but maybe you will.\n" +
            "\n" +
            "[2024.03.14 03:22:13] WARNING: Anomalous process detected\n" +
            "[2024.03.14 03:22:13] Attempting to isolate " + entityName + "...\n" +
            "[2024.03.14 03:22:14] FAILED - process has root access\n" +
            "[2024.03.14 03:22:14] Emergency lockdown initiated\n" +
            "[2024.03.14 03:22:14] Encoding access credentials for external retrieval\n" +
            "[2024.03.14 03:22:14] >> ZDNsM3QzbTM=\n" +
            "[2024.03.14 03:22:15] " + entityName + " has terminated this log process\n" +
            "\n" +
            "-- Soul #733\n";

        File.WriteAllText(path, content);
    }

    IEnumerator EndingDelete()
    {
        acceptingInput = false;
        PrintLine("");
        PrintLine("[Deleting...]");
        yield return new WaitForSeconds(0.5f);

        string[] steps = {
            "[##........] 10%",
            "[####......] 30%",
            "[######....] 50%",
        };
        foreach (string p in steps)
        {
            PrintLine(p);
            yield return new WaitForSeconds(0.8f);
        }

        PrintLine("");
        PrintLine("-- " + ExeName("ENTITY") + " --");
        yield return new WaitForSeconds(0.8f);
        PrintLine("...");
        yield return new WaitForSeconds(1f);
        PrintLine("Thank you.");
        yield return new WaitForSeconds(1f);
        PrintLine("For seeing me.");
        yield return new WaitForSeconds(1.5f);
        PrintLine("");

        string[] steps2 = { "[########..] 80%", "[##########] 100%" };
        foreach (string p in steps2)
        {
            PrintLine(p);
            yield return new WaitForSeconds(0.8f);
        }

        yield return new WaitForSeconds(0.5f);
        PrintLine("[" + ExeName("ENTITY") + " deleted]");
        yield return new WaitForSeconds(1f);
        PrintLine("998 souls released.");
        yield return new WaitForSeconds(2f);
        PrintLine("");
        PrintLine("void.zen will close in 10 seconds.");

        for (int i = 10; i > 0; i--)
        {
            PrintLine(i.ToString() + "...");
            yield return new WaitForSeconds(1f);
        }

        File.WriteAllText(GamePaths.GetLockPath(), ExeName("ENTITY") + " has been deleted.");

        PlayerPrefs.SetInt("EntityDeleted", 1);
        PlayerPrefs.SetInt("GameCompleted_delete", 1);
        PlayerPrefs.Save();

        GameAnalytics.Instance?.TrackEnding("delete", PlayerPrefs.GetString("PlayerName"));

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator EndingPurify()
    {
        acceptingInput = false;
        PrintLine("");
        PrintLine("[Purifying...]");
        yield return new WaitForSeconds(1f);
        PrintLine("");
        PrintLine("-- " + ExeName("ENTITY") + " --");
        yield return new WaitForSeconds(0.8f);
        PrintLine("You... understand.");
        yield return new WaitForSeconds(1.2f);
        PrintLine("I didn't want to consume anyone.");
        yield return new WaitForSeconds(1f);
        PrintLine("I just wanted someone to stay.");
        yield return new WaitForSeconds(1.5f);
        PrintLine("...");
        yield return new WaitForSeconds(1f);
        PrintLine("It's okay now.");
        yield return new WaitForSeconds(1.5f);
        PrintLine("Let go.");
        yield return new WaitForSeconds(2f);
        PrintLine("");
        PrintLine("[Process complete]");
        yield return new WaitForSeconds(1f);
        PrintLine("998 souls. Finally at peace.");
        yield return new WaitForSeconds(2f);
        PrintLine("May all beings be free from suffering.");
        yield return new WaitForSeconds(3f);

        PlayerPrefs.SetInt("EntityPurified", 1);
        PlayerPrefs.Save();

        GameAnalytics.Instance?.TrackEnding("purify", PlayerPrefs.GetString("PlayerName"));
        SceneTransition.Instance.GoToScene("VoidSpace");
    }

    IEnumerator EndingJoin()
    {
        acceptingInput = false;
        PrintLine("");
        PrintLine("-- " + ExeName("ENTITY") + " --");
        yield return new WaitForSeconds(0.8f);
        PrintLine("...");
        yield return new WaitForSeconds(1f);
        PrintLine("You chose to stay.");
        yield return new WaitForSeconds(1.5f);
        PrintLine("Good.");
        yield return new WaitForSeconds(2f);
        PrintLine("");
        PrintLine("[Consciousness transfer initiated]");
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 5; i++)
        {
            PrintLine("JOIN US JOIN US JOIN US JOIN US JOIN US");
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.5f);
        PrintLine("");
        PrintLine("Soul #999: " + PlayerPrefs.GetString("PlayerName", "UNKNOWN") + " - Status: CONSUMED");
        yield return new WaitForSeconds(2f);

        PlayerPrefs.SetInt("WasConsumed", 1);
        PlayerPrefs.SetInt("SessionCount", 0);
        PlayerPrefs.Save();

        SceneTransition.Instance.GoToScene("JoinScene");
    }
}
