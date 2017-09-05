﻿using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.AdoNet;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	public class QueryTimeoutTests : LinqTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			//SqlServer CE does not support timeouts
			return !(dialect is MsSqlCeDialect);
		}

		protected override void Configure(Configuration configuration)
		{
			base.Configure(configuration);
			configuration.SetProperty(Environment.BatchStrategy,
									  typeof(TimeoutCatchingNonBatchingBatcherFactory).AssemblyQualifiedName);
		}


		[Test]
		public void CanSetTimeoutOnLinqQueries()
		{
			var result = (from e in db.Customers
						  where e.CompanyName == "Corp"
						  select e)
				.SetOptions(o => o.SetTimeout(17))
				.ToList();

			Assert.That(TimeoutCatchingNonBatchingBatcher.LastCommandTimeout, Is.EqualTo(17));
		}


		[Test]
		public void CanSetTimeoutOnLinqPagingQuery()
		{
			var result = (from e in db.Customers
						  where e.CompanyName == "Corp"
						  select e)
				.Skip(5).Take(5)
				.SetOptions(o => o.SetTimeout(17))
				.ToList();

			Assert.That(TimeoutCatchingNonBatchingBatcher.LastCommandTimeout, Is.EqualTo(17));
		}


		[Test]
		public void CanSetTimeoutBeforeSkipOnLinqOrderedPageQuery()
		{
			var result = (from e in db.Customers
						  orderby e.CompanyName
						  select e)
				.SetOptions(o => o.SetTimeout(17))
				.Skip(5).Take(5)
				.ToList();

			Assert.That(TimeoutCatchingNonBatchingBatcher.LastCommandTimeout, Is.EqualTo(17));
		}


		[Test]
		public void CanSetTimeoutOnLinqGroupPageQuery()
		{
			var subQuery = db.Customers
				.Where(e2 => e2.CompanyName.Contains("a")).Select(e2 => e2.CustomerId)
				.SetOptions(o => o.SetTimeout(18)); // This Timeout() should not cause trouble, and be ignored.

			var result = (from e in db.Customers
						  where subQuery.Contains(e.CustomerId)
						  group e by e.CompanyName
							  into g
							  select new { g.Key, Count = g.Count() })
				.Skip(5).Take(5)
				.SetOptions(o => o.SetTimeout(17))
				.ToList();

			Assert.That(TimeoutCatchingNonBatchingBatcher.LastCommandTimeout, Is.EqualTo(17));
		}


		public class TimeoutCatchingNonBatchingBatcher : NonBatchingBatcher
		{
			// Is there an easier way to inspect the DbCommand instead of
			// creating a custom batcher?


			public static int LastCommandTimeout;

			public TimeoutCatchingNonBatchingBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
				: base(connectionManager, interceptor)
			{
			}

			public override DbDataReader ExecuteReader(DbCommand cmd)
			{
				LastCommandTimeout = cmd.CommandTimeout;
				return base.ExecuteReader(cmd);
			}

			public override Task<DbDataReader> ExecuteReaderAsync(DbCommand cmd, CancellationToken cancellationToken)
			{
				LastCommandTimeout = cmd.CommandTimeout;
				return base.ExecuteReaderAsync(cmd, cancellationToken);
			}
		}


		public class TimeoutCatchingNonBatchingBatcherFactory : IBatcherFactory
		{
			public IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
			{
				return new TimeoutCatchingNonBatchingBatcher(connectionManager, interceptor);
			}
		}
	}
}
