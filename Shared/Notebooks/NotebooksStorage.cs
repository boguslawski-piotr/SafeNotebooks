using System;
using pbXNet;

namespace SafeNotebooks
{
	public class NotebooksStorage : StorageOnFileSystem<string>
	{
		public NotebooksStorage(string id, IFileSystem fs) : base(id, fs) { }
	}
}
