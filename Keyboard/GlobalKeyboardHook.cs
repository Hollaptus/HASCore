namespace HASCore.Keyboard;

/// Description
/// <summary>
///     Provides a global keyboard hook that monitors key presses and releases across all applications.
/// </summary>
/// <remarks>
///     This class uses a low-level keyboard hook to capture keyboard events system‑wide.
///     It maintains a set of currently pressed keys and raises the <see cref="KeysChanged"/>
///     event whenever the set changes. The hook must be initialized once before use.
/// </remarks>
public static class GlobalKeyboardHook
{
    private static KeyboardHook? Hook;
    private static Boolean Initialized = false;
    private static readonly HashSet<Keys> PressedKeys = [];

    /// Description
    /// <summary>
    ///     Determines whether the specified key is currently being held down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the key is currently pressed; otherwise, <c>false</c>.</returns>
    public static Boolean IsKeyDown(Keys key) => PressedKeys.Contains(key);

    /// Description
    /// <summary>
    ///     Occurs when the set of currently pressed keys changes (i.e., a key is pressed or released).
    /// </summary>
    /// <remarks>
    ///     The event argument is a <see cref="HashSet{Keys}"/> containing all currently pressed keys.
    ///     Subscribers should not modify the set.
    /// </remarks>
    public static event EventHandler<HashSet<Keys>>? KeysChanged;

    /// Description
    /// <summary>
    ///     Initializes the global keyboard hook. This method must be called once before using the hook.
    /// </summary>
    /// <remarks>
    ///     The hook automatically starts capturing keyboard events. It will be stopped when the application exits
    ///     (via <see cref="AppDomain.ProcessExit"/>). If the hook is already initialized, this call does nothing.
    /// </remarks>
    public static void Initialize()
    {
        if (Initialized) return;

        Hook = new KeyboardHook();
        Hook.KeyDown += OnKeyDown;
        Hook.KeyUp += OnKeyUp;
        Hook.Start();
        Initialized = true;

        // Ensure the hook is disposed when the application shuts down.
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Hook?.Dispose();
    }

    /// Description
    /// <summary>
    ///     Stops and disposes the keyboard hook, clearing all internal state.
    /// </summary>
    /// <remarks>
    ///     After calling this method, the hook will no longer capture keyboard events.
    ///     To restart, call <see cref="Initialize"/> again.
    /// </remarks>
    public static void Shutdown()
    {
        if (Hook != null)
        {
            Hook.KeyDown -= OnKeyDown;
            Hook.KeyUp -= OnKeyUp;
            Hook.Dispose();
            Hook = null;
            Initialized = false;
            PressedKeys.Clear();
        }
    }

    // Handler for key press events.
    private static void OnKeyDown(Object? sender, Keys key)
    {
        // If the key was not already pressed, add it and notify subscribers.
        if (PressedKeys.Add(key))
        {
            KeysChanged?.Invoke(null, PressedKeys);
        }
    }

    // Handler for key release events.
    private static void OnKeyUp(Object? sender, Keys key)
    {
        // If the key was pressed, remove it and notify subscribers.
        if (PressedKeys.Remove(key))
        {
            KeysChanged?.Invoke(null, PressedKeys);
        }
    }
}