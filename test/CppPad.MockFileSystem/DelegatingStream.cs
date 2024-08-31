namespace CppPad.MockFileSystem;

public class DelegatingStream(Stream baseStream) : Stream
{
    public event EventHandler? Disposing;

    public override bool CanRead => baseStream.CanRead;
    public override bool CanSeek => baseStream.CanSeek;
    public override bool CanWrite => baseStream.CanWrite;
    public override long Length => baseStream.Length;

    public override long Position
    {
        get => baseStream.Position;
        set => baseStream.Position = value;
    }

    public override void Flush()
    {
        baseStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return baseStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return baseStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        baseStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        baseStream.Write(buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            baseStream.Dispose();
            Disposing?.Invoke(this, EventArgs.Empty);
        }
        base.Dispose(disposing);
    }
}