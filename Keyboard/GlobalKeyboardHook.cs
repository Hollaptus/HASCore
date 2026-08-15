namespace HASCore.Keyboard;

/// <summary>
///     Provides a global keyboard hook that monitors key presses and releases across all applications.
/// </summary>
/// <remarks>
///     This class uses a low‑level keyboard hook to capture keyboard events system‑wide.
///     It maintains a set of currently pressed keys and raises the <see cref="KeysChanged"/>
///     event whenever the set changes. The hook must be initialized once before use.
/// </remarks>
public static class GlobalKeyboardHook
{
    #region Events

    /// <summary>
    ///     Occurs when the set of currently pressed keys changes (i.e., a key is pressed or released).
    /// </summary>
    /// <remarks>
    ///     The event argument is a <see cref="HashSet{Keys}"/> containing all currently pressed keys.
    ///     Subscribers should not modify the set.
    /// </remarks>
    public static event EventHandler<HashSet<Keys>>? KeysChanged;

    #endregion

    #region Private Fields

    /// <summary>
    ///     The underlying <see cref="KeyboardHook"/> instance that provides low‑level events.
    /// </summary>
    private static KeyboardHook? _hook;

    /// <summary>
    ///     Indicates whether the hook has been successfully initialized.
    /// </summary>
    private static Boolean _initialized = false;

    /// <summary>
    ///     Set of keys currently held down (used to maintain state and avoid duplicate events).
    /// </summary>
    private static readonly HashSet<Keys> _pressedKeys = [];

    #endregion

    #region Public Methods

    /// <summary>
    ///     Determines whether the specified key is currently being held down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the key is currently pressed; otherwise, <c>false</c>.</returns>
    public static Boolean IsKeyDown(Keys key) => _pressedKeys.Contains(key);

    /// <summary>
    ///     Initializes the global keyboard hook. This method must be called once before using the hook.
    /// </summary>
    /// <remarks>
    ///     The hook automatically starts capturing keyboard events. It will be stopped when the application exits
    ///     (via <see cref="AppDomain.ProcessExit"/>). If the hook is already initialized, this call does nothing.
    /// </remarks>
    public static void Initialize()
    {
        if (_initialized) return;

        _hook = new KeyboardHook();
        _hook.KeyDown += OnKeyDown;
        _hook.KeyUp += OnKeyUp;
        _hook.Start();
        _initialized = true;

        // Ensure the hook is disposed when the application shuts down.
        AppDomain.CurrentDomain.ProcessExit += (_, _) => _hook?.Dispose();
    }

    /// <summary>
    ///     Stops and disposes the keyboard hook, clearing all internal state.
    /// </summary>
    /// <remarks>
    ///     After calling this method, the hook will no longer capture keyboard events.
    ///     To restart, call <see cref="Initialize"/> again.
    /// </remarks>
    public static void Shutdown()
    {
        if (_hook != null)
        {
            _hook.KeyDown -= OnKeyDown;
            _hook.KeyUp -= OnKeyUp;
            _hook.Dispose();
            _hook = null;
            _initialized = false;
            _pressedKeys.Clear();
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///     Handler for key press events from the underlying <see cref="KeyboardHook"/>.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="key">The key that was pressed.</param>
    private static void OnKeyDown(Object? sender, Keys key)
    {
        // If the key was not already pressed, add it and notify subscribers.
        if (_pressedKeys.Add(key))
            KeysChanged?.Invoke(null, _pressedKeys);
    }

    /// <summary>
    ///     Handler for key release events from the underlying <see cref="KeyboardHook"/>.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="key">The key that was released.</param>
    private static void OnKeyUp(Object? sender, Keys key)
    {
        // If the key was pressed, remove it and notify subscribers.
        if (_pressedKeys.Remove(key))
            KeysChanged?.Invoke(null, _pressedKeys);
    }

    #endregion
}