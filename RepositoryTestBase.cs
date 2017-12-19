using MS.WMS.Data;
using Shouldly;
using System;
using System.Reflection;
using Autofac;
using MS.WMS.Data.Impl;
using WillV;
using WillV.Domain.Uow;
using WillV.EntityFramework;
using WillV.EntityFramework.Uow;
using WillV.IocManager;
using WillV.TestBase;
using Xunit;
using Xunit.Abstractions;
using WillV.Runtime.Session;

namespace MS.WMS.Data.Tests
{
    public class RepoTestProviderBase : TestBaseWithLocalIocResolver
    {
      
        public RepoTestProviderBase()
        {

            this.Building(builder =>
            {
                builder
                    .UseWillV<StorageBootstrapper>(true)
                    .UseWillVEntityFramework()
                    .UseRepositoryRegistrarInAssembly(typeof(StorageDbContext).Assembly)
                    .UseWillVNullEventBus()
                    .UseWillVDbContextEfTransactionStrategy()
                    .UseWillVDefaultConnectionStringResolver()
                    .RegisterServices(
                    r =>
                    {
                        r.Register(context => new StorageDbContext());
                        r.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
                        r.RegisterAssemblyByConvention(typeof(WillVDbContext).Assembly);
                        //r.RegisterAssemblyByConvention(typeof(IProductInventoryRepository).Assembly);
                        r.UseBuilder(b =>
                        {
                            b.RegisterAssemblyModules();
                            b.RegisterAssemblyTypes(new[]
                                {
                                    typeof(ProductRepository).Assembly
                                })
                                .Where(t => t.Name.EndsWith("Repository")
                                            || t.Name.EndsWith("Service")
                                            || t.Name.EndsWith("Repo"))
                                .AsImplementedInterfaces()
                                .PropertiesAutowired();
                        });

                    });
            }).Ok();
        }


        //[Fact(DisplayName = "测试解析repository", Skip = "")]
        public void the_IProductRepository_MustHasValue()
        {
            var repo = this.The<IProductRepository>();
            repo.ShouldNotBeNull();
        }


      
    }

    /// <summary>
    /// repo test -Base
    /// </summary>
    public class RepositoryTestBase : RepoTestProviderBase
    {
        protected ITestOutputHelper _output;
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryTestBase"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public RepositoryTestBase(ITestOutputHelper output)

        {
            SessionMock.Set(1, 2, new[] { (long)1 }, "name");
            this._output = output;
        }

        //[Fact(DisplayName = "解析-IDbContextProvider")]
        public void the_BaseRepository()
        {
            var repo = this.The<IDbContextProvider<StorageDbContext>>();
            repo.ShouldNotBeNull();
            this._output.WriteLine(repo.GetType().Name);
        }
        /// <summary>
        /// 是否是指定类型的异常并包含指定的异常内容
        /// </summary>
        /// <typeparam name="T">
        /// 数据类型
        /// </typeparam>
        /// <param name="testCode">
        /// 编码
        /// </param>
        /// <param name="errmsg">
        /// 包含的异常内容
        /// </param>
        public void Throws<T>(Func<object> testCode, string errmsg)
            where T : Exception
        {
            var exception = Assert.Throws<T>(testCode);
            Assert.True(exception.Message.Contains(errmsg));
        }

        /// <summary>
        /// The throws.
        /// </summary>
        /// <param name="testCode">
        /// The test code.
        /// </param>
        /// <param name="errmsg">
        /// The errmsg.
        /// </param>
        /// <typeparam name="T">
        /// 数据类型
        /// </typeparam>
        public void Throws<T>(Action testCode, string errmsg)
            where T : Exception
        {
            var exception = Assert.Throws<T>(testCode);
            Assert.True(exception.Message.Contains(errmsg));
        }

        public string CreateTempNum()
        {
            return new Random().Next(0, 9999).ToString().PadLeft(4, '0') + DateTime.Now.ToString("MMddHHmmss");
        }
    }
}
