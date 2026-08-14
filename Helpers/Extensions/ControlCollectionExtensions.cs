namespace HASCore.Helpers.Extensions;

public static class ControlExtension 
{
    extension (Control parent)
    {
        public IEnumerable<Control> GetAllControls()
        {
            foreach (Control child in parent.Controls)
            {
                // Return this child
                yield return child;
                
                // Return all descendants nested inside, if there are any.
                foreach (Control nested in GetAllControls(child))
                    yield return nested;                 
            }
        }
    }
}