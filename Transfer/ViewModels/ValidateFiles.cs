﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Transfer.ViewModels
{
    public class ValidateFiles
    {
        [FileSize(30000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase File { get; set; }
    }


    public class FileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSize;

        public FileSizeAttribute(int maxSize)
        {
            _maxSize = maxSize;
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            return _maxSize > ((HttpPostedFileWrapper)value).ContentLength;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("File size should be within {0}", _maxSize);
        }
    }

    public class FileTypesAttribute : ValidationAttribute
    {
        private readonly List<string> _types;

        public FileTypesAttribute(string types)
        {
            _types = types.Split(',').ToList();
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            var fileExt = Path.GetExtension((value as HttpPostedFileWrapper).FileName).Substring(1);
            return _types.Contains(fileExt, StringComparer.OrdinalIgnoreCase);
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("Invalid file type. File Types supported are ", String.Join(", ", _types));
        }
    }
}