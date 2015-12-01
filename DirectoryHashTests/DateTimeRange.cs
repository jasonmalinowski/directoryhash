using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DirectoryHash.Tests
{
    internal struct DateTimeRange
    {
        private readonly DateTime _startTimeUtc;
        private readonly DateTime _endTimeUtc;

        private DateTimeRange(DateTime startTimeUtc, DateTime endTimeUtc)
        {
            _startTimeUtc = startTimeUtc;
            _endTimeUtc = endTimeUtc;
        }

        /// <summary>
        /// Creates a <see cref="DateTimeRange"/> with start and end times surrounding the execution of the action.
        /// </summary>
        public static DateTimeRange CreateSurrounding(Action a)
        {
            WaitForFileTimeChange();

            var start = DateTime.UtcNow;
            WaitForFileTimeChange();
            a();
            WaitForFileTimeChange();
            var end = DateTime.UtcNow;

            WaitForFileTimeChange();

            return new DateTimeRange(start, end);
        }

        public void AssertContains(DateTime dateTime)
        {
            Assert.True(_startTimeUtc < dateTime);
            Assert.True(dateTime < _endTimeUtc);
        }

        /// <summary>
        /// Waits until the filesystem time changes. This ensures that tests trying to assert
        /// that we correctly pick up file changes don't fail to pick things up because things
        /// happened "instantly".
        /// </summary>
        private static void WaitForFileTimeChange()
        {
            var now = DateTime.UtcNow.ToFileTimeUtc();

            while (now == DateTime.UtcNow.ToFileTimeUtc()) ;
        }
    }
}
