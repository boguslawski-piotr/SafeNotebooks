using System;
namespace SafeNotebooks
{
	public class NotebooksException : Exception
	{
		public enum ErrorCode
		{
			PasswordNotGiven,
			BadPassword,
		}

		public ErrorCode Err;

		public NotebooksException(ErrorCode err)
		{
			Err = err;
		}
	}

}
