namespace patter_pal.Util
{
    public static class SemaphoreSlimExtensions
    {
        public static void SafeRelease(this SemaphoreSlim semaphore)
        {
            if (semaphore.CurrentCount == 0)
            {
                semaphore.Release();
            }
        }
    }
}
