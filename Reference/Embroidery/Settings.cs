using System;
using System.Data;
using System.IO;
namespace Antropoid.Embroidery.Application
{
	/// <summary>
	/// Summary description for Settings.
	/// </summary>
	public class Settings
	{
		
		private static int _previewsize=200;
		/// <summary>
		/// Gets or sets the number of pixels that 5cm represents on the screen
		/// </summary>
		public static int PreviewSize
		{
			get	{
				if (Loaded==false)
					Load();
				return _previewsize;
				}
			set	{
				_previewsize=value;
				}
		}

		private static bool Loaded=false;

		public static void Save()
		{
			DataSet ds = new DataSet("EmbroiderySettings");
			ds.Tables.Add("Settings");
			ds.Tables[0].Columns.Add("PreviewSize",System.Type.GetType("System.String"));
			
			object[] values=new object[1];
			values[0]=_previewsize.ToString();
			
			ds.Tables[0].Rows.Add(values);
			ds.WriteXml(System.Windows.Forms.Application.StartupPath + "\\settings.xml",XmlWriteMode.IgnoreSchema);
		}

		public static void Load()
		{
			if(File.Exists(System.Windows.Forms.Application.StartupPath + "\\settings.xml"))
			{
				DataSet ds = new DataSet();
				ds.ReadXml(System.Windows.Forms.Application.StartupPath + "\\settings.xml");
				PreviewSize= Convert.ToInt32(ds.Tables[0].Rows[0]["PreviewSize"].ToString());
			}
		}
	}
}
