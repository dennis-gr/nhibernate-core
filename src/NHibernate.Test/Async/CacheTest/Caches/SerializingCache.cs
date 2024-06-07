﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NHibernate.Cache;

namespace NHibernate.Test.CacheTest.Caches
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class SerializingCache : CacheBase
	{

		public override Task<object> GetAsync(object key, CancellationToken cancellationToken)
		{
			try
			{
				return Task.FromResult<object>(_hashtable[key]);
			}
			catch (System.Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public override Task PutAsync(object key, object value, CancellationToken cancellationToken)
		{
			try
			{
				_hashtable[key] = value;
				return Task.CompletedTask;
			}
			catch (System.Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public override Task ClearAsync(CancellationToken cancellationToken)
		{
			try
			{
				_hashtable.Clear();
				return Task.CompletedTask;
			}
			catch (System.Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public override Task<object> LockAsync(object key, CancellationToken cancellationToken)
		{
			// local cache, no need to actually lock.
			return Task.FromResult<object>(null);
		}

		public override Task UnlockAsync(object key, object lockValue, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
			// local cache, no need to actually lock.
		}
	}
}
