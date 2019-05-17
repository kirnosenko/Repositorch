using Xunit;

namespace System.Collections.Generic
{
	public class SmartDictionaryTest
	{
		SmartDictionary<int, int> dic;

		[Fact]
		public void Should_set_default_value_when_key_does_not_exist()
		{
			dic = new SmartDictionary<int, int>();

			dic[1] = 10;
			dic[2] += 20;

			Assert.Equal(10, dic[1]);
			Assert.Equal(20, dic[2]);
			Assert.Equal(0, dic[3]);
		}
		[Fact]
		public void Should_use_default_value_builder()
		{
			dic = new SmartDictionary<int, int>(k => k + 1);

			dic[1] = 10;
			dic[2] += 20;

			Assert.Equal(10, dic[1]);
			Assert.Equal(23, dic[2]);
			Assert.Equal(4, dic[3]);
		}
	}
}
