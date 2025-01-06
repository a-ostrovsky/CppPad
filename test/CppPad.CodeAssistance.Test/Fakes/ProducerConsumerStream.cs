#region

using System.Collections.Concurrent;

#endregion

namespace CppPad.CodeAssistance.Test.Fakes;

public class ProducerConsumerStream : Stream
{
    private readonly BlockingCollection<byte[]> _bufferQueue = new();
    private byte[]? _currentBuffer;
    private int _currentBufferOffset;
    private bool _isCompleted;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public void Complete()
    {
        _isCompleted = true;
        _bufferQueue.CompleteAdding();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            Complete();
            _bufferQueue.Dispose();
        }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        var data = new byte[count];
        Array.Copy(buffer, offset, data, 0, count);
        _bufferQueue.Add(data);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        while (true)
        {
            if (_currentBuffer != null && _currentBufferOffset < _currentBuffer.Length)
            {
                var bytesToRead = Math.Min(count, _currentBuffer.Length - _currentBufferOffset);
                Array.Copy(_currentBuffer, _currentBufferOffset, buffer, offset, bytesToRead);
                _currentBufferOffset += bytesToRead;
                return bytesToRead;
            }

            if (_isCompleted && _bufferQueue.Count == 0)
            {
                return 0;
            }

            if (_bufferQueue.TryTake(out _currentBuffer, Timeout.Infinite))
            {
                _currentBufferOffset = 0;
            }
            else
            {
                return 0;
            }
        }
    }

    public override void Flush()
    {
        // No buffering, so nothing to flush
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }
}
