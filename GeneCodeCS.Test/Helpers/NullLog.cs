// 
// GeneCodeCS - Genetic programming library for code bot natural selection.
// Copyright (C) 2013 Edd Porter <genecodecs@eddporter.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see {http://www.gnu.org/licenses/}.
//  

using System;
using Common.Logging;

namespace GeneCodeCS.Test.Helpers
{
  /// <summary>
  ///   TODO: Update summary.
  /// </summary>
  public class NullLog : ILog
  {
    #region ILog Members

    public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                      Exception exception) {
    }

    public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception) {
    }

    public void Debug(Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Debug(object message, Exception exception) {
    }

    public void Debug(object message) {
    }

    public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args) {
    }

    public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args) {
    }

    public void DebugFormat(string format, Exception exception, params object[] args) {
    }

    public void DebugFormat(string format, params object[] args) {
    }

    public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                      Exception exception) {
    }

    public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception) {
    }

    public void Error(Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Error(object message, Exception exception) {
    }

    public void Error(object message) {
    }

    public void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args) {
    }

    public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args) {
    }

    public void ErrorFormat(string format, Exception exception, params object[] args) {
    }

    public void ErrorFormat(string format, params object[] args) {
    }

    public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                      Exception exception) {
    }

    public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception) {
    }

    public void Fatal(Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Fatal(object message, Exception exception) {
    }

    public void Fatal(object message) {
    }

    public void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args) {
    }

    public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args) {
    }

    public void FatalFormat(string format, Exception exception, params object[] args) {
    }

    public void FatalFormat(string format, params object[] args) {
    }

    public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                     Exception exception) {
    }

    public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception) {
    }

    public void Info(Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Info(object message, Exception exception) {
    }

    public void Info(object message) {
    }

    public void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args) {
    }

    public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args) {
    }

    public void InfoFormat(string format, Exception exception, params object[] args) {
    }

    public void InfoFormat(string format, params object[] args) {
    }

    public bool IsDebugEnabled {
      get { return false; }
    }

    public bool IsErrorEnabled {
      get { return false; }
    }

    public bool IsFatalEnabled {
      get { return false; }
    }

    public bool IsInfoEnabled {
      get { return false; }
    }

    public bool IsTraceEnabled {
      get { return false; }
    }

    public bool IsWarnEnabled {
      get { return false; }
    }

    public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                      Exception exception) {
    }

    public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception) {
    }

    public void Trace(Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Trace(object message, Exception exception) {
    }

    public void Trace(object message) {
    }

    public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args) {
    }

    public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args) {
    }

    public void TraceFormat(string format, Exception exception, params object[] args) {
    }

    public void TraceFormat(string format, params object[] args) {
    }

    public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback,
                     Exception exception) {
    }

    public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception) {
    }

    public void Warn(Action<FormatMessageHandler> formatMessageCallback) {
    }

    public void Warn(object message, Exception exception) {
    }

    public void Warn(object message) {
    }

    public void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args) {
    }

    public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args) {
    }

    public void WarnFormat(string format, Exception exception, params object[] args) {
    }

    public void WarnFormat(string format, params object[] args) {
    }

    #endregion
  }
}