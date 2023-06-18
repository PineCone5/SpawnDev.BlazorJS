﻿using Microsoft.JSInterop;
using System.Dynamic;

namespace SpawnDev.BlazorJS.JSObjects {
    // https://developer.mozilla.org/en-US/docs/Web/API/File
    public class FileSystemHandle : JSObject {
        public FileSystemHandle(IJSInProcessObjectReference _ref) : base(_ref) { }
        public string Name => JSRef.Get<string>("name");
        public string Kind => JSRef.Get<string>("kind");
        public bool IsSameEntry(FileSystemHandle fsHandle) => JSRef.Call<bool>("isSameEntry", fsHandle);

        public FileSystemHandle ResolveType() => Kind == "directory" ? JS.ReturnMe<FileSystemDirectoryHandle>(this) : JS.ReturnMe<FileSystemFileHandle>(this);
        public FileSystemDirectoryHandle? ToFileSystemDirectoryHandle() => Kind == "directory" ? JS.ReturnMe<FileSystemDirectoryHandle>(this) : null;
        public FileSystemFileHandle? ToFileSystemFileHandle() => Kind == "file" ? JS.ReturnMe<FileSystemFileHandle>(this) : null;

        public async Task<string> GetReadWritePermissions() {
            if (await IsWritable()) return "rw";
            if (await IsReadable()) return "r";
            return "";
        }

        public async Task<bool> IsWritable() {
            try {
                return await VerifyPermission(true, false);
            }
            catch { }
            return false;
        }

        public async Task<bool> IsReadable() {
            try {
                return await VerifyPermission(false, false);
            }
            catch { }
            return false;
        }

        public async Task<string> QueryPermission(bool writePermission = false) {
            dynamic options = new ExpandoObject();
            options.mode = writePermission ? "readwrite" : "read";
            return await JSRef.CallAsync<string>("queryPermission", (object)options);
        }

        public async Task<string> RequestPermission(bool writePermission = false) {
            dynamic options = new ExpandoObject();
            options.mode = writePermission ? "readwrite" : "read";
            return await JSRef.CallAsync<string>("requestPermission", (object)options);
        }

        public async Task<bool> VerifyPermission(bool readWrite = true, bool askIfNeeded = true) {
            dynamic options = new ExpandoObject();
            options.mode = readWrite ? "readwrite" : "read";
            if ((await JSRef.CallAsync<string>("queryPermission", (object)options)) == "granted") return true;
            if (askIfNeeded && (await JSRef.CallAsync<string>("requestPermission", (object)options)) == "granted") return true;
            return false;
        }
    }
}
