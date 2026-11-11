namespace HASCore.Helpers;

/// Description
/// <summary>
///		Provides shared, pre-scaled icon instances for UI controls.
/// </summary>
///
/// Additional information
/// <remarks>
///		Icons are created once and reused across forms. The .NET 11
///		<c>VisualStylesMode.Net11</c> renderer no longer scales
///		<see cref="Button.Image"/> to fit the control, so we pre-scale
///		bitmaps here to keep a consistent look everywhere.
///		Instances live for the whole application and are intentionally
///		not disposed.
/// </remarks>
internal static class Icons
{
	/// Description
	/// <summary>
	///		Caches pre-scaled bitmaps keyed by resource name and size so each
	///		combination is scaled only once.
	/// </summary>
	private static readonly Dictionary<(String, Size), Bitmap?> _cache = [];

	/// Description
	/// <summary>
	///		Gets the application icon for <see cref="Form.Icon"/>. Not cached
	///		or scaled: Windows picks the appropriate resolution from the
	///		multi-size .ico.
	/// </summary>
	internal static Icon? App => Resources.Icons.App;

	/// Description
	/// <summary>
	///		Gets a pre-scaled <see cref="Bitmap"/> of the reload button icon
	///		at the specified square size.
	/// </summary>
	///
	/// Parameters
	/// <param name="size">The width and height of the bitmap in pixels.</param>
	internal static Bitmap? ReloadButton(Int32 size) => ReloadButton(new Size(size, size));

	/// Description
	/// <summary>
	///		Gets a pre-scaled <see cref="Bitmap"/> of the reload button icon
	///		at the specified size.
	/// </summary>
	///
	/// Parameters
	/// <param name="size">The width and height of the bitmap in pixels.</param>
	internal static Bitmap? ReloadButton(Size size) => Scale("ReloadButton", Resources.Icons.ReloadButton, size);

	/// Description
	/// <summary>
	///		Scales an image to the requested size and caches the result.
	/// </summary>
	///
	/// Parameters
	/// <param name="key">The cache key identifying the source image.</param>
	/// <param name="source">The source image to scale, or <see langword="null"/>.</param>
	/// <param name="size">The target width and height in pixels.</param>
	///
	/// Return value
	/// <returns>
	///		A scaled <see cref="Bitmap"/>, or <see langword="null"/> if <paramref name="source"/> is <see langword="null"/>.
	/// </returns>
	private static Bitmap? Scale(String key, Image? source, Size size)
	{
		// Return the cached bitmap if we already scaled this image at this size.
		if (_cache.TryGetValue((key, size), out Bitmap? cached))
			return cached;
		
		// Scaling a null source yields a null result; otherwise create a new
		// bitmap at the requested dimensions.
		Bitmap? scaled = source is null ? null : new Bitmap(source, size);

		// Cache the scaled bitmap (including null results) so we never scale
		// the same image at the same size more than once.
		_cache[(key, size)] = scaled;

		return scaled;
	}
}