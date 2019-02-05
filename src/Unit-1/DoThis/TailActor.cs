using System.IO;
using System.Text;
using Akka.Actor;

namespace WinTail
{
    public class TailActor: UntypedActor
    {
        private readonly IActorRef _reporterActor;
        private readonly string _filePath;
        private FileObserver _observer;
        private Stream _fileStream;
        private StreamReader _fileStreamReader;

        #region Message types

        public class FileWrite
        {
            public string FileName { get; }

            public FileWrite(string fileName)
            {
                FileName = fileName;
            }
        }

        public class FileError
        {
            public string FileName { get; }
            public string Reason { get; }

            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }
        }

        public class InitialRead
        {
            public string FileName { get; private set; }
            public string Text { get; private set; }

            /// <inheritdoc />
            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }
        }

        #endregion

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;

            
        }

        /// <inheritdoc />
        protected override void PreStart()
        {
            // start watching file for changes
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            // open the file stream with shared read/write permissions
            // (so file can be written to while open)
            _fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            // read the initial contents of the file and send it to console as first msg
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        /// <inheritdoc />
        protected override void PostStop()
        {
            _observer.Dispose();
            _observer = null;
            _fileStreamReader.Close();
            _fileStreamReader.Dispose();
            base.PostStop();
        }

        /// <inheritdoc />
        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                // move file cursor forward
                // pull results from cursor to end of file and write to output
                // (this is assuming a log file type format that is append-only)
                var text = _fileStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }

            }
            else if (message is FileError)
            {
                var fe = message as FileError;
                _reporterActor.Tell(string.Format("Tail error: {0}", fe.Reason));
            }
            else if (message is InitialRead)
            {
                var ir = message as InitialRead;
                _reporterActor.Tell(ir.Text);
            }
        }
    }
}