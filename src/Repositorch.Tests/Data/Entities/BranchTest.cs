using System;
using Xunit;

namespace Repositorch.Data.Entities
{
	public class BranchTest
	{
		[Fact]
		public void Should_combine_masks_with_the_same_size_and_offset()
		{
			var result = Branch.CombineMask("00111001", 10, "10010101", 10);

			Assert.Equal("10111101", result.mask);
			Assert.Equal(10, result.maskOffset);
		}
		[Fact]
		public void Should_combine_masks_with_different_sizes()
		{
			var result = Branch.CombineMask("1001", 10, "00111001", 10);

			Assert.Equal("10111001", result.mask);
			Assert.Equal(10, result.maskOffset);

			var result2 = Branch.CombineMask("11", 0, "00001", 0);

			Assert.Equal("1001", result2.mask);
			Assert.Equal(1, result2.maskOffset);
		}
		[Fact]
		public void Should_combine_masks_with_different_offsets()
		{
			var result = Branch.CombineMask("00001", 9, "00001", 8);

			Assert.Equal("100011", result.mask);
			Assert.Equal(8, result.maskOffset);
		}
		[Fact]
		public void Should_combine_masks_with_different_sizes_and_offsets()
		{
			var result = Branch.CombineMask("00001", 6, "001010", 5);

			Assert.Equal("101011", result.mask);
			Assert.Equal(5, result.maskOffset);
		}
		[Fact]
		public void Should_shift_combined_mask_when_possible()
		{
			var result = Branch.CombineMask("1100", 10, "0011", 10);

			Assert.Equal("1", result.mask);
			Assert.Equal(13, result.maskOffset);
		}
	}
}
