using System;

namespace hexreader.EmbroideryHandlers
{
	/// <summary>
	/// Summary description for IEmbroideryHandler.
	/// </summary>
	interface IEmbroideryHandler
	{
		Embroidery GetEmbroidery(string filename);
	}
}
