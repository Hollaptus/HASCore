// Declaring use of this library because of the attributes
// on the partial functions that are being imported from system.
using System.Runtime.InteropServices;

namespace HASCore.Helpers;

public partial class WindowInterop
{
    /// Description
    /// <summary>
    ///     Function for finding top-level window through OS-level methods.
    /// </summary>
    /// <remarks>
    ///     Retrieves a handle to the top-level window whose class name and window name match the specified Strings. 
    /// </remarks>
    /// 
    /// Parameters
    /// <param name="lpClassName">
    ///     <para>The class name or a class atom created by a previous call to the <c>RegisterClass</c> or <c>RegisterClassEx</c> function.</para>
    ///     <para>If lpClassName points to a <see cref="String"/>, it specifies the window class name.</para>
    ///     <para>If lpClassName is <see cref="null"/>, it finds any window whose title matches the <paramref name="lpWindowName"/></para>
    /// </param>
    /// <param name="lpWindowName">
    ///     The window name (the window's title). If this parameter is <see cref="null"/>, all window names match.
    /// </param>
    /// 
    /// Return value
    /// <returns>
    ///     If the function succeeds, the return value is a handle to the window. 
    ///     If the function fails, the return value is <see cref="null"/>.
    /// </returns>
    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr FindWindow(String? lpClassName, String lpWindowName);
    
    /// Description
    /// <summary>
    ///     Function for getting the active window through OS-level methods.
    /// </summary>
    /// <remarks>
    ///     Retrieves a handle to the foreground window (the window with which the user is currently working).
    /// </remarks>
    /// 
    /// Return value
    /// <returns>
    ///     <para>The return value is a handle to the foreground window.</para> 
    ///     <para>The foreground window can be NULL in certain circumstances, such as when a window is losing activation.</para>
    /// </returns>
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial IntPtr GetForegroundWindow();

    /// Description
    /// <summary>
    ///     Function for checking if the specified window is the foreground window.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="windowTitle">Title of the window that we need to find.</param>
    /// 
    /// Return value
    /// <returns>
    ///     <para>Returns <see cref="true"/> if the window specified by its title is, in fact, the foreground window.</para>
    ///     <para>Otherwise, returns <see cref="false"/>.</para> 
    /// </returns>
    internal static Boolean IsForegroundWindow(String? windowTitle, IntPtr? foregroundWindow = null)
    {
        // If the window title is empty, then there is no window to find.
        if (String.IsNullOrEmpty(windowTitle)) return false;

        // Trying to get a window by its title.
        IntPtr window = FindWindow(null, windowTitle);
        
        // The window hasn't been found, so we return 'false'.
        if (window == IntPtr.Zero) return false;

        // Otherwise we return the result of comparison between
        // the window pointer that has been found and the pointer of
        // a foreground window we try to compare to.
        return foregroundWindow == window;
    }
}
