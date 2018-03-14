﻿using System;

namespace Server
{
    public class FileDetails : IEquatable<FileDetails>
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }

        public FileDetails(string fileName, long fileSize)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
        }

        //public override int GetHashCode()
        //{
        //    if (FileName == null) return 0;
        //    return FileName.GetHashCode();
        //}

        public bool Equals(FileDetails other)
        {
            return this.FileName == other.FileName && this.FileSize == other.FileSize;
        }
    }
}