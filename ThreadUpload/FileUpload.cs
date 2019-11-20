using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ThreadUpload
{
	public class FileUpload
	{
	/// <summary>
	/// Class encapsultes the Oracle Service Cloud agttachment file Upload object
	/// </summary>
		
		//	Fully Qualified File Path 
		public string path { get; set; }
		
		//	File Name derived from Path
		public string name { get; set; }
		//Content as Base64 Encoded string
		public string content { get; set; }
		//	Empty Constructor
		public FileUpload()
		{
			path = null;
			name = null;
			content = null;
		}
		public FileUpload(string _path)
		{
			path = _path;
			name = Path.GetFileName(path);
		}
		/// <summary>
		/// Read File content from the given path and convert to Base64 Encoded string
		/// </summary>
		/// <param name="path"></param>
		/// <returns>Base64 Encoded String</returns>
		public String Read(string _path)
		{

			path = _path;
			name = Path.GetFileName(path);
			if ((name != null) && (name.Length > 0))
			{
				Byte[] bytes = File.ReadAllBytes(path);
				content = Convert.ToBase64String(bytes);
			}
			return content;
		}
		public String Read()
		{
			if ((name != null) && (name.Length > 0) && File.Exists(path))
			{
				Byte[] bytes = File.ReadAllBytes(path);
				content = Convert.ToBase64String(bytes);
			}
			return content;
		}
	}
}
