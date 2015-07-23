using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace msbuild_lesschat
{
    public class LesschatLogger : Logger
    {
        private readonly StringBuilder _sb;
        private int _indent;
        private LesschatClient _client;
        private readonly IList<Task<LesschatIncomingMessageResponse>> _tasks;

        private readonly Stopwatch _stopwatch;
        private int _warnings;
        private int _errors;

        public LesschatLogger() : base()
        {
            _sb = new StringBuilder();
            _indent = 0;
            _client = null;
            _tasks = new List<Task<LesschatIncomingMessageResponse>>();

            _stopwatch = new Stopwatch();
            _warnings = 0;
            _errors = 0;
        }

        public override void Initialize(IEventSource eventSource)
        {
            if (Parameters == null)
            {
                throw new LoggerException("Parameter was not specified.");
            }

            var parameters = Parameters.Split(';');
            if (parameters.Length <= 0)
            {
                throw new LoggerException("Parameter was not specified.");
            }

            var webhook = parameters[0].Trim();
            if (string.IsNullOrWhiteSpace(webhook))
            {
                throw new LoggerException("Lesschat incoming message webhook URL was not specified.");
            }

            Uri webhookUri;
            if (!Uri.TryCreate(webhook, UriKind.Absolute, out webhookUri))
            {
                if (!Uri.TryCreate(string.Format("https://hook.lesschat.com/incoming/{0}", webhook), UriKind.Absolute, out webhookUri))
                {
                    throw new LoggerException(string.Format("Invalid incoming message webhook URL ({0}).", webhook));
                }
            }
            _client = new LesschatClient(webhookUri);

            eventSource.BuildStarted += (sender, e) =>
            {
                _stopwatch.Start();

                if (IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                {
                    var response = _client.Send(new LesschatIncomingMessage()
                    {
                        Attachment = new LesschatIncomingMessage.LesschatIncomingMessageAttachment()
                        {
                            Fallback = e.Message,
                            Color = LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Message,
                            Text = string.Format("{0} @{1}", e.Message, e.Timestamp.ToString())
                        }
                    });
                    Console.WriteLine("BuildStarted");
                    Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                }
            };

            eventSource.BuildFinished += (sender, e) =>
            {
                _stopwatch.Stop();
                if (IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                {
                    var message = new LesschatIncomingMessage();
                    message.Attachment.Fallback = e.Message;
                    message.Attachment.Text = string.Format("{0} @{1}", e.Message, e.Timestamp.ToString());
                    if (_errors > 0)
                    {
                        message.Attachment.Color = LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Error;
                    }
                    else if (_warnings > 0)
                    {
                        message.Attachment.Color = LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Warning;
                    }
                    else
                    {
                        message.Attachment.Color = LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Success;
                    }
                    message.Attachment.Fields.Add(new LesschatIncomingMessage.LesschatIncomingMessageAttachmentField("Warning(s)", _warnings.ToString(), true));
                    message.Attachment.Fields.Add(new LesschatIncomingMessage.LesschatIncomingMessageAttachmentField("Error(s)", _errors.ToString(), true));
                    message.Attachment.Fields.Add(new LesschatIncomingMessage.LesschatIncomingMessageAttachmentField("Time Elapsed", _stopwatch.Elapsed.ToString(), true));

                    var response = _client.Send(message);
                    Console.WriteLine("BuildFinished");
                    Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                }
            };

            eventSource.ProjectStarted += (sender, e) =>
            {
                if (IsVerbosityAtLeast(LoggerVerbosity.Normal))
                {
                    var response = _client.Send(new LesschatIncomingMessage()
                    {
                        Attachment = new LesschatIncomingMessage.LesschatIncomingMessageAttachment()
                        {
                            Fallback = e.Message,
                            Color = LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Message,
                            Text = e.Message
                        }
                    });
                    Console.WriteLine("ProjectStarted");
                    Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                }
            };

            eventSource.ProjectFinished += (sender, e) =>
            {
                if (IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                {
                    var response = _client.Send(new LesschatIncomingMessage()
                    {
                        Attachment = new LesschatIncomingMessage.LesschatIncomingMessageAttachment()
                        {
                            Fallback = e.Message,
                            Color = e.Succeeded ? LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Success : LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Error,
                            Text = e.Message
                        }
                    });
                    Console.WriteLine("ProjectFinished");
                    Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                }
            };

            eventSource.TaskStarted += (sender, e) =>
            {
            };

            eventSource.TaskFinished += (sender, e) =>
            {
            };

            eventSource.TargetStarted += (sender, e) =>
            {
            };

            eventSource.TargetFinished += (sender, e) =>
            {
            };

            eventSource.ErrorRaised += (sender, e) =>
            {
                _errors++;

                if (IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                {
                    var message = new LesschatIncomingMessage();
                    message.Attachment.Fallback = e.Message;
                    message.Attachment.Text = string.Format("Error: {0}", e.Message);
                    message.Attachment.Color = LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Error;
                    if (!string.IsNullOrWhiteSpace(e.File) && e.LineNumber > 0 && e.ColumnNumber > 0 && !string.IsNullOrWhiteSpace(e.Code))
                    {
                        message.Attachment.Fields.Add(new LesschatIncomingMessage.LesschatIncomingMessageAttachmentField("File", e.File, false));
                        message.Attachment.Fields.Add(new LesschatIncomingMessage.LesschatIncomingMessageAttachmentField("Location", string.Format("Ln {0}, Col {1}", e.LineNumber, e.ColumnNumber), false));
                    }

                    var response = _client.Send(message);
                    Console.WriteLine("ErrorRaised");
                    Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                }
            };

            eventSource.WarningRaised += (sender, e) =>
            {
                _warnings++;

                if (IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                {
                    var message = new LesschatIncomingMessage();
                    message.Attachment.Fallback = e.Message;
                    message.Attachment.Text = string.Format("Warning: {0}", e.Message);
                    message.Attachment.Color = LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Warning;
                    if (!string.IsNullOrWhiteSpace(e.File) && e.LineNumber > 0 && e.ColumnNumber > 0 && !string.IsNullOrWhiteSpace(e.Code))
                    {
                        message.Attachment.Fields.Add(new LesschatIncomingMessage.LesschatIncomingMessageAttachmentField("File", e.File, false));
                        message.Attachment.Fields.Add(new LesschatIncomingMessage.LesschatIncomingMessageAttachmentField("Location", string.Format("Ln {0}, Col {1}", e.LineNumber, e.ColumnNumber), false));
                    }

                    var response = _client.Send(message);
                    Console.WriteLine("WarningRaised");
                    Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                }
            };

            eventSource.MessageRaised += (sender, e) =>
            {
                if ((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                    || (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal))
                    || (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed)))
                {
                    var message = new LesschatIncomingMessage();
                    message.Attachment.Fallback = e.Message;
                    message.Attachment.Text = string.Format("Message: {0}", e.Message);
                    message.Attachment.Color = LesschatIncomingMessage.LesschatIncomingMessageAttachmentColors.Message;
                    if (!string.IsNullOrWhiteSpace(e.File) && e.LineNumber > 0 && e.ColumnNumber > 0 && !string.IsNullOrWhiteSpace(e.Code))
                    {
                        message.Attachment.Fields.Add(new LesschatIncomingMessage.LesschatIncomingMessageAttachmentField("File", e.File, false));
                        message.Attachment.Fields.Add(new LesschatIncomingMessage.LesschatIncomingMessageAttachmentField("Location", string.Format("Ln {0}, Col {1}", e.LineNumber, e.ColumnNumber), false));
                    }

                    var response = _client.Send(message);
                    Console.WriteLine("MessageRaised");
                    Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                }
            };
        }

        private string GenerateMessage(string line, BuildEventArgs e, string category)
        {
            var title = string.Compare(e.SenderName, "MSBuild", true) == 0 ? line : string.Format("{0}: {1}", e.SenderName, line);
            var result = string.Empty;
            for (int i = _indent; i > 0; i--)
            {
                result += "    ";
            }
            result += category + " > ";
            result += title;
            result += e.Message;
            return result;
        }

        private void OutputMessages(string message)
        {
            Console.WriteLine(message);

            //var response = _client.SendAsync(message).Result;
            //Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
        }

        public override void Shutdown()
        {
            //OutputMessages(_sb.ToString());

            

            //System.Threading.Tasks.Task.WhenAll(_tasks).Wait();

            base.Shutdown();
        }
    }
}
