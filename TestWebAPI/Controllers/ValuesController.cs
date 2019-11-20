using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TestWebAPI.Controllers
{
	public class UploadFile
	{
		private readonly ILogger<UploadFile> _logger;
		public string filename { get; set; }
		public string data { get; set; }
		public UploadFile(ILogger<UploadFile> logger)
		{
			_logger = logger;
		}

		public long Save(string folderPath)
		{
			string path = $@"{AppContext.BaseDirectory}{folderPath}\{filename}";
			if ((data.Length > 0) && (filename.Length > 0))
			{
				Byte[] bytes = Convert.FromBase64String(data); 
				File.WriteAllBytes(path, bytes);
			}
			return data.Length;
		}
	}

	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		// GET api/values
		[HttpGet]
		public ActionResult<IEnumerable<string>> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult<string> Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPatch]
		public void Patch([FromBody] string value)
		{
		}

		// POST api/values
		[HttpPatch("{Id}")]
		public void Patch(int Id, [FromBody] UploadFile uploadFile)
		{

			string folder = "attachments";
			uploadFile.Save(folder);
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
