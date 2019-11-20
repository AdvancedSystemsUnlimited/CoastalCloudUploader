using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AttachmentUpload
{
    public interface IArgService
    {
        string[] GetArgs();
        string[] SetArgs(string[] args);
    }
    public class ArgService : IArgService
    {
        private readonly IArgService _argService;
        public string[] _args { get; set; }

        public ArgService(IArgService argService)
        {
            _argService = argService;
        }

        public string[] GetArgs()
        {
            return _args;
        }

        public string[] SetArgs(string[] args)
        {
            _args = args;
            return _args;
        }
    }
}
