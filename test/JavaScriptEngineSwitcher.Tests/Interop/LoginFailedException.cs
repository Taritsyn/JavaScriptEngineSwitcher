using System;
using System.Runtime.Serialization;

namespace JavaScriptEngineSwitcher.Tests.Interop
{
	[Serializable]
	public class LoginFailedException : Exception
	{
		private string _userName;

		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}


		public LoginFailedException()
		{ }

		public LoginFailedException(string message)
			: base(message)
		{ }

		public LoginFailedException(string message, Exception innerException)
			: base(message, innerException)
		{ }

		protected LoginFailedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info != null)
			{
				_userName = info.GetString("UserName");
			}
		}


		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			base.GetObjectData(info, context);
			info.AddValue("UserName", this._userName);
		}
	}
}