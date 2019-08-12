using System;
using Xunit;

namespace Repositorch.Data.Entities
{
	public class BranchMaskTest
	{
		[Fact]
		public void Should_do_logical_and_for_masks_with_the_same_size_and_offset()
		{
			var m1 = new BranchMask() { Data = "01111", Offset = 10 };
			var m2 = new BranchMask() { Data = "11001", Offset = 10 };

			var result = BranchMask.And(m1, m2);

			Assert.Equal("101001", result.Data);
			Assert.Equal(9, result.Offset);
		}
		[Fact]
		public void Should_do_logical_or_for_masks_with_the_same_size_and_offset()
		{
			var m1 = new BranchMask() { Data = "00111001", Offset = 10 };
			var m2 = new BranchMask() { Data = "10010101", Offset = 10 };

			var result = BranchMask.Or(m1, m2);

			Assert.Equal("10111101", result.Data);
			Assert.Equal(10, result.Offset);
		}
		[Fact]
		public void Should_do_logical_xor_for_masks_with_the_same_size_and_offset()
		{
			var m1 = new BranchMask() { Data = "01111", Offset = 2 };
			var m2 = new BranchMask() { Data = "11001", Offset = 2 };

			var result = BranchMask.Xor(m1, m2);

			Assert.Equal("001011", result.Data);
			Assert.Equal(0, result.Offset);
		}
		[Fact]
		public void Should_do_logical_and_for_masks_with_different_sizes()
		{
			var m1 = new BranchMask() { Data = "101", Offset = 10 };
			var m2 = new BranchMask() { Data = "11", Offset = 10 };

			var result = BranchMask.And(m1, m2);

			Assert.Equal("1", result.Data);
			Assert.Equal(10, result.Offset);
		}
		[Fact]
		public void Should_do_logical_or_for_masks_with_different_sizes()
		{
			var m1 = new BranchMask() { Data = "1001", Offset = 10 };
			var m2 = new BranchMask() { Data = "00111001", Offset = 10 };

			var result = BranchMask.Or(m1, m2);

			Assert.Equal("10111001", result.Data);
			Assert.Equal(10, result.Offset);

			m1 = new BranchMask() { Data = "11", Offset = 0 };
			m2 = new BranchMask() { Data = "00001", Offset = 0 };

			result = BranchMask.Or(m1, m2);

			Assert.Equal("1001", result.Data);
			Assert.Equal(1, result.Offset);
		}
		[Fact]
		public void Should_do_logical_xor_for_masks_with_different_sizes()
		{
			var m1 = new BranchMask() { Data = "1001", Offset = 10 };
			var m2 = new BranchMask() { Data = "00111001", Offset = 10 };

			var result = BranchMask.Xor(m1, m2);

			Assert.Equal("000000000010101001", result.Data);
			Assert.Equal(0, result.Offset);
		}
		[Fact]
		public void Should_do_logical_and_for_masks_with_different_offsets()
		{
			var m1 = new BranchMask() { Data = "000111", Offset = 9 };
			var m2 = new BranchMask() { Data = "00111", Offset = 8 };

			var result = BranchMask.And(m1, m2);

			Assert.Equal("100001", result.Data);
			Assert.Equal(7, result.Offset);
		}
		[Fact]
		public void Should_do_logical_or_for_masks_with_different_offsets()
		{
			var m1 = new BranchMask() { Data = "00001", Offset = 9 };
			var m2 = new BranchMask() { Data = "00001", Offset = 8 };

			var result = BranchMask.Or(m1, m2);

			Assert.Equal("100011", result.Data);
			Assert.Equal(8, result.Offset);
		}
		[Fact]
		public void Should_do_logical_xor_for_masks_with_different_offsets()
		{
			var m1 = new BranchMask() { Data = "00011", Offset = 9 };
			var m2 = new BranchMask() { Data = "00011", Offset = 8 };

			var result = BranchMask.Xor(m1, m2);

			Assert.Equal("00000000100101", result.Data);
			Assert.Equal(0, result.Offset);
		}
		[Fact]
		public void Should_do_logical_and_for_masks_with_different_sizes_and_offsets()
		{
			var m1 = new BranchMask() { Data = "00111", Offset = 6 };
			var m2 = new BranchMask() { Data = "001010", Offset = 5 };

			var result = BranchMask.And(m1, m2);

			Assert.Equal("100001", result.Data);
			Assert.Equal(4, result.Offset);
		}
		[Fact]
		public void Should_do_logical_or_for_masks_with_different_sizes_and_offsets()
		{
			var m1 = new BranchMask() { Data = "00001", Offset = 6 };
			var m2 = new BranchMask() { Data = "001010", Offset = 5 };

			var result = BranchMask.Or(m1, m2);

			Assert.Equal("101011", result.Data);
			Assert.Equal(5, result.Offset);
		}
		[Fact]
		public void Should_do_logical_xor_for_masks_with_different_sizes_and_offsets()
		{
			var m1 = new BranchMask() { Data = "00111", Offset = 6 };
			var m2 = new BranchMask() { Data = "001010", Offset = 5 };

			var result = BranchMask.Xor(m1, m2);

			Assert.Equal("00000101101", result.Data);
			Assert.Equal(0, result.Offset);
		}
		[Fact]
		public void Should_shift_result_mask_when_possible()
		{
			var m1 = new BranchMask() { Data = "1100", Offset = 10 };
			var m2 = new BranchMask() { Data = "0011", Offset = 10 };

			var result = BranchMask.Or(m1, m2);
			
			Assert.Equal("1", result.Data);
			Assert.Equal(13, result.Offset);
		}
		[Fact]
		public void Should_truncate_leading_zeros()
		{
			var m1 = new BranchMask() { Data = "0010000", Offset = 0 };
			var m2 = new BranchMask() { Data = "0000100", Offset = 0 };

			var result = BranchMask.Or(m1, m2);

			Assert.Equal("00101", result.Data);
			Assert.Equal(0, result.Offset);
		}
	}
}
