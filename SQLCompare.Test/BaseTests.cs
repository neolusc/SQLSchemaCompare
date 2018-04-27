﻿using System;
using Xunit.Abstractions;

namespace SQLCompare.Test
{
    public abstract class BaseTests<T> : IDisposable
    {
        protected readonly XunitLogger<T> Logger;

        protected BaseTests(ITestOutputHelper output)
        {
            Logger = new XunitLogger<T>(output);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
