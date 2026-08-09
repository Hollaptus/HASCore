namespace HASCore.Helpers;

/// Description
/// <summary>
/// Helper class for various functions used in other parts of the program, such as:
/// <list type="bullet|number|table"> 
///     <item> 
///         <term>Working with windows</term>
///         <description>Checking if a certain window is the foreground window</description>
///     </item>
///     <item>
///         <term>Conversion</term>
///         <description>Converting between different object types.</description>
///     </item>
/// </list>
/// </summary>
public class Files
{
    /// Description
    /// <summary>
    ///     Function to invoke OS-level file manager for user to select a XML file path.
    /// </summary>
    /// 
    /// Return value
    /// <returns>
    ///     Path to a file that has been selected in the dialog box.
    /// </returns>
    internal static String UserGetXMLLocation()
    {
        SaveFileDialog dialog = new () { Filter = "XML file containing keys and sounds|*.xml" };
        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : String.Empty;            
    }
    
    /// Description
    /// <summary>
    ///     Function to clean up the specified file name from illegal characters.
    /// </summary>
    /// <param name="fileName">File name that needs to be validated and cleaned up.</param>
    /// <returns>A string containing the file name with removed illegal characters.</returns>
    internal static String CleanFileName(String fileName) 
        => Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), String.Empty));
}
