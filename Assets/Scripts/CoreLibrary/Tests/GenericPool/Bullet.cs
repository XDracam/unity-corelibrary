using CoreLibrary.Pool;

namespace CoreLibrary.Tests.GenericPool
{
	/// <summary>
	/// Author: Cameron Reuschel
	/// </summary>
	public class Bullet : Reusable {

		public override void ResetForReuse()
		{
			// nothing
		}

		public override void AfterReuse()
		{
			// nothing
		}

		public override void ReuseRequested()
		{
			FreeForReuse();
		}
	}
}
